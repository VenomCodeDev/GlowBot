using System.Data;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

using GlowBot.Data;
using GlowBot.Data.Entities;

namespace GlowBot.SlashCommands
{
    public class SlashCommander : ApplicationCommandModule
    {
        public enum ProgramChannelType
        {
            [ChoiceName("CustomVC")]
            CUSTOM_VC,
            [ChoiceName("Stats")]
            STATS,
        }
        [SlashCommand( "ProgramChannel", "Programs guild in the Database" )]
        public async Task ProgramRoleCommand( InteractionContext ctx, [Option("Type", "Type of programming to do")] ProgramChannelType type, [Option("Target", "The target")]DiscordChannel channel  )
        {
            await ctx.CreateResponseAsync( InteractionResponseType.DeferredChannelMessageWithSource );

            GuildData guildData = Database.GetDBGuild( ctx.Guild );

            bool callerAllowed = ctx.Member.Roles.Any( x => x.Id == guildData.ServerRole_Admin ) || ctx.Member.IsOwner;

            if ( callerAllowed )
            {
                if ( type is ProgramChannelType.CUSTOM_VC )
                {
                    if ( channel.Type is ChannelType.Voice )
                    {
                        guildData.ServerVC_NewVC = channel.Id;
                        
                        await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( $"NewVC's Snowflake was updated in the database." ) );
                    }
                    else
                    {
                        await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( Program.Data!.RESPONSE_INVALID_CHANNEL_TYPE ) );
                    }
                }
                else
                {
                    if ( channel.Type is ChannelType.Voice )
                    {
                        guildData.ServerVC_Stats = channel.Id;
                        
                        await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( $"Stats's Snowflake was updated in the database." ) );
                    }
                    else
                    {
                        await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( Program.Data!.RESPONSE_INVALID_CHANNEL_TYPE ) );
                    }
                }
                try
                {
                    Database.SaveDBGuild( guildData );
                }
                catch ( Exception ex )
                {
                    Console.WriteLine( ex );
                }
                
                return;
            }
            await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( Program.Data!.RESPONSE_INSUFFICIENT_PERMISSIONS ) );
        }
        public enum ProgramRoleType
        {
            [ChoiceName("Trusted Role")]
            TRUSTED_ROLE,
            [ChoiceName("Pending Role")]
            PENDING_ROLE,
            [ChoiceName("Admin Role")]
            ADMIN_ROLE,
        }
        [SlashCommand( "ProgramRole", "Programs guild in the Database" )]
        public async Task ProgramRoleCommand( InteractionContext ctx, [Option("Type", "Type of programming to do")] ProgramRoleType type, [Option("Target", "The target")]DiscordRole tarRole  )
        {
            await ctx.CreateResponseAsync( InteractionResponseType.DeferredChannelMessageWithSource );

            GuildData guildData = Database.GetDBGuild( ctx.Guild );

            bool callerAllowed = ctx.Member.Roles.Any( x => x.Id == guildData.ServerRole_Admin ) || ctx.Member.IsOwner;

            if ( callerAllowed )
            {
                switch (type)
                {
                    case ProgramRoleType.ADMIN_ROLE:
                        guildData.ServerRole_Admin = tarRole.Id;
                        break;
                    case ProgramRoleType.TRUSTED_ROLE:
                        guildData.ServerRole_Trusted = tarRole.Id;
                        break;
                    case ProgramRoleType.PENDING_ROLE:
                        guildData.ServerRole_Pending = tarRole.Id;
                        break;
                }
                try
                {

                    Database.SaveDBGuild( guildData );
                }
                catch ( Exception ex )
                {
                    Console.WriteLine( ex );
                    throw;
                }
                
                await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( $"Role Id was updated in the database." ) );
                return;
            }
            await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( Program.Data!.RESPONSE_INSUFFICIENT_PERMISSIONS ) );
        }
        [SlashCommand( "Shutdown", "Terminates the bot" )]
        public async Task ShutdownCommand( InteractionContext ctx )
        {
            await ctx.CreateResponseAsync( InteractionResponseType.DeferredChannelMessageWithSource );

            GuildData guildData = Database.GetDBGuild( ctx.Guild );

            bool callerAllowed = ctx.Member.Id == Program.Data!.USER_MASTER_ID;

            if ( callerAllowed )
            {
                await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( $"Shutting down D:" ) );
                Program.IsShutdown = true;
                return;
            }
            await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( Program.Data!.RESPONSE_INSUFFICIENT_PERMISSIONS ) );
        }
        [SlashCommand( "Preload", "Preloads a user into the database" )]
        public async Task PreloadCommand( InteractionContext ctx, [Option( "User", "User change" )] DiscordUser user )
        {
            await ctx.CreateResponseAsync( InteractionResponseType.DeferredChannelMessageWithSource );

            GuildData guildData = Database.GetDBGuild( ctx.Guild );

            bool callerAllowed = ctx.Member.Roles.Any( x => x.Id == guildData.ServerRole_Admin ) || ctx.Member.IsOwner;

            if ( callerAllowed )
            {
                DiscordMember tarMember = await ctx.Guild.GetMemberAsync( user.Id );
                
                GuildUserData tarUserData = Database.GetDBUser( tarMember );

                Database.SaveDBUser( tarUserData );
                
                await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( $"Preloaded '{tarUserData.Nickname}' into the database." ) );
                return;
            }
            await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( Program.Data!.RESPONSE_INSUFFICIENT_PERMISSIONS ) );
        }
        [SlashCommand( "Nickname", "Gets your account status" )]
        public async Task NicknameCommand( InteractionContext ctx, [Option( "User", "User change" )] DiscordUser user, [Option("Nickanme", "New nickname")]string newNick )
        {
            await ctx.CreateResponseAsync( InteractionResponseType.DeferredChannelMessageWithSource );

            GuildData guildData = Database.GetDBGuild( ctx.Guild );

            bool callerAllowed = ctx.Member.Roles.Any( x => x.Id == guildData.ServerRole_Admin ) || ctx.Member.IsOwner;

            if ( callerAllowed )
            {
                DiscordMember tarMember = await ctx.Guild.GetMemberAsync( user.Id );
                
                GuildUserData srcUserData = Database.GetDBUser( ctx.Member );
                GuildUserData tarUserData = Database.GetDBUser( tarMember );

                tarUserData.Nickname = newNick;

                Database.SaveDBUser( tarUserData );
                
                await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( $"{tarMember.Mention} 's nickname was changed." ) );
                return;
            }
            await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( Program.Data!.RESPONSE_INSUFFICIENT_PERMISSIONS ) );
        }

        [SlashCommand( "Status", "Gets your account status" )]
        public async Task StatusCommand( InteractionContext ctx )
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            GuildData guildData = Database.GetDBGuild( ctx.Guild );
            
            bool callerIsTrusted = ctx.Member.Roles.Any( x => x.Id == guildData.ServerRole_Trusted );

            if ( callerIsTrusted )
            {
                GuildUserData srcUserData = Database.GetDBUser( ctx.Member );
                
                await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( $"{ctx.Member.Mention} has been reported '{srcUserData.Reports}' times." ) );
                return;
            }
            await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( Program.Data!.RESPONSE_INSUFFICIENT_PERMISSIONS ) );
        }
        
        [SlashCommand( "GetStatus", "Reports a user to administrators" )]
        public async Task GetStatusCommand( InteractionContext ctx, [Option( "user", "User to report" )] DiscordUser user )
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            GuildData guildData = Database.GetDBGuild( ctx.Guild );
            
            bool callerIsTrusted = ctx.Member.Roles.Any( x => x.Id == guildData.ServerRole_Trusted );

            if ( callerIsTrusted )
            {
                DiscordMember tarMember = await ctx.Guild.GetMemberAsync( user.Id );

                GuildUserData tarUserData = Database.GetDBUser( tarMember );

                Database.SaveDBUser( tarUserData );
                
                await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( $"{tarMember.Mention} has been reported '{ tarUserData.Reports }' times." ) );
                return;
            }
            await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( Program.Data!.RESPONSE_INSUFFICIENT_PERMISSIONS ) );
        }
        [SlashCommand( "Report", "Reports a user to administrators" )]
        public async Task ReportCommand( InteractionContext ctx, [Option( "user", "User to report" )] DiscordUser user, [Option("reason", "Reason for report")] string reason )
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            GuildData guildData = Database.GetDBGuild( ctx.Guild );
            
            bool callerIsTrusted = ctx.Member.Roles.Any( x => x.Id == guildData.ServerRole_Trusted );

            if ( callerIsTrusted )
            {
                DiscordMember tarMember = await ctx.Guild.GetMemberAsync( user.Id );

                GuildUserData tarUserData = Database.GetDBUser( tarMember );
                GuildUserData srcUserData = Database.GetDBUser( ctx.Member );
            
                bool callerCanCommand = (DateTime.Now - srcUserData.LastCommandTime ).TotalSeconds >= 360;

                if (! callerCanCommand )
                {
                    await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( Program.Data.RESPONSE_COMMAND_COOLDOWN ) );
                    return;
                }
                srcUserData.LastCommandTime = DateTime.Now;

                tarUserData.Reports++;
                Database.SaveDBUser( srcUserData );
                Database.SaveDBUser( tarUserData );
                
                await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( $"{tarMember.Mention} has been reported for \"{reason}\"." ) );
                return;
            }
            
            await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( Program.Data!.RESPONSE_INSUFFICIENT_PERMISSIONS ) );
        }
        
        [SlashCommand( "Pay", "Pay given user" )]
        public async Task PayCommand( InteractionContext ctx, [Option( "user", "User to set balance for" )] DiscordUser user, [Option( "amount", "Payment Amount" )] long amount )
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            GuildData guildData = Database.GetDBGuild( ctx.Guild );
            
            bool callerIsTrusted = ctx.Member.Roles.Any( x => x.Id == guildData.ServerRole_Trusted );

            if ( callerIsTrusted )
            {
                DiscordMember tarMember = await ctx.Guild.GetMemberAsync( user.Id );

                GuildUserData tarUserData = Database.GetDBUser( tarMember );
                GuildUserData srcUserData = Database.GetDBUser( ctx.Member );
            
                bool callerCanCommand = (DateTime.Now - srcUserData.LastCommandTime ).TotalSeconds >= 360;

                if (! callerCanCommand )
                {
                    await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( Program.Data.RESPONSE_COMMAND_COOLDOWN ) );
                    return;
                }
                srcUserData.LastCommandTime = DateTime.Now;
                
                if ( amount <= srcUserData.Currency )
                {
                    if ( amount > 0 )
                    {
                        srcUserData.Currency -= amount;
                        tarUserData.Currency += amount;
                        
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"{ ctx.Member.Mention } payed ${amount} to {tarMember.Mention}!"));
                        return;
                    }
                    else
                    {
                        await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( $"{ctx.Member.Mention} Gave an invalid amount. Do better math next time, ay?" ) );
                    }
                }
                else
                {
                    await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( $"{ctx.Member.Mention} is too poor. LOL!" ) );
                }

                        
                Database.SaveDBUser( srcUserData );
                Database.SaveDBUser( tarUserData );
                return;
            }

            await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( Program.Data!.RESPONSE_INSUFFICIENT_PERMISSIONS ) );
        }
        
        [SlashCommand( "Balance", "Gets Your Balance" )]
        public async Task BalanceCommand( InteractionContext ctx )
        {
            await ctx.CreateResponseAsync( InteractionResponseType.DeferredChannelMessageWithSource );
            
            GuildUserData userData = Database.GetDBUser( ctx.Member );
            
            await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( $"{ctx.Member.Mention} has a balance of ${userData.Currency}" ) );
        }

        [SlashCommand( "GetBalance", "Gets other users balance" )]
        public async Task GetBalanceCommand( InteractionContext ctx, [Option( "User", "User to pend" )] DiscordUser user )
        {
            await ctx.CreateResponseAsync( InteractionResponseType.DeferredChannelMessageWithSource );
            
            DiscordMember member = await ctx.Guild.GetMemberAsync( user.Id );
            GuildUserData tarUserData = Database.GetDBUser( member );
            GuildUserData srcUserData = Database.GetDBUser( ctx.Member );
            
            bool callerCanCommand = (DateTime.Now - srcUserData.LastCommandTime ).TotalSeconds >= 360;

            if (! callerCanCommand )
            {
                await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( Program.Data.RESPONSE_COMMAND_COOLDOWN ) );
                return;
            }
            srcUserData.LastCommandTime = DateTime.Now;
                        
            Database.SaveDBUser( srcUserData );
            Database.SaveDBUser( tarUserData );

            await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( $"{member.Mention} 's balance is ${tarUserData.Currency}" ) );
        }

        [SlashCommand( "SetBalance", "Sets given users balance" )]
        public async Task SetBalanceCommand( InteractionContext ctx, [Option( "user", "User to set balance for" )] DiscordUser user, [Option( "balance", "New Balance" )] long balance )
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            GuildData guildData = Database.GetDBGuild( ctx.Guild );
            
            bool callerIsAdmin = ctx.Member.Roles.Any( x => x.Id == guildData.ServerRole_Admin );

            if ( callerIsAdmin )
            {
                DiscordMember member = await ctx.Guild.GetMemberAsync( user.Id );

                GuildUserData tarUserData = Database.GetDBUser( member );
                tarUserData.Currency = balance;
                Database.SaveDBUser( tarUserData );
                
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"{ user.Mention } 's balance was set to ${balance}."));
                return;
            }

            await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( Program.Data!.RESPONSE_INSUFFICIENT_PERMISSIONS ) );
        }
        
        public enum ClearUsersType
        {
            [ChoiceName("Pending")]
            Pending,
            [ChoiceName("Unregistered")]
            Unregistered,
        }

        public async Task ClearUsersCommand( InteractionContext ctx, [Option( "Type", "Type of clear" )] ClearUsersType type )
        {
            
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            GuildData guildData = Database.GetDBGuild( ctx.Guild );
            
            bool callerIsAdmin = ctx.Member.Roles.Any( x => x.Id == guildData.ServerRole_Admin );

            if ( callerIsAdmin )
            {

                int removedAmt = 0;
            
                var allMembers = await ctx.Guild.GetAllMembersAsync( );
                foreach (DiscordMember member in allMembers)
                {
                    bool shouldRemove = false;

                    if ( type is ClearUsersType.Pending )
                    {
                        shouldRemove = member.Roles.Any( x => x.Id == guildData.ServerRole_Pending ) && member.Roles.All( x => x.Id != guildData.ServerRole_Pending );
                    }
                    else if( type is ClearUsersType.Unregistered)
                    {
                        shouldRemove = member.Roles.All( x =>
                            x.Id != guildData.ServerRole_Pending && x.Id != guildData.ServerRole_Pending );
                    }

                    if ( shouldRemove )
                    {
                        await member.RemoveAsync( "Kicked via the ClearUsers command." );
                        removedAmt++;
                    }
                }
                if ( removedAmt > 0 )
                {
                    await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( $"Removed {removedAmt} users." ) );
                    return;
                }
                await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( $"No users were found to remove..." ) );
                return;
            }
            await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( Program.Data!.RESPONSE_INSUFFICIENT_PERMISSIONS ) );
        }
        
        [SlashCommand("PendUser", "Adds user to the pending database")]
        public async Task PendUserCommand(InteractionContext ctx, [Option("User", "User to pend")] DiscordUser user)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            GuildData guildData = Database.GetDBGuild( ctx.Guild );
            
            bool callerIsAdmin = ctx.Member.Roles.Any( x => x.Id == guildData.ServerRole_Admin );

            if ( callerIsAdmin )
            {
                DiscordMember member = await ctx.Guild.GetMemberAsync( user.Id );
                DiscordRole pendRole = ctx.Guild.GetRole( guildData.ServerRole_Pending );
                await member.GrantRoleAsync( pendRole, "Command-Requested by /PendUser" );
                
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"{ user.Mention } was added to the 'Pending' database."));
                return;
            }

            await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( Program.Data!.RESPONSE_INSUFFICIENT_PERMISSIONS ) );
        }
        
        [SlashCommand("TrustUser", "Adds user to the trusted database")]
        public async Task TrustUserCommand(InteractionContext ctx, [Option("User", "User to trust")] DiscordUser user)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            GuildData guildData = Database.GetDBGuild( ctx.Guild );
            
            bool callerIsAdmin = ctx.Member.Roles.Any( x => x.Id == guildData.ServerRole_Admin );

            DiscordMember member = await ctx.Guild.GetMemberAsync( user.Id );
            
            if ( callerIsAdmin )
            {
                DiscordRole pendRole = ctx.Guild.GetRole( guildData.ServerRole_Pending );
                DiscordRole trustRole = ctx.Guild.GetRole( guildData.ServerRole_Pending );

                // Check if user already has the pending role; remove if yes
                if ( member.Roles.Any( x => x.Id == guildData.ServerRole_Pending ) )
                {
                    await member.RevokeRoleAsync( pendRole );
                }
                
                await member.GrantRoleAsync( trustRole );
                
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("User was added to the 'Trusted' database."));
            }
            else
            {
                await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( Program.Data!.RESPONSE_INSUFFICIENT_PERMISSIONS ) );
            }
        }
    }
}
