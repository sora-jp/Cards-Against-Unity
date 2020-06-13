using System.Collections.Generic;
using UnityEngine;

public class CardLayout : MonoBehaviour
{
    public float verticalOffset;
    public float horizPadding;
    public float smoothTime;
    RectTransform rtransform;

    Dictionary<Transform, float> _cToTargetHeight = new Dictionary<Transform, float>();
    Dictionary<Transform, Vector3> _cToTarget = new Dictionary<Transform, Vector3>();
    Dictionary<Transform, Vector3> _cToVel = new Dictionary<Transform, Vector3>();
    Dictionary<Transform, bool> _cWasCreatedLastFrame = new Dictionary<Transform, bool>();

    void Awake()
    {
        rtransform = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        RecalculateTargets();
        for (var i = 0; i < transform.childCount; i++)
        {
            var c = transform.GetChild(i);
            var target = GetTarget(c);
            var vel = _cToVel.ContainsKey(c) ? _cToVel[c] : Vector3.zero;
            c.localPosition = _cWasCreatedLastFrame[c] ? target : Vector3.SmoothDamp(c.localPosition, target, ref vel, smoothTime);
            _cWasCreatedLastFrame[c] = false;
            _cToVel[c] = vel;
        }
    }

    void RecalculateTargets()
    {
        var totalH = rtransform.rect.height + verticalOffset;
        var totalW = rtransform.rect.width - horizPadding * 2;
        var wSlice = totalW / (transform.childCount-1);
        var wStart = -totalW / 2;

        if (transform.childCount <= 1)
        {
            wSlice = 0;
            wStart = 0;
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            var c = transform.GetChild(i);
            if (!_cToTarget.ContainsKey(c)) _cWasCreatedLastFrame[c] = true;
            _cToTarget[c] = Vector3.right * (wStart + i * wSlice) + Vector3.up * totalH;
        }
    }

    Vector3 GetTarget(Transform c)
    {
        if (!_cToTarget.ContainsKey(c))
        {
            _cToTarget[c] = Vector3.zero;
        }
        if (!_cToTargetHeight.ContainsKey(c)) _cToTargetHeight[c] = 0;
        return _cToTarget[c] + Vector3.up * _cToTargetHeight[c];
    }

    public void SetTargetY(Transform c, float target)
    {
        _cToTargetHeight[c] = target;
    }
}
