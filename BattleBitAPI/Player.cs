using BattleBitAPI.Common;
using BattleBitAPI.Server;
using System.Numerics;

namespace BattleBitAPI
{
    public class Player
    {
        public ulong SteamID { get; internal set; }
        public string Name { get; internal set; }
        public GameServer GameServer { get; internal set; }
        public GameRole Role { get; internal set; }
        public Team Team { get; internal set; }
        public Squads Squad { get; internal set; }
        public bool IsAlive { get; internal set; }
        public PlayerLoadout CurrentLoadout { get; internal set; } = new PlayerLoadout();
        public PlayerWearings CurrentWearings { get; internal set; } = new PlayerWearings();

        internal virtual async Task OnInitialized()
        {

        }
        internal virtual async Task OnSpawned()
        {

        }
        internal virtual async Task OnDied()
        {

        }

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
        public void SetNewRole(GameRole role)
        {
            this.GameServer.SetRoleTo(this, role);
        }
        public void Teleport(Vector3 target)
        {

        }
        public void SpawnPlayer(PlayerLoadout loadout, PlayerWearings wearings, Vector3 position, Vector3 lookDirection, PlayerStand stand, float spawnProtection)
        {
            GameServer.SpawnPlayer(this, loadout, wearings, position, lookDirection, stand, spawnProtection);
        }

        public override string ToString()
        {
            return this.Name + " (" + this.SteamID + ")";
        }
    }
}
