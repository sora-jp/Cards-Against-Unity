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

    readonly Dictionary<int, MethodInfo> _idToMethod = new Dictionary<int, MethodInfo>();

    public EventDistributor(object impl)
    {
        m_implementation = impl;
        var methods = m_implementation.GetType().MethodsWith(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            typeof(MessageAttribute));
        _idToMethod = methods.ToDictionary(m => Convert.ToInt32(m.Attribute<MessageAttribute>().MessageId));
    }

    public void AttachEvent<T>(string evtName)
    {
        m_evtProvider = typeof(T);
        m_evtName = evtName;
        
        m_evtProvider.AddHandler(m_evtName, HandleMessage);
    }

    // TODO: Cache everythang
    object HandleMessage(object[] arg)
    {
        // var paramTypes = arg.Skip(2).ToArray().ToTypeArray();

        
        // && m.Parameters().Select(p => p.ParameterType).Skip(1).SequenceEqual(paramTypes)

        var method = _idToMethod[Convert.ToInt32(arg[0])];

        var data = (IMessageData)method.Parameters()[0].ParameterType.CreateInstance();

        using (var stream = new MemoryStream((byte[])arg[1])) using (var reader = new BinaryReader(stream)) data.FromBytes(reader);

        List<object> parameters = new List<object> {data};
        parameters.AddRange(arg.Skip(2));

        method.Call(m_implementation, parameters.ToArray());

        return null;
    }
}

public class EventImplementor : MonoBehaviour
{
    readonly EventDistributor m_distributor;

    protected EventImplementor()
    {
        m_distributor = new EventDistributor(this);
    }

    protected void AttachEvent<T>(string name)
    {
        m_distributor.AttachEvent<T>(name);
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
    void FromBytes(BinaryReader reader);
    void Write(BinaryWriter writer);
}