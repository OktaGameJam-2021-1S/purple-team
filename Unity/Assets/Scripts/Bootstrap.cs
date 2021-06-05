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

        yield return null;

        while (string.IsNullOrEmpty(AuthManager.Instance.localNickname))
            yield return null;

        if (SceneManager.GetActiveScene().name == "Boot")
            yield return SceneManager.LoadSceneAsync("Menu", LoadSceneMode.Additive);

        SceneManager.UnloadSceneAsync("Boot");
    }

    [ContextMenu("PlayersPrefs.ClearAll")]
    private void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
}
