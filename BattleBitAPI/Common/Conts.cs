namespace BattleBitAPI
{
    public static class Const
    {
        public static string Version = "1.0.2v";

        // ---- Networking ---- 
        /// <summary>
        /// Maximum data size for a single package. 4MB is default.
        /// </summary>
        public const int MaxNetworkPackageSize = 1024 * 1024 * 4;//4mb
        /// <summary>
        /// How long should server/client wait until connection is determined as timed out when no packages is being sent for long time.
        /// </summary>
        public const int NetworkTimeout = 60 * 1000;//60 seconds
        /// <summary>
        /// How frequently client/server will send keep alive to each other when no message is being sent to each other for a while.
        /// </summary>
        public const int NetworkKeepAlive = 5 * 1000;//15 seconds
        /// <summary>
        /// How long server/client will wait other side to send their hail/initial package. In miliseconds.
        /// </summary>
#if DEBUG
        public const int HailConnectTimeout = 20 * 1000;
#else
        public const int HailConnectTimeout = 2 * 1000;
#endif

        // ---- Server Fields ---- 
        public const int MinServerNameLength = 5;
        public const int MaxServerNameLength = 400;

        public const int MaxTokenSize = 512;

        public const int MinGamemodeNameLength = 2;
        public const int MaxGamemodeNameLength = 12;

        public const int MinMapNameLength = 2;
        public const int MaxMapNameLength = 36;

        public const int MinLoadingScreenTextLength = 0;
        public const int MaxLoadingScreenTextLength = 1024 * 8;

        public const int MinServerRulesTextLength = 0;
        public const int MaxServerRulesTextLength = 1024 * 8;

    }
}
