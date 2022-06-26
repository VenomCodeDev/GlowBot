using System;
using System.Data.SQLite;
using System.Threading.Channels;
using System.Xml;
using System.Xml.Serialization;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;

using GlowBot.Data;
using GlowBot.Data.Entities;
using GlowBot.SlashCommands;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using Formatting = Newtonsoft.Json.Formatting;

namespace GlowBot;

internal class Program
{
    static void Main(string[] args)
    {
        try
        {
            MainAsync().GetAwaiter().GetResult();
        }
        catch ( Exception ex )
        {
            Log($"Crash: { ex }", ConsoleColor.Red );
            throw;
        }
    }

    async static Task MainAsync( )
    {
        string json = string.Empty;
        BinPath = Directory.GetCurrentDirectory( ) + $"\\bin\\";
        
        Data = ConfigData.Default;

        if ( !File.Exists( BinPath + ConfigFile ) )
        {
            json = JsonConvert.SerializeObject(Data, Formatting.Indented);
            await File.WriteAllTextAsync( BinPath + ConfigFile, json );
        }
        else
        {
            json = await File.ReadAllTextAsync( BinPath + ConfigFile );
            Data = JsonConvert.DeserializeObject<ConfigData>( json );
        }

        if ( !File.Exists( BinPath + TokenFile ) )
        {
            Log("Please make \\bin\\token.txt!", ConsoleColor.Red );
            return;
        }
        if ( Data is null )
        {
            Log( $"Please fix the \\bin\\config.json file...", ConsoleColor.Red );
            return;
        }

        string botToken = await File.ReadAllTextAsync( BinPath + TokenFile );

        Database.Init( );

        DiscordConfiguration conf = new DiscordConfiguration( )
        {
            TokenType = TokenType.Bot,
            Token = botToken,
            MinimumLogLevel = LogLevel.Warning,
            Intents = DiscordIntents.AllUnprivileged | DiscordIntents.Guilds,
        };
        _discord = new DiscordClient( conf );

        _discord.GuildAvailable += OnGuildAvailable;
        _discord.VoiceStateUpdated += OnVoiceStateUpdated;
        
        await _discord.ConnectAsync( );

        SlashCommandsExtension slashCmds = _discord.UseSlashCommands( );
        slashCmds.RegisterCommands<SlashCommander>( Data!.GUILD_MASTER_ID );

        while ( !IsShutdown )
        {
            await Task.Delay( 3000 );
        }
        Log( $"Shutting down...", ConsoleColor.Yellow );
        await _discord.DisconnectAsync( );
        await Task.Delay( 250 );
        _discord.Dispose( );
        await Task.Delay( 1000 );
        Log( $"Shutdown Complete!", ConsoleColor.Red );
    }

    public static bool IsShutdown = false;
    
    async private static Task OnVoiceStateUpdated( DiscordClient sender, VoiceStateUpdateEventArgs e )
    {
        GuildData guildData = Database.GetDBGuild( e.Guild );

        if ( e.After?.Channel?.Id is not null )
        {
            GuildUserData userData = Database.GetDBUser( (DiscordMember)e.User );
            if ( ( DateTime.Now - userData.JoinDate ).TotalSeconds <= 30 )
            {
                Database.SaveDBUser( userData );
            }
        }
        
        if ( e.After?.Channel?.Id is not null && e.After.Channel.Id == guildData.ServerVC_NewVC )
        {
            DiscordMember member = (DiscordMember)e.User;
            GuildUserData userData = Database.GetDBUser( member );
            
            if ( ( DateTime.Now - userData.LastNewVCTime ).TotalSeconds < 120 )
            {
                await member.ModifyAsync( x => x.VoiceChannel = null );
                return;
            }

            DiscordChannel channel = await e.Guild.CreateChannelAsync( $"{userData.Nickname}'s Hangout", ChannelType.Voice, position: 10 );
            await member.ModifyAsync( x => x.VoiceChannel = channel );
            DiscordOverwriteBuilder overwriteBuilder = new DiscordOverwriteBuilder( member )
            {
                Allowed = Permissions.ManageChannels,
            };

            List<DiscordOverwriteBuilder> permOverwrites = new List<DiscordOverwriteBuilder>
            {
                overwriteBuilder,
            };

            await channel.ModifyAsync( x => x.PermissionOverwrites = permOverwrites );

            TempVoiceChannels.Add( channel.Id );
            
            userData.LastNewVCTime = DateTime.Now;
            Database.SaveDBUser( userData );

            Log( $"Created TMP-VC for {userData.Nickname}!", ConsoleColor.Cyan );
        }
        
        if ( e.Before?.Channel?.Id is null )
        {
            return;
        }

        if ( TempVoiceChannels.Contains( e.Before.Channel.Id ) )
        {
            if ( e.Before.Channel.Users.Count <= 0 )
            {
                await e.Before.Channel.DeleteAsync( );
                TempVoiceChannels.Remove( e.Before.Channel.Id );
            }
        }
    }
    
    private static async Task OnGuildAvailable( DiscordClient sender, GuildCreateEventArgs e )
    {
        GuildData guildData = Database.GetDBGuild( e.Guild );
        Log( $"Guild '{guildData.Nickname}' has come online!", ConsoleColor.Green );
        Database.SaveDBGuild( guildData );
    }

    public static void Log( string msg, ConsoleColor color )
    {
        ConsoleColor oldColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine( msg );
        Console.ForegroundColor = oldColor;
    }

    public static List<ulong> TempVoiceChannels = new List<ulong>( );

    public static string BinPath { get; private set; }
    
    private static DiscordClient? _discord;
    public static ConfigData? Data { get; private set; }

    private const string TokenFile = "token.txt";
    private const string ConfigFile = "config.json";
}