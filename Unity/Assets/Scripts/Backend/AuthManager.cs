using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParrelSync;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class AuthManager : MonoBehaviour
{
    public class PlayerData
    {
        public string id;
        public string name;
        public int games;
    }

    private const string LOGIN_ENDPOINT = "https://okta-team-purple.herokuapp.com/login/{0}/{1}";

    private static AuthManager m_Instance;
    public static AuthManager Instance
    {
        get
        {
            if (m_Instance == null)
                new GameObject("AuthManager").AddComponent<AuthManager>();
            return m_Instance;
        }
    }

    public bool initialized { get; private set; }

    public string localDeviceId => SystemInfo.deviceUniqueIdentifier + (ClonesManager.IsClone() ? "" : "_clone");
    public string localNickname { get; private set; }
    private string namePrefsKey => $"{localDeviceId}.name";

    public PlayerData player { get; private set; } = null;
    public bool authenticated => player != null;

    private void Awake()
    {
        m_Instance = this;

        if (this.transform.root == this.transform)
            DontDestroyOnLoad(this.gameObject);

        localNickname = "";
        player = null;
    }

    private IEnumerator Start()
    {
        if (PlayerPrefs.HasKey(namePrefsKey))
        {
            localNickname = PlayerPrefs.GetString(namePrefsKey);
            yield return Co_Login(PlayerPrefs.GetString(namePrefsKey), null, null);
        }
        else
        {
            SceneManager.LoadScene(UILogin.SCENE_NAME, LoadSceneMode.Additive);

            while (player == null)
                yield return null;
        }
    }

    private IEnumerator Co_Login(string name, System.Action onSuccess, System.Action<string> onError)
    {
        string endpoint = string.Format(LOGIN_ENDPOINT, localDeviceId, name);
        var op = UnityWebRequest.Get(endpoint).SendWebRequest();
        yield return op;

        if (op.webRequest.responseCode == 200 || op.webRequest.responseCode == 201)
        {
            Debug.Log("authenticated: " + op.webRequest.downloadHandler.text);
            player = Newtonsoft.Json.JsonConvert.DeserializeObject<PlayerData>(op.webRequest.downloadHandler.text);
            localNickname = name;
            PlayerPrefs.SetString(namePrefsKey, player.name);
            onSuccess?.Invoke();
        }
        else
        {
            Debug.LogError($"auth error {op.webRequest.responseCode} {op.webRequest.error}");
            onError?.Invoke(op.webRequest.error);
        }
    }

    public void Authenticate(string name, System.Action onSuccess, System.Action<string> onError)
    {
        StopAllCoroutines();
        StartCoroutine(Co_Login(name, onSuccess, onError));
    }
}
