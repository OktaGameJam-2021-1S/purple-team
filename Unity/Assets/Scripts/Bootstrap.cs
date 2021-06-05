using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void RunOnStart()
    {
        LeanTween.reset();
    }

    private IEnumerator Start()
    {
        DontDestroyOnLoad(this.gameObject);

        if (SceneManager.GetActiveScene().name == "Boot")
            yield return SceneManager.LoadSceneAsync("Menu");
    }
}
