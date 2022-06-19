using System;

using DSharpPlus;
using DSharpPlus.SlashCommands;

using GlowBot.Data;
using GlowBot.SlashCommands;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace GlowBot;

internal class Program
{
    static void Main(string[] args)
    {
        MainAsync().GetAwaiter().GetResult();
    }

    async static Task MainAsync( )
    {
        string json = string.Empty;
        string prePath = Directory.GetCurrentDirectory( ) + $"\\bin\\";
        
        data = ConfigData.Default;

        if ( !File.Exists( prePath + ConfigFile ) )
        {
            json = JsonConvert.SerializeObject(data, Formatting.Indented);
            await File.WriteAllTextAsync( prePath + ConfigFile, json );
        }
        else
        {
            json = await File.ReadAllTextAsync( prePath + ConfigFile );
            data = JsonConvert.DeserializeObject<ConfigData>( json );
        }

        if ( !File.Exists( prePath + TokenFile ) )
        {
            Log("Please make \\bin\\token.txt!", ConsoleColor.Red );
            return;
        }
        if ( data is null )
        {
            Log( $"Please fix the \\bin\\config.json file...", ConsoleColor.Red );
            return;
        }

        string botToken = await File.ReadAllTextAsync( prePath + TokenFile );
        
        DiscordConfiguration conf = new DiscordConfiguration( )
        {
            TokenType = TokenType.Bot,
            Token = botToken,
        };
        disc = new DiscordClient( conf );
        await disc.ConnectAsync( );

        SlashCommandsExtension slashCmds = disc.UseSlashCommands( );
        slashCmds.RegisterCommands<SlashCommander>( data.ID_GUILD_MASTERS );

        await Task.Delay( -1 );
    }

    public static void Log( string msg, ConsoleColor color )
    {
        ConsoleColor oldColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine( msg );
        Console.ForegroundColor = oldColor;
    }

    private static DiscordClient disc;
    public static ConfigData? data { get; private set; }

    private const string TokenFile = "token.txt";
    private const string ConfigFile = "config.json";
}