using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Utilities;
using UnityEngine;
using ReliableSequencedPipelineStage = Unity.Networking.Transport.ReliableSequencedPipelineStage;

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

    public static event Action<byte, byte[]> OnDataReceived; // id, data
    public static event Action OnConnected;
    public static event Action OnDisconnected;
    public static event Action<State, State> OnStateChange; // old, new

    NetworkDriver m_driver;
    NetworkConnection m_connection;
    NetworkPipeline m_pipeline;

    void Awake()
    {
        // Singleton
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        // Init connections
        m_driver = new NetworkDriver(new UDPNetworkInterface(), new ReliableUtility.Parameters {WindowSize = 32});
        
        // Corresponds to server default pipeline
        m_pipeline = m_driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
        State = State.Initialized;
    }

    // TODO: Remove this
    void Start()
    {
        var ep = NetworkEndPoint.LoopbackIpv4;
        ep.Port = 9000;
        ConnectTo(ep);
    }

    // Connect to a certain endpoint
    public void ConnectTo(NetworkEndPoint endpoint)
    {
        State = State.Connecting;
        m_connection = m_driver.Connect(endpoint);
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

                        //Debug.Log($"CLI: Received {(MessageType)id} from server");
                        OnDataReceived?.Invoke(id, data.ToArray());
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

    // Send data to the server
    public void Send<T>(T id, IMessageData data, bool log = true) where T:Enum
    {
        if (log) Debug.Log($"CLI: Sending {id.ToString()} to server");

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
                    var writer = m_driver.BeginSend(m_connection, (int) stream.Length);
                    writer.WriteBytes(buf, (int)stream.Length);
                    m_driver.EndSend(writer);
                }
            }
        }
    }
}

public enum State
{
    Initializing, Initialized, Connecting, Connected, Disconnected
}