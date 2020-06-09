using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadCliAndSrv : MonoBehaviour
{
    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            SceneManager.LoadScene(3);
            SceneManager.LoadScene(2, LoadSceneMode.Additive);
        });
    }
}