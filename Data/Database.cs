using System.Data;
using System.Data.SQLite;

using DSharpPlus.Entities;

using GlowBot.Data.Entities;

namespace GlowBot.Data
{
    public static class Database
    {
        public static void Init( )
        {
            DatabaseSource = $"data source={ Program.BinPath }database.db";
            bool newDatabase = false;
            if ( !File.Exists( Program.BinPath + DatabaseFile ) )
            {
                newDatabase = true;
                SQLiteConnection.CreateFile( Program.BinPath + DatabaseFile );
            }

            if ( !newDatabase )
            {
                conn = new SQLiteConnection( DatabaseSource );
                conn.Open(  );
                SQLiteCommand cmd = new SQLiteCommand( conn );

                cmd.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{ GuildDBName }'";
                
                bool found = cmd.ExecuteScalar( ) != null;
                
                conn.Close(  );
            }

            if ( newDatabase )
            {
                InitDB( );
                
                Program.Log( "Initialized New DB.", ConsoleColor.Yellow );
            }
        }

        private static void InitDB( )
        {
            conn = new SQLiteConnection( DatabaseSource );
            conn.Open(  );

            SQLiteCommand cmd = new SQLiteCommand( conn );

            cmd.CommandText = $@"CREATE TABLE '{GuildDBName}' (
                'id'	INTEGER NOT NULL,
                'snowflake'	INTEGER NOT NULL,
                'role_master'	INTEGER,
                'role_pending'	INTEGER,
                'role_trusted'	INTEGER,
                PRIMARY KEY('id' AUTOINCREMENT)
            )";
            if ( cmd.ExecuteNonQuery( ) != 0 )
            {
                Program.Log( $"Creation of guilds table failed!", ConsoleColor.Red );
            }
            
            conn.Close(  );

            Program.Log( $"Initialized Database `guilds' table", ConsoleColor.Green );
            
            
            conn = new SQLiteConnection( DatabaseSource );
            conn.Open(  );

            cmd = new SQLiteCommand( conn );
            cmd.CommandText = $@"CREATE TABLE 'users' (
                    'id'	INTEGER NOT NULL,
                    'snowflake'	INTEGER NOT NULL,
                    'nickname'	TEXT NOT NULL,
                    'experience'	INTEGER NOT NULL,
                    'currency'	INTEGER NOT NULL,
                    PRIMARY KEY('id')
                )";
                
            bool failed = cmd.ExecuteNonQuery( ) != 0;
            conn.Close(  );
            if ( failed )
            {
                Program.Log( $"Failed to create `users'!", ConsoleColor.Red );
                return;
            }
            Program.Log( $"Initialized Database `users' table", ConsoleColor.Green );
        }

        public static GuildUserData GetDBUser( DiscordMember member )
        {
            GuildUserData userData = null;
            
            conn = new SQLiteConnection( DatabaseSource );
            conn.Open(  );

            SQLiteCommand cmd = new SQLiteCommand( conn );

            cmd = new SQLiteCommand( conn );
            cmd.CommandText = $"SELECT * FROM 'users' WHERE `snowflake`='{ member.Id }'";
            SQLiteDataReader reader = cmd.ExecuteReader( );

            if ( reader.Read(  ) )
            {
                userData = new GuildUserData( )
                {
                    Snowflake = (ulong)(long)reader["snowflake"],
                    Nickname = (string)reader["nickname"],
                    Experience = (ulong)(long)reader["experience"],
                    Currency = (long)reader["currency"],
                };
            }
            else
            {
                userData = new GuildUserData( )
                {
                    Snowflake = member.Id,
                    Nickname = ( member.Nickname == string.Empty ) ? member.Username : member.Nickname,
                    Experience = 0,
                    Currency = 100,
                };
            }
            reader.Close(  );
            conn.Close(  );

            return userData;
        }

        public static void SaveGuildUser( GuildUserData data )
        {
            conn = new SQLiteConnection( DatabaseSource );
            conn.Open(  );

            SQLiteCommand cmd = new SQLiteCommand( conn );

            cmd = new SQLiteCommand( conn );
            cmd.CommandText = $"SELECT count(*) FROM 'users' WHERE `snowflake`='{ data.Snowflake }'";

            bool found = cmd.ExecuteScalar( ) != null;

            conn.Close(  );
            
            if ( found )
            {
                conn = new SQLiteConnection( DatabaseSource );
                conn.Open(  );
                cmd = new SQLiteCommand( conn );
                cmd.CommandText = $"UPDATE 'users' SET 'experience'='{data.Experience}','currency'='{data.Currency}','nickname'='{data.Nickname}' WHERE `snowflake`='{ data.Snowflake }'";
                bool failed = cmd.ExecuteScalar( ) != null;
                conn.Close(  );
                
                if ( failed )
                {
                    Program.Log( $"Failed to update users[{ data.Snowflake }]!", ConsoleColor.Red );
                }
            }
            else
            {
                conn = new SQLiteConnection( DatabaseSource );
                conn.Open(  );
                cmd = new SQLiteCommand( conn );
                
                cmd.CommandText = $@"INSERT INTO 'users' ('snowflake', 'nickname', 'experience', 'currency') VALUES ('{ data.Snowflake }','{data.Nickname}','{data.Experience}','{data.Currency}')";
                bool failed = cmd.ExecuteScalar( ) != null;
                conn.Close(  );
                
                if ( failed )
                {
                    Program.Log( $"Failed to insert users[{ data.Snowflake }]!", ConsoleColor.Red );
                }
            }

        }

        private static SQLiteConnection conn;
    
        private const string DatabaseFile = "database.db";
        private static string DatabaseSource;
        private const string GuildDBName = "guilds";
    }
}
