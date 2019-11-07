using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardLayout : MonoBehaviour
{
    public float verticalOffset;
    public float horizPadding;
    RectTransform rtransform;

    void Awake()
    {
        rtransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        var totalH = rtransform.rect.height + verticalOffset;
        var totalW = rtransform.rect.width - horizPadding * 2;
        var wSlice = totalW / (transform.childCount - 1);
        var wStart = -totalW / 2;

        for (int i = 0; i < transform.childCount; i++)
        {
            var c = transform.GetChild(i);
            c.transform.localPosition = Vector3.right * (wStart + i * wSlice) + Vector3.up * totalH;
        }
    }
}
