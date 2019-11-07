using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerImplementation : EventImplementor
{
    void Awake()
    {
        AttachEvent<CardsServer>(nameof(CardsServer.OnDataReceived));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B)) CardsServer.Instance.Send(0, MessageType.Test, new TestMessageData {Value = 5});
    }

    [Message(MessageType.Test)]
    void Handle(TestMessageData data, int client)
    {
        Debug.Log($"SRV: Client {client} sent data {data.Value}");
    }
}