namespace GlowBot.Data.Entities
{
    public class GuildData
    {
        public ulong Snowflake { get; set; }
        public string Nickname { get; set; }
        public DateTime JoinDate { get; set; }
        public ulong ServerOwnerSnowflake { get; set; }
        
        public ulong ServerRole_Admin { get; set; }
        public ulong ServerRole_Trusted { get; set; }
        public ulong ServerRole_Pending { get; set; }
        
        public ulong ServerVC_NewVC { get; set; }
        public ulong ServerVC_Stats { get; set; }
        
        public ulong ServerTC_Leaderboard { get; set; }
        
        public ulong ServerReactsMsg_PingSubscriber { get; set; }
    }
}
