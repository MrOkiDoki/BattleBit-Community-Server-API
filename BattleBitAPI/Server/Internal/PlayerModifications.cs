namespace BattleBitAPI.Server
{
    public class PlayerModifications<TPlayer> where TPlayer : Player<TPlayer>
    {
        private Player<TPlayer>.Internal @internal;
        public PlayerModifications(Player<TPlayer>.Internal @internal)
        {
            this.@internal = @internal;
        }

        public float RunningSpeedMultiplier { get; set; }
        public float ReceiveDamageMultiplier { get; set; }
        public float GiveDamageMultiplier { get; set; }
        public float JumpHeightMultiplier { get; set; }
        public float FallDamageMultiplier { get; set; }
        public float ReloadSpeedMultiplier { get; set; }
        public bool CanUseNightVision { get; set; }
        public bool HasCollision { get; set; }
        public float DownTimeGiveUpTime { get; set; }
        public bool AirStrafe { get; set; }
        public bool CanSpawn { get; set; }
        public bool CanSpectate { get; set; }
        public bool IsTextChatMuted { get; set; }
        public bool IsVoiceChatMuted { get; set; }
        public float RespawnTime { get; set; }
        public bool CanRespawn { get; set; }
    }
}
