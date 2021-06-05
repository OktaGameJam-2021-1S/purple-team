using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIRankingItem : MonoBehaviour
{
    [SerializeField] private RectTransform m_AnimRect;
    [Space]
    [SerializeField] private TextMeshProUGUI m_Nickname_1;
    [SerializeField] private TextMeshProUGUI m_Nickname_2;
    [SerializeField] private TextMeshProUGUI m_Score;

    private int m_TweenId;

    public void Init(string player1, string player2, int score)
    {
        m_Nickname_1.text = player1;
        m_Nickname_2.text = player2;
        m_Score.text = score.ToString();
    }

    public void AnimateIn(float delay)
    {
        LeanTween.cancel(m_TweenId);
    }
}
