using BattleBitAPI.Common;
using System;

namespace BattleBitAPI.Common
{
    public class Attachment : IEquatable<string>, IEquatable<Attachment>
    {
        public string Name { get; private set; }
        public AttachmentType AttachmentType { get; private set; }
        public Attachment(string name, AttachmentType attachmentType)
        {
            Name = name;
            AttachmentType = attachmentType;
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
        public bool Equals(Attachment other)
        {
            if (other == null)
                return false;
            return this.Name.Equals(other.Name);
        }

        public static bool operator ==(string left, Attachment right)
        {
            bool leftNull = object.ReferenceEquals(left, null);
            bool rightNull = object.ReferenceEquals(right, null);
            if (leftNull && rightNull)
                return true;
            if (leftNull || rightNull)
                return false;
            return right.Name.Equals(left);
        }
        public static bool operator !=(string left, Attachment right)
        {
            bool leftNull = object.ReferenceEquals(left, null);
            bool rightNull = object.ReferenceEquals(right, null);
            if (leftNull && rightNull)
                return true;
            if (leftNull || rightNull)
                return false;
            return right.Name.Equals(left);
        }
        public static bool operator ==(Attachment right, string left)
        {
            bool leftNull = object.ReferenceEquals(left, null);
            bool rightNull = object.ReferenceEquals(right, null);
            if (leftNull && rightNull)
                return true;
            if (leftNull || rightNull)
                return false;
            return right.Name.Equals(left);
        }
        public static bool operator !=(Attachment right, string left)
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
