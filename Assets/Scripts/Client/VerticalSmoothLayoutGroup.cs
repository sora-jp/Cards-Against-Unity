using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class VerticalSmoothLayoutGroup : MonoBehaviour
{
    public float spacing;
    public float smoothTime;
    RectTransform m_transform;

    readonly Dictionary<Transform, Vector3> m_cToTarget = new Dictionary<Transform, Vector3>();
    readonly Dictionary<Transform, Vector3> m_cToVel = new Dictionary<Transform, Vector3>();
    readonly Dictionary<Transform, bool> m_cWasCreatedLastFrame = new Dictionary<Transform, bool>();

    void Awake()
    {
        m_transform = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        RecalculateTargets();
        for (var i = 0; i < transform.childCount; i++)
        {
            var c = transform.GetChild(i);
            var target = GetTarget(c);
            var vel = m_cToVel.ContainsKey(c) ? m_cToVel[c] : Vector3.zero;
#if UNITY_EDITOR
            if (!Application.isPlaying) m_cWasCreatedLastFrame[c] = true;
#endif
            c.localPosition = m_cWasCreatedLastFrame[c] ? target : Vector3.SmoothDamp(c.localPosition, target, ref vel, smoothTime);
            m_cWasCreatedLastFrame[c] = false;
            m_cToVel[c] = vel;
        }
    }

    void RecalculateTargets()
    {
        var totalH = m_transform.rect.height;
        var totalW = m_transform.rect.width;
        var wSlice = spacing;
        var wStart = -totalH / 2;

        for (int i = 0; i < transform.childCount; i++)
        {
            var c = transform.GetChild(i);
            if (!m_cToTarget.ContainsKey(c)) m_cWasCreatedLastFrame[c] = true;
            m_cToTarget[c] = Vector3.down * (wStart + i * wSlice) + Vector3.right * (totalW / 2);
        }
    }

    Vector3 GetTarget(Transform c)
    {
        if (!m_cToTarget.ContainsKey(c))
        {
            m_cToTarget[c] = Vector3.zero;
        }
        return m_cToTarget[c];
    }
}
