using BattleBitAPI.Common;
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
        public GameRole Role
        {
            get => mInternal.Role;
            set
            {
                if (value == mInternal.Role)
                    return;
                SetNewRole(value);
            }
        }
        public Team Team
        {
            get => mInternal.Team;
            set
            {
                if (mInternal.Team != value)
                    ChangeTeam(value);
            }
        }
        public Squads Squad
        {
            get => mInternal.Squad;
            set
            {
                if (value == mInternal.Squad)
                    return;
                if (value == Squads.NoSquad)
                    KickFromSquad();
                else
                    JoinSquad(value);
            }
        }
        public bool InSquad => mInternal.Squad != Squads.NoSquad;
        public int PingMs => mInternal.PingMs;

        public float HP => mInternal.HP;
        public bool IsAlive => mInternal.HP >= 0f;
        public bool IsUp => mInternal.HP > 0f;
        public bool IsDown => mInternal.HP == 0f;
        public bool IsDead => mInternal.HP == -1f;

        public Vector3 Position
        {
            get => mInternal.Position;
            set => Teleport(value);
        }
        public PlayerStand StandingState => mInternal.Standing;
        public LeaningSide LeaningState => mInternal.Leaning;
        public LoadoutIndex CurrentLoadoutIndex => mInternal.CurrentLoadoutIndex;
        public bool InVehicle => mInternal.InVehicle;
        public bool IsBleeding => mInternal.IsBleeding;
        public PlayerLoadout CurrentLoadout => mInternal.CurrentLoadout;
        public PlayerWearings CurrentWearings => mInternal.CurrentWearings;
        public PlayerModifications<TPlayer> Modifications => mInternal.Modifications;

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
        public virtual async Task OnDowned()
        {

        }
        public virtual async Task OnGivenUp()
        {

        }
        public virtual async Task OnRevivedByAnotherPlayer()
        {

        }
        public virtual async Task OnRevivedAnotherPlayer()
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
            GameServer.Kick(this, reason);
        }
        public void Kill()
        {
            GameServer.Kill(this);
        }
        public void ChangeTeam()
        {
            GameServer.ChangeTeam(this);
        }
        public void ChangeTeam(Team team)
        {
            GameServer.ChangeTeam(this, team);
        }
        public void KickFromSquad()
        {
            GameServer.KickFromSquad(this);
        }
        public void JoinSquad(Squads targetSquad)
        {
            GameServer.JoinSquad(this, targetSquad);
        }
        public void DisbandTheSquad()
        {
            GameServer.DisbandPlayerCurrentSquad(this);
        }
        public void PromoteToSquadLeader()
        {
            GameServer.PromoteSquadLeader(this);
        }
        public void WarnPlayer(string msg)
        {
            GameServer.WarnPlayer(this, msg);
        }
        public void Message(string msg)
        {
            GameServer.MessageToPlayer(this, msg);
        }
        public void Message(string msg, float fadeoutTime)
        {
            GameServer.MessageToPlayer(this, msg, fadeoutTime);
        }
        public void SetNewRole(GameRole role)
        {
            GameServer.SetRoleTo(this, role);
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
            return Name + " (" + SteamID + ")";
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

            public mPlayerModifications _Modifications;
            public PlayerModifications<TPlayer> Modifications;

            public Internal()
            {
                this.Modifications = new PlayerModifications<TPlayer>(this);
                this._Modifications = new mPlayerModifications();
            }

            public void OnDie()
            {
                IsAlive = false;
                HP = -1f;
                Position = default;
                Standing = PlayerStand.Standing;
                Leaning = LeaningSide.None;
                CurrentLoadoutIndex = LoadoutIndex.Primary;
                InVehicle = false;
                IsBleeding = false;
                CurrentLoadout = new PlayerLoadout();
                CurrentWearings = new PlayerWearings();
            }
        }
        public class mPlayerModifications
        {
            public float RunningSpeedMultiplier = 1f;
            public float ReceiveDamageMultiplier = 1f;
            public float GiveDamageMultiplier = 1f;
            public float JumpHeightMultiplier = 1f;
            public float FallDamageMultiplier = 1f;
            public float ReloadSpeedMultiplier = 1f;
            public bool CanUseNightVision = true;
            public bool HasCollision = false;
            public float DownTimeGiveUpTime = 60f;
            public bool AirStrafe = true;
            public bool CanSpawn = true;
            public bool CanSpectate = true;
            public bool IsTextChatMuted = false;
            public bool IsVoiceChatMuted = false;
            public float RespawnTime = 10f;
            public bool CanRespawn = true;

            public bool IsDirtyFlag = false;
            public void Write(BattleBitAPI.Common.Serialization.Stream ser)
            {
                ser.Write(this.RunningSpeedMultiplier);
                ser.Write(this.ReceiveDamageMultiplier);
                ser.Write(this.GiveDamageMultiplier);
                ser.Write(this.JumpHeightMultiplier);
                ser.Write(this.FallDamageMultiplier);
                ser.Write(this.ReloadSpeedMultiplier);
                ser.Write(this.CanUseNightVision);
                ser.Write(this.HasCollision);
                ser.Write(this.DownTimeGiveUpTime);
                ser.Write(this.AirStrafe);
                ser.Write(this.CanSpawn);
                ser.Write(this.CanSpectate);
                ser.Write(this.IsTextChatMuted);
                ser.Write(this.IsVoiceChatMuted);
                ser.Write(this.RespawnTime);
                ser.Write(this.CanRespawn);
            }
            public void Read(BattleBitAPI.Common.Serialization.Stream ser)
            {
                this.RunningSpeedMultiplier = ser.ReadFloat();
                if (this.RunningSpeedMultiplier <= 0f)
                    this.RunningSpeedMultiplier = 0.01f;

                this.ReceiveDamageMultiplier = ser.ReadFloat();
                this.GiveDamageMultiplier = ser.ReadFloat();
                this.JumpHeightMultiplier = ser.ReadFloat();
                this.FallDamageMultiplier = ser.ReadFloat();
                this.ReloadSpeedMultiplier = ser.ReadFloat();
                this.CanUseNightVision = ser.ReadBool();
                this.HasCollision = ser.ReadBool();
                this.DownTimeGiveUpTime = ser.ReadFloat();
                this.AirStrafe = ser.ReadBool();
                this.CanSpawn = ser.ReadBool();
                this.CanSpectate = ser.ReadBool();
                this.IsTextChatMuted = ser.ReadBool();
                this.IsVoiceChatMuted = ser.ReadBool();
                this.RespawnTime = ser.ReadFloat();
                this.CanRespawn = ser.ReadBool();
            }
        }
    }
}
