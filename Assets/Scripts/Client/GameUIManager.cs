using System.Collections;
using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    public SlideFromEdge[] additionalSlides;
    public SlideFromEdge cardHolderSlide;

    void Awake()
    {
        //cardHolderSlide.transition = 0;

        ClientImplementation.OnVotingBegin += Hide;
        ClientImplementation.OnNewRoundBegin += Show;
    }

    void OnDestroy()
    {
        ClientImplementation.OnVotingBegin -= Hide;
        ClientImplementation.OnNewRoundBegin -= Show;
    }

    void Show()
    {
        StartCoroutine(Animate());
        IEnumerator Animate()
        {
            SetTransitionAmt(0);
            float t = 0;

            while (t < 1)
            {
                t += Time.deltaTime / 0.75f;
                SetTransitionAmt(Mathf.Pow(t, 3) / (Mathf.Pow(t, 3) + Mathf.Pow(1-t, 3)));
                yield return null;
            }

            SetTransitionAmt(1);
        }
    }

    void Hide()
    {
        StartCoroutine(Animate());
        IEnumerator Animate()
        {
            SetTransitionAmt(1);
            float t = 0;

            while (t < 1)
            {
                t += Time.deltaTime / 0.75f;
                SetTransitionAmt(Mathf.Pow(1-t, 3) / (Mathf.Pow(1-t, 3) + Mathf.Pow(t, 3)));
                yield return null;
            }

            SetTransitionAmt(0);
        }
    }

    void LateUpdate()
    {
        if (ClientImplementation.Instance.State != null && ClientImplementation.Instance.MyGuid == ClientImplementation.Instance.State.CurrentCzarGuid)
            cardHolderSlide.transition = 0;
    }

    void SetTransitionAmt(float amt)
    {
        foreach (var slide in additionalSlides)
        {
            slide.transition = amt;
        }

        if (ClientImplementation.Instance.MyGuid != ClientImplementation.Instance.State.CurrentCzarGuid) 
            cardHolderSlide.transition = amt;
    }
}
