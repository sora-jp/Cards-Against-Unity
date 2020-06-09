using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

[Preserve]
public class HeadlessServerInitializer : MonoBehaviour
{
    public int mainMenuScene;
    public int serverScene;

    void Awake()
    {
#if !UNITY_SERVER
        if (Application.isBatchMode)
        {
#endif
            SceneManager.LoadScene(serverScene);
            return;
#if !UNITY_SERVER
        }
#endif

        SceneManager.LoadScene(mainMenuScene);
    }
}
