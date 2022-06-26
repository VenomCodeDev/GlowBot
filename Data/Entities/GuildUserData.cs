namespace GlowBot.Data.Entities
{
    public class GuildUserData
    {
        public ulong Snowflake { get; set; }
        public string Nickname { get; set; }
        public ulong Experience { get; set; }
        public long Currency { get; set; }
        public ulong Reports { get; set; }
        public ulong Messages { get; set; }
        public DateTime JoinDate { get; set; }
        public DateTime LastTalkedTime { get; set; }
        public DateTime LastCommandTime { get; set; }
        public DateTime LastNewVCTime { get; set; }
    }
}
