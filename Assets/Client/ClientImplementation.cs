using System.Collections;
using System.Collections.Generic;
using System.IO;
using Fasterflect;
using UnityEngine;

public class ClientImplementation : MonoBehaviour
{
    EventDistributor m_distributor;

    void Awake()
    {
        m_distributor = new EventDistributor(this);
        m_distributor.AttachEvent<CardsClient>(nameof(CardsClient.OnDataReceived));
    }

    [Message(MessageType.Test)]
    void Handle(TestMessageData data)
    {

    }
}

public class TestMessageData : IMessageData
{
    public int Value { get; private set; }

    public void ParseFromBytes(BinaryReader reader)
    {
        Value = reader.ReadInt32();
    }
}