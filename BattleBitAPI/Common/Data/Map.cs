namespace BattleBitAPI.Common
{
    public class Map : IEquatable<string>, IEquatable<Map>
    {
        public string Name { get; private set; }
        public Map(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return this.Name;
        }
        public bool Equals(string other)
        {
            if (other == null)
                return false;
            return this.Name.Equals(other);
        }
        public bool Equals(Map other)
        {
            if (other == null)
                return false;
            return this.Name.Equals(other.Name);
        }

        public static bool operator ==(string left, Map right)
        {
            bool leftNull = object.ReferenceEquals(left, null);
            bool rightNull = object.ReferenceEquals(right, null);
            if (leftNull && rightNull)
                return true;
            if (leftNull || rightNull)
                return false;
            return right.Name.Equals(left);
        }
        public static bool operator !=(string left, Map right)
        {
            bool leftNull = object.ReferenceEquals(left, null);
            bool rightNull = object.ReferenceEquals(right, null);
            if (leftNull && rightNull)
                return true;
            if (leftNull || rightNull)
                return false;
            return right.Name.Equals(left);
        }
        public static bool operator ==(Map right, string left)
        {
            bool leftNull = object.ReferenceEquals(left, null);
            bool rightNull = object.ReferenceEquals(right, null);
            if (leftNull && rightNull)
                return true;
            if (leftNull || rightNull)
                return false;
            return right.Name.Equals(left);
        }
        public static bool operator !=(Map right, string left)
        {
            bool leftNull = object.ReferenceEquals(left, null);
            bool rightNull = object.ReferenceEquals(right, null);
            if (leftNull && rightNull)
                return true;
            if (leftNull || rightNull)
                return false;
            return right.Name.Equals(left);
        }
    }
}
