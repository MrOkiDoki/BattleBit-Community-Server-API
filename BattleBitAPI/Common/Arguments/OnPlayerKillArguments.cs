using System.Numerics;

using BattleBitAPI.Common.Enums;

namespace BattleBitAPI.Common.Arguments
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
