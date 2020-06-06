using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Utilities;
using UnityEngine;
using UnityEngine.Assertions;

public class CardsServer : MonoBehaviour
{
    public static CardsServer Instance { get; private set; }

    NetworkDriver m_driver;
    NetworkPipeline m_pipeline;
    NativeList<NetworkConnection> m_connections;

    public static event Action<byte, byte[], int> OnDataReceived; // id, data, conn
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

        m_driver = new NetworkDriver(new UDPNetworkInterface(), new ReliableUtility.Parameters {WindowSize = 32});
        m_pipeline = m_driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));

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
                    using (var data = new NativeArray<byte>(stream.Length - 1, Allocator.Temp))
                    {
                        var id = stream.ReadByte();
                        stream.ReadBytes(data);

                        OnDataReceived?.Invoke(id, data.ToArray(), conn);
                    }
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
    public void Send<T>(int connId, T id, IMessageData data, bool log = true) where T:Enum
    {
        if (log) Debug.Log($"SRV: Sending {id.ToString()} to client {connId}");

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
                    var writer = m_driver.BeginSend(m_connections[connId]);
                    writer.WriteBytes(buf, (int) stream.Length);
                    m_driver.EndSend(writer);
                }
            }
        }
    }

    public void SendAll<T>(T id, IMessageData data, bool log = true) where T:Enum
    {
        for (var i = 0; i < m_connections.Length; i++) Send(i, id, data, log);
    }

    public void SendAllExcept<T>(int cli, T id, IMessageData data, bool log = true) where T:Enum
    {
        for (var i = 0; i < m_connections.Length; i++)
        {
            if (i == cli) continue;
            Send(i, id, data, log);
        }
    }
}