﻿using BattleBitAPI.Common;
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

        public float HP
        {
            get => mInternal.HP;
            set
            {
                if (mInternal.HP > 0)
                {
                    float v = value;
                    if (v <= 0)
                        v = 0.1f;
                    else if (v > 100f)
                        v = 100f;

                    SetHP(v);
                }
            }
        }
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
        public void Ban()
        {
            GameServer.Ban(this);
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
        public static void SetInstance(TPlayer player, Player<TPlayer>.Internal @internal)
        {
            player.mInternal = @internal;
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

            public PlayerModifications<TPlayer>.mPlayerModifications _Modifications;
            public PlayerModifications<TPlayer> Modifications;

            public Internal()
            {
                this._Modifications = new PlayerModifications<TPlayer>.mPlayerModifications();
                this.Modifications = new PlayerModifications<TPlayer>(this);
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
    }
}
