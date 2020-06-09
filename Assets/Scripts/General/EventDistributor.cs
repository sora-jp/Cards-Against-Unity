using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Type = System.Type;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EventDistributor
{
    Type m_evtProvider;
    string m_evtName;

    readonly object m_implementation;

    readonly Dictionary<int, MethodInfo> m_idToMethod;
    readonly Dictionary<MethodInfo, bool> m_methodNeedsData;

    public EventDistributor(object impl)
    {
        m_implementation = impl;
        var methods = m_implementation.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(m => m.GetCustomAttribute<MessageAttribute>() != null).ToArray();

        m_idToMethod = methods.ToDictionary(m => Convert.ToInt32(m.GetCustomAttribute<MessageAttribute>().MessageId));
        m_methodNeedsData = methods.ToDictionary(m => m, NeedsData);
    }

    static bool NeedsData(MethodInfo method)
    {
        var param = method.GetParameters();
        return param.Length > 0 && param.First().ParameterType.GetInterfaces().Contains(typeof(IMessageData));
    }

    public void AttachEvent<T>(string evtName)
    {
        m_evtProvider = typeof(T);
        m_evtName = evtName;
        
        var evt = m_evtProvider.GetEvent(m_evtName);
        evt.AddEventHandler(null, 
            Delegate.CreateDelegate(
                typeof(Action<object[]>), 
                this, 
                GetType().GetMethod(nameof(HandleMessage), BindingFlags.Instance | BindingFlags.NonPublic))
            );
    }

    // TODO: Cache everythang
    [Preserve]
    void HandleMessage(object[] arg)
    {
        var id = Convert.ToInt32(arg[0]);
        if (!m_idToMethod.ContainsKey(id)) return;

        var method = m_idToMethod[Convert.ToInt32(arg[0])];

        var data = (IMessageData) null;

        if (m_methodNeedsData[method]) data = (IMessageData)Activator.CreateInstance(method.GetParameters()[0].ParameterType);

        if (data != null) using (var stream = new MemoryStream((byte[])arg[1])) using (var reader = new BinaryReader(stream)) data.FromBytes(reader);

        var parameters = new List<object>();
        if (data != null) parameters.Add(data);
        parameters.AddRange(arg.Skip(2));

        method.Invoke(m_implementation, parameters.ToArray());
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
public sealed class MessageAttribute : PreserveAttribute
{
    public object MessageId { get; }

    public MessageAttribute(object messageId)
    {
        MessageId = messageId;
    }
}

[Preserve]
public interface IMessageData
{
    [Preserve] void FromBytes(BinaryReader reader);
    [Preserve] void Write(BinaryWriter writer);
}