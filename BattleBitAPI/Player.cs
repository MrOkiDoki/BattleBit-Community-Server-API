using BattleBitAPI.Server;
using System.Numerics;

namespace BattleBitAPI
{
    public class Player
    {
        public ulong SteamID { get; set; }
        public string Name { get; set; }
        public GameServer GameServer { get; set; }

        public void Kick(string reason = "")
        {
            this.GameServer.Kick(this, reason);
        }
        public void Kill()
        {
            this.GameServer.Kill(this);
        }
        public void ChangeTeam()
        {
            this.GameServer.ChangeTeam(this);
        }
        public void KickFromSquad()
        {
            this.GameServer.KickFromSquad(this);
        }
        public void DisbandTheSquad()
        {
            this.GameServer.DisbandPlayerCurrentSquad(this);
        }
        public void PromoteToSquadLeader()
        {
            this.GameServer.PromoteSquadLeader(this);
        }
        public void WarnPlayer(string msg)
        {
            this.GameServer.WarnPlayer(this, msg);
        }
        public void Message(string msg)
        {
            this.GameServer.MessageToPlayer(this, msg);
        }
        public void Teleport(Vector3 target)
        {

        }

        public override string ToString()
        {
            return this.Name + " (" + this.SteamID + ")";
        }
    }
}
