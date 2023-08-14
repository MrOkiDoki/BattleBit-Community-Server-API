namespace BattleBitAPI.Common;

public class Map : IEquatable<string>, IEquatable<Map>
{
    public Map(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public bool Equals(Map other)
    {
        if (other == null)
            return false;
        return Name.Equals(other.Name);
    }

    public bool Equals(string other)
    {
        if (other == null)
            return false;
        return Name.Equals(other);
    }

    public override string ToString()
    {
        return Name;
    }

    public static bool operator ==(string left, Map right)
    {
        var leftNull = ReferenceEquals(left, null);
        var rightNull = ReferenceEquals(right, null);
        if (leftNull && rightNull)
            return true;
        if (leftNull || rightNull)
            return false;
        return right.Name.Equals(left);
    }

    public static bool operator !=(string left, Map right)
    {
        var leftNull = ReferenceEquals(left, null);
        var rightNull = ReferenceEquals(right, null);
        if (leftNull && rightNull)
            return true;
        if (leftNull || rightNull)
            return false;
        return right.Name.Equals(left);
    }

    public static bool operator ==(Map right, string left)
    {
        var leftNull = ReferenceEquals(left, null);
        var rightNull = ReferenceEquals(right, null);
        if (leftNull && rightNull)
            return true;
        if (leftNull || rightNull)
            return false;
        return right.Name.Equals(left);
    }

    public static bool operator !=(Map right, string left)
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