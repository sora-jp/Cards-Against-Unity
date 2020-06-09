using UnityEngine;

[ExecuteInEditMode, RequireComponent(typeof(RectTransform))]
public class SlideFromEdge : MonoBehaviour
{
    public RectTransform.Edge side;

    [Range(0, 1)]
    public float transition;

    public float distanceFromEdge;
    public Vector2 offsets;

    RectTransform m_rect;

    void Update()
    {
        if (m_rect == null) m_rect = GetComponent<RectTransform>();
        var size = side == RectTransform.Edge.Bottom || side == RectTransform.Edge.Top
            ? m_rect.rect.height
            : m_rect.rect.width;
        m_rect.SetInsetAndSizeFromParentEdge(side, (transition * 2 - 1) * distanceFromEdge + Mathf.Lerp(offsets.x, offsets.y, transition) - size / 2, size);
    }
}