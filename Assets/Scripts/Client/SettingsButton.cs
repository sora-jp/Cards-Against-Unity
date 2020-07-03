using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsButton : MonoBehaviour
{
    void Update()
    {
        gameObject.SetActive(ClientImplementation.Instance.MyGuid ==
                             ClientImplementation.Instance.State.CurrentServerOwner);
    }
}
