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
        public int rank;

        public bool isLocalPlayer = false;
    }

    public class MatchScore
    {
        public EntryData me;
        public List<EntryData> above5;
        public List<EntryData> bottom5;
    }
    
    private string GET_TOP10_ENDPOINT = "https://okta-team-purple.herokuapp.com/scores/top10";
    private string GET_TOP5_ENDPOINT = "https://okta-team-purple.herokuapp.com/scores/top5";
    private string GET_SAVESCORE_ENDPOINT = "https://okta-team-purple.herokuapp.com/scores/save/{0}/{1}/{2}";

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

    private List<EntryData> m_Top5;
    private List<EntryData> m_Top10;
    private MatchScore m_LastMatchRank;

    public bool initialized { get; private set; }
    public List<EntryData> top10 => new List<EntryData>(m_Top10);
    public List<EntryData> top5 => new List<EntryData>(m_Top5);
    public MatchScore lastMatchRank => m_LastMatchRank;

    public void Awake()
    {
        m_Instance = this;

        if (this.transform.root == this.transform)
            DontDestroyOnLoad(this.gameObject);

        m_Top5 = new List<EntryData>();
        m_Top10 = new List<EntryData>();
    }

    private IEnumerator Start()
    {
        yield return Co_GetLeaderboard(GET_TOP10_ENDPOINT, null);
    }

    public void RefreshTop10(System.Action<List<EntryData>> callback)
    {
        StartCoroutine(Co_GetLeaderboard(GET_TOP10_ENDPOINT, callback));
    }

    public void RefreshTop5(System.Action<List<EntryData>> callback)
    {
        StartCoroutine(Co_GetLeaderboard(GET_TOP10_ENDPOINT, callback));
    }

    public void SaveScore(string player1, string player2, int score, System.Action<MatchScore> callback)
    {
        StartCoroutine(Co_SaveScore(player1, player2, score, callback));
    }

    private IEnumerator Co_SaveScore(string player1, string player2, int score, System.Action<MatchScore> callback)
    {
        Debug.Log("saving score " + string.Format(GET_SAVESCORE_ENDPOINT, player1, player2, score));
        UnityWebRequestAsyncOperation operation = UnityWebRequest.Get(string.Format(GET_SAVESCORE_ENDPOINT, player1, player2, score)).SendWebRequest();
        yield return operation;

        if (operation.webRequest.responseCode == 200)
        {
            Debug.Log("leaderboard retrieved: " + operation.webRequest.downloadHandler.text);
            m_LastMatchRank = Newtonsoft.Json.JsonConvert.DeserializeObject<MatchScore>(operation.webRequest.downloadHandler.text);
            initialized = true;
            callback?.Invoke(m_LastMatchRank);
        }
        else
        {
            Debug.LogError(operation.webRequest.error);
            callback(new MatchScore());
        }
    }

    private IEnumerator Co_GetLeaderboard(string endpoint, System.Action<List<EntryData>> callback)
    {
        Debug.Log("retrieving leaderboards " + endpoint);
        UnityWebRequestAsyncOperation operation = UnityWebRequest.Get(endpoint).SendWebRequest();
        yield return operation;

        if (operation.webRequest.responseCode == 200)
        {
            Debug.Log("leaderboard retrieved: " + operation.webRequest.downloadHandler.text);
            m_Top10 = Newtonsoft.Json.JsonConvert.DeserializeObject<List<EntryData>>(operation.webRequest.downloadHandler.text);
            initialized = true;
            callback?.Invoke(m_Top10);
        }
        else
        {
            Debug.LogError(operation.webRequest.error);
            callback?.Invoke(new List<EntryData>());
        }
    }

    [ContextMenu("Open Top5")]
    public void OpenTop5() => UIRankingScreen.OpenTop5();
        
    [ContextMenu("Open Top10")]
    public void OpenTop10() => UIRankingScreen.OpenTop10();

    [ContextMenu("Test LastMatch")]
    public void TestSaveScore()
    {
        SaveScore("testp1", "testp2", 1, (rank) => UIRankingScreen.OpenLastMatch());
    }
}
