using BattleBitAPI.Common;
using BattleBitAPI.Networking;
using BattleBitAPI.Server;
using System.Net;
using System.Numerics;

namespace BattleBitAPI
{
    public class Player<TPlayer> where TPlayer : Player<TPlayer>
    {
        private Internal mInternal;

        // ---- Variables ----
        public ulong SteamID => mInternal.SteamID;
        public string Name => mInternal.Name;
        public IPAddress IP => mInternal.IP;
        public GameServer<TPlayer> GameServer => mInternal.GameServer;
        public GameRole Role => mInternal.Role;
        public Team Team => mInternal.Team;
        public Squads Squad => mInternal.Squad;
        public bool InSquad => mInternal.Squad != Squads.NoSquad;
        public int PingMs => mInternal.PingMs;

        public float HP => mInternal.HP;
        public bool IsAlive => mInternal.HP >= 0f;
        public bool IsUp => mInternal.HP > 0f;
        public bool IsDown => mInternal.HP == 0f;
        public bool IsDead => mInternal.HP == -1f;

        public Vector3 Position => mInternal.Position;
        public PlayerStand Standing => mInternal.Standing;
        public LeaningSide Leaning => mInternal.Leaning;
        public LoadoutIndex CurrentLoadoutIndex => mInternal.CurrentLoadoutIndex;
        public bool InVehicle => mInternal.InVehicle;
        public bool IsBleeding => mInternal.IsBleeding;
        public PlayerLoadout CurrentLoadout => mInternal.CurrentLoadout;
        public PlayerWearings CurrentWearings => mInternal.CurrentWearings;

        // ---- Events ----
        public virtual void OnCreated()
        {

        }

        public virtual async Task OnConnected()
        {

        }
        public virtual async Task OnSpawned()
        {

        }
        public virtual async Task OnDied()
        {

        }
        public virtual async Task OnChangedTeam()
        {

        }
        public virtual async Task OnChangedRole(GameRole newRole)
        {

        }
        public virtual async Task OnJoinedSquad(Squads newSquad)
        {

        }
        public virtual async Task OnLeftSquad(Squads oldSquad)
        {

        }
        public virtual async Task OnDisconnected()
        {

        }

        // ---- Functions ----
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
        public void ChangeTeam(Team team)
        {
            this.GameServer.ChangeTeam(this, team);
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
        public void Message(string msg, float fadeoutTime)
        {
            this.GameServer.MessageToPlayer(this, msg, fadeoutTime);
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
        public void SetHP(float newHP)
        {
            GameServer.SetHP(this, newHP);
        }
        public void GiveDamage(float damage)
        {
            GameServer.GiveDamage(this, damage);
        }
        public void Heal(float hp)
        {
            GameServer.Heal(this, hp);
        }
        public void SetRunningSpeedMultiplier(float value)
        {
            GameServer.SetRunningSpeedMultiplier(this, value);
        }
        public void SetReceiveDamageMultiplier(float value)
        {
            GameServer.SetReceiveDamageMultiplier(this, value);
        }
        public void SetGiveDamageMultiplier(float value)
        {
            GameServer.SetGiveDamageMultiplier(this, value);
        }
        public void SetJumpMultiplier(float value)
        {
            GameServer.SetJumpMultiplier(this, value);
        }
        public void SetFallDamageMultiplier(float value)
        {
            GameServer.SetFallDamageMultiplier(this, value);
        }
        public void SetPrimaryWeapon(WeaponItem item, int extraMagazines, bool clear = false)
        {
            GameServer.SetPrimaryWeapon(this, item, extraMagazines, clear);
        }
        public void SetSecondaryWeapon(WeaponItem item, int extraMagazines, bool clear = false)
        {
            GameServer.SetSecondaryWeapon(this, item, extraMagazines, clear);
        }
        public void SetFirstAidGadget(string item, int extra, bool clear = false)
        {
            GameServer.SetFirstAid(this, item, extra, clear);
        }
        public void SetLightGadget(string item, int extra, bool clear = false)
        {
            GameServer.SetLightGadget(this, item, extra, clear);
        }
        public void SetHeavyGadget(string item, int extra, bool clear = false)
        {
            GameServer.SetHeavyGadget(this, item, extra, clear);
        }
        public void SetThrowable(string item, int extra, bool clear = false)
        {
            GameServer.SetThrowable(this, item, extra, clear);
        }

        // ---- Static ----
        public static TPlayer CreateInstance<TPlayer>(Player<TPlayer>.Internal @internal) where TPlayer : Player<TPlayer>
        {
            TPlayer player = (TPlayer)Activator.CreateInstance(typeof(TPlayer));
            player.mInternal = @internal;
            return player;
        }

        // ---- Overrides ----
        public override string ToString()
        {
            return this.Name + " (" + this.SteamID + ")";
        }

        // ---- Internal ----
        public class Internal
        {
            public ulong SteamID;
            public string Name;
            public IPAddress IP;
            public GameServer<TPlayer> GameServer;
            public GameRole Role;
            public Team Team;
            public Squads Squad;
            public int PingMs = 999;

            public bool IsAlive;
            public float HP;
            public Vector3 Position;
            public PlayerStand Standing;
            public LeaningSide Leaning;
            public LoadoutIndex CurrentLoadoutIndex;
            public bool InVehicle;
            public bool IsBleeding;
            public PlayerLoadout CurrentLoadout;
            public PlayerWearings CurrentWearings;

            public void OnDie()
            {
                this.IsAlive = false;
                this.HP = -1f;
                this.Position = default;
                this.Standing = PlayerStand.Standing;
                this.Leaning = LeaningSide.None;
                this.CurrentLoadoutIndex = LoadoutIndex.Primary;
                this.InVehicle = false;
                this.IsBleeding = false;
                this.CurrentLoadout = new PlayerLoadout();
                this.CurrentWearings = new PlayerWearings();
            }
        }
    }
}
