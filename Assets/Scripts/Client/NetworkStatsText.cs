using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class NetworkStatsText : MonoBehaviour
{
    public string format;

    public float timeBetweenSamples;
    public int sampleCountForAverage;

    TextMeshProUGUI m_text;
    readonly Queue<NetworkDebugInfo> m_samples = new Queue<NetworkDebugInfo>();
    NetworkDebugInfo? m_lastInfo;

    void Awake()
    {
        m_text = GetComponent<TextMeshProUGUI>();
        StartCoroutine(UpdateStats());
    }

    IEnumerator UpdateStats()
    {
        while (true)
        {
            while (CardsClient.Instance == null || CardsClient.Instance.State != State.Connected) yield return null;
            var info = CardsClient.Instance.GatherDebugInfo();
            var last = m_lastInfo ?? info;
            m_lastInfo = info;

            info.dropped -= last.dropped;
            info.received -= last.received;
            info.sent -= last.sent;

            m_samples.Enqueue(info);
            while (m_samples.Count > sampleCountForAverage) m_samples.Dequeue();

            var curRtt = info.rtt.SmoothedRtt;
            var dropPercent = (float)m_samples.Sum(s => s.dropped) / m_samples.Sum(s => s.sent);
            if (float.IsNaN(dropPercent) || float.IsInfinity(dropPercent)) dropPercent = 0;

            m_text.text = $"RTT: {curRtt:F1}, Packets dropped: {dropPercent*100:F1}%";

            yield return new WaitForSeconds(timeBetweenSamples);
        }
    }
}
