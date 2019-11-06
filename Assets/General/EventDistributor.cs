using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Fasterflect;
using UnityEngine;
using Type = System.Type;

public class EventDistributor
{
    Type m_evtProvider;
    string m_evtName;

    readonly object m_implementation;

    public EventDistributor(object impl) => m_implementation = impl;

    public void AttachEvent<T>(string evtName)
    {
        m_evtProvider = typeof(T);
        m_evtName = evtName;
        
        m_evtProvider.AddHandler(m_evtName, HandleMessage);
    }

    object HandleMessage(object[] arg)
    {
        var paramTypes = arg.Skip(2).ToArray().ToTypeArray();

        var methods = m_implementation.GetType().MethodsWith(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            typeof(MessageAttribute));

        var method = methods.Single(m => Convert.ToInt32(m.Attribute<MessageAttribute>().MessageId) == Convert.ToInt32(arg[0]) && m.Parameters().Select(p => p.ParameterType).Skip(2).SequenceEqual(paramTypes));

        var data = (IMessageData)method.Parameters()[0].ParameterType.CreateInstance();

        using (var stream = new MemoryStream((byte[])arg[1])) using (var reader = new BinaryReader(stream)) data.ParseFromBytes(reader);

        var parameters = new List<object> {data};
        parameters.AddRange(arg.Skip(2));

        method.Call(m_implementation, parameters.ToArray());

        return null;
    }
}

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
sealed class MessageAttribute : Attribute
{
    public object MessageId { get; }

    public MessageAttribute(object messageId)
    {
        MessageId = messageId;
    }
}

public interface IMessageData
{
    void ParseFromBytes(BinaryReader reader);
}