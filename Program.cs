using System;
using System.Data.SQLite;
using System.Xml;
using System.Xml.Serialization;

using DSharpPlus;
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
            MinimumLogLevel = LogLevel.Error,
        };
        _discord = new DiscordClient( conf );

        _discord.GuildAvailable += OnGuildAvailable;
        
        await _discord.ConnectAsync( );

        SlashCommandsExtension slashCmds = _discord.UseSlashCommands( );
        slashCmds.RegisterCommands<SlashCommander>( Data.ID_GUILD_MASTERS );

        await Task.Delay( -1 );
    }
    private static async Task OnGuildAvailable( DiscordClient sender, GuildCreateEventArgs e )
    {
        //Database.InitDBGuild( e.Guild.Id );
    }

    public static void Log( string msg, ConsoleColor color )
    {
        ConsoleColor oldColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine( msg );
        Console.ForegroundColor = oldColor;
    }

    public static string BinPath { get; private set; }
    
    private static DiscordClient? _discord;
    public static ConfigData? Data { get; private set; }

    private const string TokenFile = "token.txt";
    private const string ConfigFile = "config.json";
}