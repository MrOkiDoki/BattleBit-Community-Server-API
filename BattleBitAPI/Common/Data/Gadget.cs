namespace BattleBitAPI.Common
{
    public class Gadget : IEquatable<string>, IEquatable<Gadget>
    {
        public string Name { get; private set; }
        public Gadget(string name)
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
        public bool Equals(Gadget other)
        {
            if (other == null)
                return false;
            return this.Name.Equals(other.Name);
        }

        public static bool operator ==(string left, Gadget right)
        {
            bool leftNull = object.ReferenceEquals(left,null);
            bool rightNull = object.ReferenceEquals(right, null);
            if (leftNull && rightNull)
                return true;
            if (leftNull || rightNull)
                return false;
            return right.Name.Equals(left);
        }
        public static bool operator !=(string left, Gadget right)
        {
            bool leftNull = object.ReferenceEquals(left, null);
            bool rightNull = object.ReferenceEquals(right, null);
            if (leftNull && rightNull)
                return true;
            if (leftNull || rightNull)
                return false;
            return right.Name.Equals(left);
        }
        public static bool operator ==(Gadget right, string left)
        {
            bool leftNull = object.ReferenceEquals(left, null);
            bool rightNull = object.ReferenceEquals(right, null);
            if (leftNull && rightNull)
                return true;
            if (leftNull || rightNull)
                return false;
            return right.Name.Equals(left);
        }
        public static bool operator !=(Gadget right, string left)
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
