using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Utilities;
using UnityEngine;
using UnityEngine.Scripting;

public class CardsClient : MonoBehaviour
{
    public static CardsClient Instance { get; private set; }

    State m_state = State.Initializing;
    public State State
    {
        get => m_state;
        private set
        {
            OnStateChange?.Invoke(m_state, value);
            m_state = value;
        }
    }

    public static event Action<object[]> OnDataReceived; // id, data
    public static event Action OnConnected;
    public static event Action OnDisconnected;
    public static event Action<State, State> OnStateChange; // old, new

    NetworkDriver m_driver;
    NetworkConnection m_connection;
    NetworkPipeline m_pipeline;
    readonly Queue<Packet> m_sendQueue = new Queue<Packet>();

    void Awake()
    {
        // Singleton
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        // Init driver
        m_driver = NetworkDriver.Create(
            new ReliableUtility.Parameters
            {
                WindowSize = 32
            }
#if SIMULATE_BAD_CONNECTION
            ,new SimulatorUtility.Parameters
            {
                PacketDelayMs = 25,
                PacketDropPercentage = 15
            }
#endif
            );

        // Create pipelines corresponding to those on the server
        m_pipeline = m_driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));

#if SIMULATE_BAD_CONNECTION
        m_driver.CreatePipeline(typeof(SimulatorPipelineStage));
#endif

        State = State.Initialized;
    }

    // TODO: Remove this
    void Start()
    {
        if (CardsServer.Instance == null) return;
        var ep = NetworkEndPoint.LoopbackIpv4;
        ep.Port = CardsServer.PORT;
        ConnectTo("localhost");
    }

    // Connect to a certain endpoint
    public void ConnectTo(string endpoint)
    {
        State = State.Connecting;
        Debug.Log($"Resolving DNS record for host {endpoint}");
        var dnsRecord = Dns.GetHostEntry(endpoint).AddressList.Single(e => e.AddressFamily == AddressFamily.InterNetwork);
        Debug.Log($"DNS Resolved to addr: {dnsRecord}");
        m_connection = m_driver.Connect(NetworkEndPoint.Parse(dnsRecord.ToString(), CardsServer.PORT));
    }

    void OnDestroy()
    {
        m_driver.Dispose();
    }

    void Update()
    {
        // Update the network driver.
        m_driver.ScheduleUpdate().Complete();

        // Idle if we aren't connected yet.
        if (!m_connection.IsCreated) return;

        while (true)
        {
            // Read the next event
            switch (m_connection.PopEvent(m_driver, out var stream))
            {
                case NetworkEvent.Type.Empty:
                    return;
                case NetworkEvent.Type.Data:
                    using (var data = new NativeArray<byte>(stream.Length - 1, Allocator.Temp))
                    {
                        var id = stream.ReadByte();
                        stream.ReadBytes(data);

                        if (id != (byte)MessageType.CmdHeartbeat) Debug.Log($"CLI: Received {(MessageType)id} from server");
                        OnDataReceived?.Invoke(new object[] {id, data.ToArray()});
                    }

                    break;
                case NetworkEvent.Type.Connect:
                    State = State.Connected;
                    OnConnected?.Invoke();

                    Debug.Log("CLI: Connected");
                    break;
                case NetworkEvent.Type.Disconnect:
                    State = State.Disconnected;
                    OnDisconnected?.Invoke();

                    Debug.Log("CLI: Disconnected");
                    m_connection = default;
                    break;
                default:
                    break;
            }
        }
    }

    void LateUpdate()
    {
        if (State != State.Connected) return;
        while (m_sendQueue.Count > 0) Send(m_sendQueue.Dequeue());
    }

    // Send data to the server
    public void Send<T>(T id, IMessageData data, bool log = true) where T:Enum
    {
        m_sendQueue.Enqueue(new Packet {messageID = id, messageData = data, logPacket = log});
    }

    void Send(Packet packet)
    {
        if (packet.logPacket) Debug.Log($"CLI: Sending {packet.messageID} to server");

        using (var stream = new MemoryStream(32))
        {
            using (var binWriter = new BinaryWriter(stream, Encoding.ASCII, true))
            {
                binWriter.Write(Convert.ToByte(packet.messageID));
                packet.messageData.Write(binWriter);
            }

            unsafe
            {
                fixed (byte* buf = &stream.ToArray()[0])
                {
                    var writer = m_driver.BeginSend(m_connection, (int)stream.Length);
                    writer.WriteBytes(buf, (int)stream.Length);
                    m_driver.EndSend(writer);
                }
            }
        }
    }

    public NetworkDebugInfo GatherDebugInfo()
    {
        unsafe
        {
            var pipelineStage = NetworkPipelineStageCollection.GetStageId(typeof(ReliableSequencedPipelineStage));
            m_driver.GetPipelineBuffers(m_pipeline, pipelineStage, m_connection, out _, out _, out var reliableBuffer);
            var ctx = (ReliableUtility.SharedContext*)reliableBuffer.GetUnsafePtr();

            return new NetworkDebugInfo
            {
                rtt = ctx->RttInfo,
                sent = ctx->stats.PacketsSent,
                received = ctx->stats.PacketsReceived,
                dropped = ctx->stats.PacketsDropped
            };
        }
    }
}