using System.Collections;
using System.Collections.Generic;
using System.IO;
using Fasterflect;
using UnityEngine;

public class ClientImplementation : EventImplementor
{
    void Awake()
    {
        AttachEvent<CardsClient>(nameof(CardsClient.OnDataReceived));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)) CardsClient.Instance.Send(MessageType.Test, new TestMessageData {Value = 5});
    }

    [Message(MessageType.Test)]
    void Handle(TestMessageData data)
    {
        Debug.Log($"CLI: Server sent data {data.Value}");
    }
}

public struct TestMessageData : IMessageData
{
    public int Value { get; set; }

    public void FromBytes(BinaryReader reader)
    {
        Value = reader.ReadInt32();
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(Value);
    }
}