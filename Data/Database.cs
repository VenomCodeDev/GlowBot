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
            
            cmd.CommandText = $@"CREATE TABLE 'guilds' (
                'id' INTEGER NOT NULL,
                'snowflake'	INTEGER NOT NULL,
                'nickname' TEXT NOT NULL,
                'joined_date' INTEGER NOT NULL,
                'guild_owner_id' INTEGER NOT NULL,
                'reactsmsg_pingsubscriber_id' INTEGER NOT NULL,
                'role_admin_id'	INTEGER NOT NULL,
                'role_pending_id' INTEGER NOT NULL,
                'role_trusted_id' INTEGER NOT NULL,
                'textchannel_leaderboard_id' INTEGER NOT NULL,
                'voicechannel_stats_id'	INTEGER NOT NULL,
                'voicechannel_newvc_id'	INTEGER NOT NULL,
                PRIMARY KEY('id' AUTOINCREMENT)
            )";
            if ( cmd.ExecuteNonQuery( ) != 0 )
            {
                Program.Log( $"Creation of guilds table failed!", ConsoleColor.Red );
                conn.Close(  );
                return;
            }
            
            conn.Close(  );

            Program.Log( $"Initialized Database `guilds' table", ConsoleColor.Green );
            
            
            conn = new SQLiteConnection( DatabaseSource );
            conn.Open(  );

            cmd = new SQLiteCommand( conn );
            cmd.CommandText = $"CREATE TABLE 'users' ('id' INTEGER NOT NULL,'snowflake' INTEGER NOT NULL,'nickname' TEXT NOT NULL,'experience' INTEGER NOT NULL,'currency' INTEGER NOT NULL,'reports' INTEGER NOT NULL,'messages' INTEGER NOT NULL,'last_talk_time' INTEGER NOT NULL, 'last_newvc_time' INTEGER NOT NULL,'joined_date' INTEGER NOT NULL,'last_command_time' INTEGER NOT NULL,PRIMARY KEY('id'))";
                
            bool failed = cmd.ExecuteNonQuery( ) != 0;
            conn.Close(  );
            if ( failed )
            {
                Program.Log( $"Failed to create `users'!", ConsoleColor.Red );
                return;
            }
            Program.Log( $"Initialized Database `users' table", ConsoleColor.Green );
        }

        public static GuildData GetDBGuild( DiscordGuild guild )
        {
            GuildData guildData = null;

            conn = new SQLiteConnection( DatabaseSource );
            conn.Open(  );

            SQLiteCommand cmd = new SQLiteCommand( conn );

            cmd.CommandText = $"SELECT * FROM 'guilds' WHERE `snowflake`='{ guild.Id }'";
            SQLiteDataReader reader = cmd.ExecuteReader( );

            if ( reader.Read( ) )
            {
                guildData = new GuildData( )
                {
                    Snowflake = (ulong)(long)reader[ "snowflake" ],
                    Nickname = (string)reader[ "nickname" ],
                    JoinDate = DateTime.FromBinary( (long)reader[ "joined_date" ] ),
                    ServerOwnerSnowflake = (ulong)(long)reader[ "guild_owner_id" ],
                    ServerReactsMsg_PingSubscriber = (ulong)(long)reader[ "reactsmsg_pingsubscriber_id" ],
                    ServerRole_Admin = (ulong)(long)reader[ "role_admin_id" ],
                    ServerRole_Pending = (ulong)(long)reader[ "role_pending_id" ],
                    ServerRole_Trusted = (ulong)(long)reader[ "role_trusted_id" ],
                    ServerTC_Leaderboard = (ulong)(long)reader[ "textchannel_leaderboard_id" ],
                    ServerVC_Stats = (ulong)(long)reader[ "voicechannel_stats_id" ],
                    ServerVC_NewVC = (ulong)(long)reader[ "voicechannel_newvc_id" ],
                };
            }
            else
            {
                guildData = new GuildData( )
                {
                    Snowflake = guild.Id,
                    Nickname = ( guild.Name == string.Empty ) ? guild.Id.ToString() : guild.Name,
                    JoinDate = DateTime.Now,
                    ServerOwnerSnowflake = 0,
                    ServerReactsMsg_PingSubscriber = 0,
                    ServerRole_Admin = 0,
                    ServerRole_Trusted = 0,
                    ServerTC_Leaderboard = 0,
                    ServerVC_Stats = 0,
                    ServerVC_NewVC = 0,
                };
            }
            reader.Close(  );
            conn.Close(  );

            return guildData;
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
                    Reports = (ulong)(long)reader["reports"],
                    Messages = (ulong)(long)reader["messages"],
                    JoinDate = DateTime.FromBinary( (long)reader["joined_date"] ),
                    LastTalkedTime = DateTime.FromBinary( (long)reader["last_talk_time"] ),
                    LastCommandTime = DateTime.FromBinary( (long)reader["last_command_time"] ),
                    LastNewVCTime = DateTime.FromBinary( (long)reader["last_newvc_time"] ),
                };
            }
            else
            {
                userData = new GuildUserData( )
                {
                    Snowflake = member.Id,
                    Nickname = member.Username,
                    Experience = 0,
                    Currency = 100,
                    Reports = 0,
                    Messages = 0,
                    JoinDate = DateTime.Now,
                    LastTalkedTime = DateTime.Now,
                    LastCommandTime = DateTime.Now,
                    LastNewVCTime = DateTime.Now,
                };
            }
            reader.Close(  );
            conn.Close(  );

            return userData;
        }

        public static void SaveDBGuild( GuildData data )
        {
            conn = new SQLiteConnection( DatabaseSource );
            conn.Open(  );

            SQLiteCommand cmd = new SQLiteCommand( conn );
            cmd.CommandText = $"SELECT count(*) FROM 'guilds' WHERE `snowflake`='{ data.Snowflake }'";

            bool found = (long)cmd.ExecuteScalar( ) > 0;
            
            conn.Close(  );

            if ( found )
            {
                string parts = $"UPDATE 'guilds' SET ";
                parts += $"'nickname'='{data.Nickname}',";
                parts += $"'joined_date'='{data.JoinDate.ToBinary(  )}',";
                parts += $"'guild_owner_id'='{data.ServerOwnerSnowflake}',";
                parts += $"'reactsmsg_pingsubscriber_id'='{data.ServerReactsMsg_PingSubscriber}',";
                parts += $"'role_admin_id'='{data.ServerRole_Admin}',";
                parts += $"'role_pending_id'='{data.ServerRole_Pending}',";
                parts += $"'role_trusted_id'='{data.ServerRole_Trusted}',";
                parts += $"'textchannel_leaderboard_id'='{data.ServerTC_Leaderboard}',";
                parts += $"'voicechannel_stats_id'='{data.ServerVC_Stats}',";
                parts += $"'voicechannel_newvc_id'='{data.ServerVC_NewVC}' ";

                parts += $"WHERE `snowflake`='{ data.Snowflake }'";

                conn = new SQLiteConnection( conn );
                conn.Open(  );
                cmd = new SQLiteCommand( conn );
                cmd.CommandText = parts;

                bool failed = cmd.ExecuteScalar( ) != null;
                conn.Close(  );

                if ( failed )
                {
                    Program.Log( $"Failed to update guilds[{data.Snowflake}]!", ConsoleColor.Red );
                }
            }
            else
            {
                string parts = $"INSERT INTO 'guilds' ";
                parts += $"('snowflake', 'nickname', 'joined_date', 'guild_owner_id', 'reactsmsg_pingsubscriber_id', 'role_admin_id', 'role_pending_id', 'role_trusted_id', 'textchannel_leaderboard_id', 'voicechannel_stats_id', 'voicechannel_newvc_id')";
                parts += $" VALUES ";
                parts += $"('{ data.Snowflake }','{data.Nickname}','{data.JoinDate.ToBinary(  )}','{data.ServerOwnerSnowflake}', '{data.ServerReactsMsg_PingSubscriber}', '{data.ServerRole_Admin}', '{data.ServerRole_Pending}', '{data.ServerRole_Trusted}', '{data.ServerTC_Leaderboard}', '{data.ServerVC_Stats}', '{data.ServerVC_NewVC}')";
                
                conn = new SQLiteConnection( DatabaseSource );
                conn.Open(  );
                cmd = new SQLiteCommand( conn );

                cmd.CommandText = parts;

                bool failed = cmd.ExecuteScalar( ) != null;
                conn.Close(  );

                if ( failed )
                {
                    Program.Log( $"Failed to insert guilds[{data.Snowflake}]!", ConsoleColor.Red );
                }
            }
            
        }
        public static void SaveDBUser( GuildUserData data )
        {
            conn = new SQLiteConnection( DatabaseSource );
            conn.Open(  );

            SQLiteCommand cmd = new SQLiteCommand( conn );
            cmd.CommandText = $"SELECT count(*) FROM 'users' WHERE `snowflake`='{ data.Snowflake }'";

            bool found = (long)cmd.ExecuteScalar( ) > 0;

            conn.Close(  );
            
            if ( found )
            {
                conn = new SQLiteConnection( DatabaseSource );
                conn.Open(  );
                cmd = new SQLiteCommand( conn );
                cmd.CommandText = $"UPDATE 'users' SET 'last_newvc_time'='{ data.LastNewVCTime.ToBinary(  ) }','experience'='{data.Experience}','currency'='{data.Currency}','nickname'='{data.Nickname}','reports'='{data.Reports}','messages'='{data.Messages}','last_talk_time'='{data.LastTalkedTime.ToBinary(  )}','joined_date'='{data.JoinDate.ToBinary(  )}','last_command_time'='{data.LastCommandTime.ToBinary(  )}' WHERE `snowflake`='{ data.Snowflake }'";
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
                
                cmd.CommandText = $"INSERT INTO 'users' ('last_newvc_time', 'snowflake', 'nickname', 'experience', 'currency', 'reports', 'messages', 'last_talk_time', 'joined_date', 'last_command_time') VALUES ('{data.LastNewVCTime.ToBinary(  )}','{ data.Snowflake }','{data.Nickname}','{data.Experience}','{data.Currency}', '{data.Reports}', '{data.Messages}', '{data.LastTalkedTime.ToBinary(  )}', '{data.JoinDate.ToBinary(  )}', '{data.LastCommandTime.ToBinary(  )}')";
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
