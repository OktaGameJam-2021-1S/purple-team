using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDungeonMap : MonoBehaviour
{
    [SerializeField] private Canvas m_Canvas;
    [SerializeField] private RectTransform m_Panel;

    private int m_TweenId;
    public bool startActive = false;

    private void Awake()
    {
        m_Canvas.enabled = startActive;
    }

    [ContextMenu("Show")]
    public void Show()
    {
        LeanTween.cancel(m_TweenId);
        m_Panel.localScale = Vector3.zero;
        m_TweenId = LeanTween.scale(m_Panel.gameObject, Vector3.one, 0.4f)
            .setEaseOutBack()
            .setOnStart(() =>
            {
                m_Canvas.enabled = true;
            })
            .uniqueId;
    }

    [ContextMenu("Close")]
    public void Close()
    {
        LeanTween.cancel(m_TweenId);
        m_Panel.localScale = Vector3.one;
        m_TweenId = LeanTween.scale(m_Panel.gameObject, Vector3.zero, 0.25f)
            .setEaseInBack()
            .setOnComplete(() =>
            {
                m_Canvas.enabled = false;
            })
            .uniqueId;
    }
}
