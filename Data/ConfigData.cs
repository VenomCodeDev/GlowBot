namespace GlowBot.Data
{
    public class ConfigData
    {
        public ulong ID_GUILD_MASTERS { get; set; }

        public string RESPONSE_INSUFFICIENT_PERMISSIONS { get; set; }
        public string RESPONSE_COMMAND_COOLDOWN { get; set; }
        public string RESPONSE_INVALID_CHANNEL_TYPE { get; set; }

        public static ConfigData? Default
        {
            get
            {
                return new ConfigData( )
                {
                    RESPONSE_INSUFFICIENT_PERMISSIONS = "Uh, no.",
                    RESPONSE_COMMAND_COOLDOWN = "You're in timeout! Git Gud",
                    RESPONSE_INVALID_CHANNEL_TYPE = "Invalid channel type.",
                };
            }
        }
    }
}
