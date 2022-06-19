using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace GlowBot.SlashCommands
{
    public class SlashCommander : ApplicationCommandModule
    {
        [SlashCommand("PendUser", "Adds user to the pending database")]
        public async Task PendUserCommand(InteractionContext ctx, [Option("user", "User to pend")] DiscordUser user)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            bool callerIsAdmin = ctx.Member.Roles.Any( x => x.Id == Program.data!.ID_ROLE_ADMIN );

            if ( callerIsAdmin )
            {
                DiscordMember member = await ctx.Guild.GetMemberAsync( user.Id );
                DiscordRole pendRole = ctx.Guild.GetRole( Program.data!.ID_ROLE_PENDING );
                await member.GrantRoleAsync( pendRole, "Command-Requested by /PendUser" );
                
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"{ user.Mention } was added to the 'Pending' database."));
                return;
            }

            await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( Program.data!.RESPONSE_INSUFFICIENT_PERMISSIONS ) );
        }
        
        [SlashCommand("TrustUser", "Adds user to the trusted database")]
        public async Task TrustUserCommand(InteractionContext ctx, [Option("user", "User to trust")] DiscordUser user)
        {
            
            try
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

                bool callerIsAdmin = ctx.Member.Roles.Any( x => x.Id == Program.data!.ID_ROLE_ADMIN );

                DiscordMember member = await ctx.Guild.GetMemberAsync( user.Id );
            
                if ( callerIsAdmin )
                {
                    DiscordRole pendRole = ctx.Guild.GetRole( Program.data!.ID_ROLE_PENDING );
                    DiscordRole trustRole = ctx.Guild.GetRole( Program.data!.ID_ROLE_TRUSTED );

                    // Check if user already has the pending role; remove if yes
                    if ( member.Roles.Any( x => x.Id == Program.data!.ID_ROLE_PENDING ) )
                    {
                        await member.RevokeRoleAsync( pendRole );
                    }
                
                    await member.GrantRoleAsync( trustRole );
                
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("User was added to the 'Trusted' database."));
                }
                else
                {
                    await ctx.EditResponseAsync( new DiscordWebhookBuilder( ).WithContent( Program.data!.RESPONSE_INSUFFICIENT_PERMISSIONS ) );
                }
            }
            catch ( Exception ex )
            {
                Console.WriteLine( $"Exception in TrustUserCommand: {ex}" );
                throw;
            }
            
        }
    }
}
