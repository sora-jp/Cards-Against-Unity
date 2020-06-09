using Unity.Networking.Transport.Utilities;

public struct NetworkDebugInfo
{
    public ReliableUtility.RTTInfo rtt;
    public int sent, received, dropped;
}