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

    private void Awake()
    {
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

        InitScroll(RankingManager.Instance.leaderboard);

        AnimateShow();
    }

    public void InitScroll(List<RankingManager.EntryData> leaderboard)
    {
        UIRankingItem aux = null;
        for (int i = 0; i < leaderboard.Count; i++)
        {
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
        RankingManager.Instance.RefreshLeaderboard(InitScroll);
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
}
