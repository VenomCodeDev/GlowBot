namespace GlowBot.Data
{
    public class ConfigData
    {
        public ulong ID_USER_MASTER { get; set; }
        public ulong ID_GUILD_MASTERS { get; set; }

        public ulong ID_ROLE_PENDING { get; set; }
        public ulong ID_ROLE_TRUSTED { get; set; }
        
        public ulong ID_ROLE_ADMIN { get; set; }

        public string RESPONSE_INSUFFICIENT_PERMISSIONS { get; set; }

        public static ConfigData? Default
        {
            get
            {
                return new ConfigData( )
                {
                    RESPONSE_INSUFFICIENT_PERMISSIONS = "Uh, no.",
                };
            }
        }
    }
}
