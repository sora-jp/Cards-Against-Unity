// If this code works, it was written by Sora
// Otherwise, I don't know who wrote it
public partial class ClientData
{
    public bool Equals(ClientData other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return guid == other.guid;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == this.GetType() && Equals((ClientData) obj);
    }

    public override int GetHashCode()
    {
        return guid;
    }

    public static bool operator ==(ClientData left, ClientData right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ClientData left, ClientData right)
    {
        return !Equals(left, right);
    }
}