using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIRankingScreen : MonoBehaviour
{
    [SerializeField] private Canvas m_Canvas;
    [SerializeField] private GraphicRaycaster m_InputRaycaster;
    [Space]
    [SerializeField] private RectTransform m_MainRect;
    [SerializeField] private Button m_CloseButton;
    [SerializeField] private Button m_RefreshButton;
    [Space]
    [SerializeField] private RectTransform m_ItemContainer;
    [SerializeField] private UIRankingItem m_ItemPrefab;
    [Space]
    [SerializeField] private RectTransform m_LoadCircle;

    public const string SCENE_NAME = "UIRanking";

    private int m_AnimTweenId;

    private static bool m_ShowTop5;
    private static bool m_ShowTop10;
    private static bool m_ShowLastMatch;

    private static UIRankingScreen m_Instance;

    private void Awake()
    {
        if (m_Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        m_Instance = this;

        m_Canvas.enabled = false;
        m_LoadCircle.gameObject.SetActive(false);

        m_CloseButton.onClick.AddListener(OnClickClose);
        m_RefreshButton.onClick.AddListener(OnClickRefresh);
    }

    private IEnumerator Start()
    {
        yield return null;

        if (!RankingManager.Instance.initialized)
            m_LoadCircle.gameObject.SetActive(true);

        while (!RankingManager.Instance.initialized)
            yield return null;

        m_LoadCircle.gameObject.SetActive(false);
        m_Canvas.enabled = true;

        if (m_ShowTop10)
        {
            InitScroll(RankingManager.Instance.top10);
        }
        else if (m_ShowTop5)
        {
            InitScroll(RankingManager.Instance.top5);
        }
        else
        {
            List<RankingManager.EntryData> rank = new List<RankingManager.EntryData>();
            var data = RankingManager.Instance.lastMatchRank;
            if (data.me != null)
            {
                data.me.isLocalPlayer = true;
                rank.AddRange(data.above5);
                rank.Add(data.me);
            }
            rank.AddRange(data.bottom5);
            InitScroll(rank);
        }

        AnimateShow();
    }

    public void InitScroll(List<RankingManager.EntryData> leaderboard)
    {
        for (int i = 0; i < leaderboard.Count; i++)
        {
            UIRankingItem aux = null;

            if (i < m_ItemContainer.childCount)
                aux = m_ItemContainer.GetChild(i).GetComponent<UIRankingItem>();
            if (aux == null)
                aux = Instantiate(m_ItemPrefab, m_ItemContainer);

            aux.Init(
                leaderboard[i].player1,
                leaderboard[i].player2,
                leaderboard[i].score);
        }
    }

    private void OnClickClose()
    {
        AnimateClose();
    }

    private void OnClickRefresh()
    {
        RankingManager.Instance.RefreshTop10(InitScroll);
    }

    private void AnimateShow()
    {
        LeanTween.cancel(m_AnimTweenId);
        m_MainRect.localScale = Vector3.zero;
        m_AnimTweenId = LeanTween.scale(m_MainRect.gameObject, Vector3.one, 0.4f)
            .setEaseOutBack()
            .setOnStart(() =>
            {
                m_Canvas.enabled = true;
                m_InputRaycaster.enabled = true;
            })
            .setOnComplete(() =>
            {

            })
            .uniqueId;
    }

    private void AnimateClose()
    {
        LeanTween.cancel(m_AnimTweenId);
        m_MainRect.localScale = Vector3.one;
        m_AnimTweenId = LeanTween.scale(m_MainRect.gameObject, Vector3.zero, 0.25f)
            .setEaseInBack()
            .setOnStart(() =>
            {
                m_InputRaycaster.enabled = false;
            })
            .setOnComplete(() =>
            {
                m_Canvas.enabled = false;
                SceneManager.UnloadSceneAsync(SCENE_NAME);
            })
            .uniqueId;
    }

    public static void OpenLastMatch()
    {
        m_ShowTop5 = false;
        m_ShowTop10 = false;
        m_ShowLastMatch = true;
        SceneManager.LoadScene(UIRankingScreen.SCENE_NAME, LoadSceneMode.Additive);
    }

    public static void OpenTop5()
    {
        m_ShowTop5 = true;
        m_ShowTop10 = false;
        m_ShowLastMatch = false;
        SceneManager.LoadScene(UIRankingScreen.SCENE_NAME, LoadSceneMode.Additive);
    }

    public static void OpenTop10()
    {
        m_ShowTop5 = false;
        m_ShowTop10 = true;
        m_ShowLastMatch = false;
        SceneManager.LoadScene(UIRankingScreen.SCENE_NAME, LoadSceneMode.Additive);
    }
}
