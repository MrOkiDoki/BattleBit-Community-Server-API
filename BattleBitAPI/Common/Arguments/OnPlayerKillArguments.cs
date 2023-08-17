using System.Numerics;

namespace BattleBitAPI.Common
{
    public struct OnPlayerKillArguments<TPlayer> where TPlayer : Player<TPlayer>
    {
        public TPlayer Killer;
        public Vector3 KillerPosition;

        public TPlayer Victim;
        public Vector3 VictimPosition;

        public string KillerTool;
        public PlayerBody BodyPart;
        public ReasonOfDamage SourceOfDamage;
    }
}
