namespace BattleBitAPI.Common;

public class Attachment : IEquatable<string>, IEquatable<Attachment>
{
    public Attachment(string name, AttachmentType attachmentType)
    {
        Name = name;
        AttachmentType = attachmentType;
    }

    public string Name { get; }
    public AttachmentType AttachmentType { get; private set; }

    public bool Equals(Attachment other)
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

    public static bool operator ==(string left, Attachment right)
    {
        var leftNull = ReferenceEquals(left, null);
        var rightNull = ReferenceEquals(right, null);
        if (leftNull && rightNull)
            return true;
        if (leftNull || rightNull)
            return false;
        return right.Name.Equals(left);
    }

    public static bool operator !=(string left, Attachment right)
    {
        var leftNull = ReferenceEquals(left, null);
        var rightNull = ReferenceEquals(right, null);
        if (leftNull && rightNull)
            return true;
        if (leftNull || rightNull)
            return false;
        return right.Name.Equals(left);
    }

    public static bool operator ==(Attachment right, string left)
    {
        var leftNull = ReferenceEquals(left, null);
        var rightNull = ReferenceEquals(right, null);
        if (leftNull && rightNull)
            return true;
        if (leftNull || rightNull)
            return false;
        return right.Name.Equals(left);
    }

    public static bool operator !=(Attachment right, string left)
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