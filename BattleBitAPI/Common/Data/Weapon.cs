using System;

namespace BattleBitAPI.Common
{
    public class Weapon : IEquatable<string>, IEquatable<Weapon>
    {
        public string Name { get; private set; }
        public WeaponType WeaponType { get; private set; }
        public Weapon(string name, WeaponType weaponType)
        {
            Name = name;
            WeaponType = weaponType;
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
        public bool Equals(Weapon other)
        {
            if (other == null)
                return false;
            return this.Name.Equals(other.Name);
        }

        public static bool operator ==(string left, Weapon right)
        {
            bool leftNull = object.ReferenceEquals(left, null);
            bool rightNull = object.ReferenceEquals(right, null);
            if (leftNull && rightNull)
                return true;
            if (leftNull || rightNull)
                return false;
            return right.Name.Equals(left);
        }
        public static bool operator !=(string left, Weapon right)
        {
            bool leftNull = object.ReferenceEquals(left, null);
            bool rightNull = object.ReferenceEquals(right, null);
            if (leftNull && rightNull)
                return true;
            if (leftNull || rightNull)
                return false;
            return right.Name.Equals(left);
        }
        public static bool operator ==(Weapon right, string left)
        {
            bool leftNull = object.ReferenceEquals(left, null);
            bool rightNull = object.ReferenceEquals(right, null);
            if (leftNull && rightNull)
                return true;
            if (leftNull || rightNull)
                return false;
            return right.Name.Equals(left);
        }
        public static bool operator !=(Weapon right, string left)
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