namespace BattleBitAPI.Common;

public class Weapon : IEquatable<string>, IEquatable<Weapon>
{
    public Weapon(string name, WeaponType weaponType)
    {
        Name = name;
        WeaponType = weaponType;
    }

    public string Name { get; }
    public WeaponType WeaponType { get; private set; }

    public bool Equals(string other)
    {
        if (other == null)
            return false;
        return Name.Equals(other);
    }

    public bool Equals(Weapon other)
    {
        if (other == null)
            return false;
        return Name.Equals(other.Name);
    }

    public override string ToString()
    {
        return Name;
    }

    public static bool operator ==(string left, Weapon right)
    {
        var leftNull = ReferenceEquals(left, null);
        var rightNull = ReferenceEquals(right, null);
        if (leftNull && rightNull)
            return true;
        if (leftNull || rightNull)
            return false;
        return right.Name.Equals(left);
    }

    public static bool operator !=(string left, Weapon right)
    {
        var leftNull = ReferenceEquals(left, null);
        var rightNull = ReferenceEquals(right, null);
        if (leftNull && rightNull)
            return true;
        if (leftNull || rightNull)
            return false;
        return right.Name.Equals(left);
    }

    public static bool operator ==(Weapon right, string left)
    {
        var leftNull = ReferenceEquals(left, null);
        var rightNull = ReferenceEquals(right, null);
        if (leftNull && rightNull)
            return true;
        if (leftNull || rightNull)
            return false;
        return right.Name.Equals(left);
    }

    public static bool operator !=(Weapon right, string left)
    {
        var leftNull = ReferenceEquals(left, null);
        var rightNull = ReferenceEquals(right, null);
        if (leftNull && rightNull)
            return true;
        if (leftNull || rightNull)
            return false;
        return right.Name.Equals(left);
    }
}