using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPressButtonPrompt : MonoBehaviour
{
    [SerializeField] private RectTransform m_MainRect;
    [SerializeField] private Canvas m_Canvas;

    private int m_AnimTweenId;

    public static UIPressButtonPrompt Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        m_Canvas.enabled = false;
    }

    [ContextMenu("Show")]
    public void Show()
    {
        m_Canvas.enabled = true;
        LeanTween.cancel(m_AnimTweenId);
        m_MainRect.localScale = Vector3.zero;
        m_AnimTweenId = LeanTween.scale(m_MainRect.gameObject, Vector3.one, 0.4f)
            .setEaseOutBack()
            .setOnStart(() =>
            {
                m_Canvas.enabled = true;
            })
            .uniqueId;
    }

    [ContextMenu("Hide")]
    public void Hide()
    {
        m_Canvas.enabled = false;
        LeanTween.cancel(m_AnimTweenId);
        m_MainRect.localScale = Vector3.one;
        m_AnimTweenId = LeanTween.scale(m_MainRect.gameObject, Vector3.zero, 0.25f)
            .setEaseInBack()
            .setOnComplete(() =>
            {
                m_Canvas.enabled = false;
            })
            .uniqueId;
    }
}
