using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadScene : MonoBehaviour
{
    public int scene;

    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => SceneManager.LoadScene(scene));
    }
}
