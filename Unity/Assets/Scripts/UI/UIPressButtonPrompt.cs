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
    }

    [ContextMenu("Hide")]
    public void Hide()
    {
        m_Canvas.enabled = false;
    }
}
