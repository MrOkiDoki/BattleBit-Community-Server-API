using BattleBitAPI.Common;

namespace BattleBitAPI.Server
{
    public class Squad<TPlayer> where TPlayer : Player<TPlayer>
    {
        public Team Team => @internal.Team;
        public Squads Name => @internal.Name;
        public GameServer<TPlayer> Server => @internal.Server;
        public int NumberOfMembers => @internal.Members.Count;
        public bool IsEmpty => NumberOfMembers == 0;
        public IEnumerable<TPlayer> Members => @internal.Server.IterateMembersOf(this);
        public int SquadPoints
        {
            get => @internal.SquadPoints;

            set
            {
                @internal.SquadPoints = value;
                Server.SetSquadPointsOf(@internal.Team, @internal.Name, value);
            }
        }

        private Internal @internal;
        public Squad(Internal @internal)
        {
            this.@internal = @internal;
        }

        public override string ToString()
        {
            return "Squad " + Name;
        }

        // ---- Internal ----
        public class Internal
        {
            public readonly Team Team;
            public readonly Squads Name;
            public int SquadPoints;
            public GameServer<TPlayer> Server;
            public HashSet<TPlayer> Members;

            public Internal(GameServer<TPlayer> server, Team team, Squads squads)
            {
                this.Team = team;
                this.Name = squads;
                this.Server = server;
                this.Members = new HashSet<TPlayer>(8);
            }

            public void Reset()
            {

            }
        }
    }
}
