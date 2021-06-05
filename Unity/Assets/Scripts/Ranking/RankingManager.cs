using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class RankingManager : MonoBehaviour
{
    public class EntryData
    {
        public string player1;
        public string player2;
        public int score;
    }

    private string GET_LEADERBOARD_ENDPOINT = "https://okta-team-purple.herokuapp.com/scores/top10";

    private static RankingManager m_Instance;
    public static RankingManager Instance
    {
        get
        {
            if (m_Instance == null)
                new GameObject("RankingManager").AddComponent<RankingManager>();
            return m_Instance;
        }
    }

    private List<EntryData> m_Leaderboard;
    private System.DateTime m_LastRetrievalTime;

    public double secondsSinceLastRefresh => (System.DateTime.Now - m_LastRetrievalTime).TotalSeconds;
    public bool initialized { get; private set; }
    public List<EntryData> leaderboard => new List<EntryData>(m_Leaderboard);

    public void Awake()
    {
        m_Instance = this;
        DontDestroyOnLoad(this.gameObject);
        m_Leaderboard = new List<EntryData>();
    }

    private IEnumerator Start()
    {
        yield return GetLeaderboard(null);
    }

    public void RefreshLeaderboard(System.Action<List<EntryData>> callback)
    {
        StopAllCoroutines();
        StartCoroutine(GetLeaderboard(callback));
    }

    private IEnumerator GetLeaderboard(System.Action<List<EntryData>> callbck)
    {
        Debug.Log("retrieving leaderboards");
        UnityWebRequestAsyncOperation operation = UnityWebRequest.Get(GET_LEADERBOARD_ENDPOINT).SendWebRequest();
        yield return operation;

        if (operation.webRequest.responseCode == 200)
        {
            Debug.Log("leaderboard retrieved: " + operation.webRequest.downloadHandler.text);
            m_Leaderboard = Newtonsoft.Json.JsonConvert.DeserializeObject<List<EntryData>>(operation.webRequest.downloadHandler.text);
            m_LastRetrievalTime = System.DateTime.Now;
            initialized = true;
            callbck?.Invoke(m_Leaderboard);
        }
        else
        {
            yield return new WaitForSeconds(5);
            yield return GetLeaderboard(callbck);
        }
    }

    [ContextMenu("Open Ranking Screen")]
    public void OpenRankingScreen()
    {
        SceneManager.LoadScene(UIRankingScreen.SCENE_NAME, LoadSceneMode.Additive);
    }
}
