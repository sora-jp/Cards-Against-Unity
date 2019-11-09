using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Assertions;
using DefaultNetworkDriver = Unity.Networking.Transport.GenericNetworkDriver<Unity.Networking.Transport.IPv4UDPSocket, Unity.Networking.Transport.DefaultPipelineStageCollection>;

public class CardsServer : MonoBehaviour
{
    public static CardsServer Instance { get; private set; }

    DefaultNetworkDriver m_driver;
    NetworkPipeline m_pipeline;
    NativeList<NetworkConnection> m_connections;

    public static event Action<byte, byte[], int> OnDataReceived; // conn, id, data
    public static event Action<int> OnConnected;
    public static event Action<int> OnDisconnected;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        m_driver = new DefaultNetworkDriver(new INetworkParameter[0]);

        m_pipeline = m_driver.CreatePipeline(typeof(NullPipelineStage));

        var ip = NetworkEndPoint.AnyIpv4;
        ip.Port = 9000;

        if (m_driver.Bind(ip) != 0) Debug.LogError("SRV: Failed to bind to port 9000");
        else m_driver.Listen();

        Debug.Log("SRV: Server started on port 9000");

        m_connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
    }

    void OnDestroy()
    {
        m_driver.Dispose();
        m_connections.Dispose();
    }

    void Update()
    {
        m_driver.ScheduleUpdate().Complete();

        // Clean up connections
        for (var i = 0; i < m_connections.Length; i++)
        {
            if (m_connections[i].IsCreated) continue;
            m_connections.RemoveAtSwapBack(i);
            i--;
        }

        // Accept new connections
        NetworkConnection c;
        while ((c = m_driver.Accept()) != default)
        {
            m_connections.Add(c);
            OnConnected?.Invoke(m_connections.Length-1);
            Debug.Log($"SRV: Client {m_connections.Length-1} connected");
        }

        // Process current connections
        for (var i = 0; i < m_connections.Length; i++)
        {
            if (!m_connections[i].IsCreated) continue;

            ProcessConnectionData(i);
        }
    }

    void ProcessConnectionData(int conn)
    {
        while (true)
        {
            switch (m_driver.PopEventForConnection(m_connections[conn], out var stream))
            {
                case NetworkEvent.Type.Data:
                    var readerCtx = default(DataStreamReader.Context);
                    var data = new byte[stream.Length - 1];

                    var id = stream.ReadByte(ref readerCtx);
                    stream.ReadBytesIntoArray(ref readerCtx, ref data, data.Length);

                    OnDataReceived?.Invoke(id, data, conn);
                    break;

                case NetworkEvent.Type.Disconnect:
                    OnDisconnected?.Invoke(conn);

                    Debug.Log($"SRV: Client {conn} disconnected");
                    m_connections[conn] = default;
                    break;

                case NetworkEvent.Type.Empty:
                    return;
                case NetworkEvent.Type.Connect:
                    break;
                default:
                    break;
            }
        }
    }

    // Send data to a client
    public void Send<T>(int connId, T id, IMessageData data) where T:Enum
    {
        Debug.Log($"SRV: Sending data to client {connId}");

        using (var stream = new MemoryStream(32))
        {
            using (var binWriter = new BinaryWriter(stream, Encoding.ASCII, true))
            {
                binWriter.Write(Convert.ToByte(id));
                data.Write(binWriter);
            }

            unsafe
            {
                fixed (byte* buf = &stream.ToArray()[0])
                {
                    using (var writer = new DataStreamWriter((int) stream.Length, Allocator.Temp))
                    {
                        writer.WriteBytes(buf, (int) stream.Length);
                        m_connections[connId].Send(m_driver, writer);
                    }
                }
            }
        }
    }
}