using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class AddressEntryPanel : MonoBehaviour
{
    public TMP_InputField addressField, nameField;
    CanvasGroup m_group;

    void Start()
    {
        m_group = GetComponent<CanvasGroup>();

        if (CardsServer.Instance != null)
        {
            addressField.gameObject.SetActive(false);
        }

        m_group.alpha = 1;
        m_group.interactable = m_group.blocksRaycasts = true;
    }

    public void Connect()
    {
        if (CardsClient.Instance.State != State.Initialized) return;
        CardsClient.Instance.ConnectTo(CardsServer.Instance == null ? addressField.text : "localhost");
        ClientImplementation.Instance.SetName(nameField.text);
        m_group.DOFade(0, 0.5f).SetEase(Ease.InOutCubic);
        m_group.interactable = m_group.blocksRaycasts = false;
    }
}
