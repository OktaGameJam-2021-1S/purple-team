using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class UILogin : MonoBehaviour
{
    public const string SCENE_NAME = "UILogin";

    [SerializeField] private Canvas m_Canvas;
    [SerializeField] private GraphicRaycaster m_InputRaycaster;
    [SerializeField] private Button m_ConfirmButton;
    [SerializeField] private Button m_SkipButton;
    [SerializeField] private TMP_InputField m_InputField;
    [SerializeField] private RectTransform m_MainRect;

    [SerializeField] private GameObject m_ErrorObject;
    [SerializeField] private GameObject m_LoadOverlay;

    private int m_AnimTweenId;

    private void Awake()
    {
        m_Canvas.enabled = false;
        m_ConfirmButton.onClick.AddListener(OnClickOk);
        m_ErrorObject.SetActive(false);
        m_LoadOverlay.SetActive(false);
        m_SkipButton.onClick.AddListener(OnClickSkip);
    }

    private void Start()
    {
        AnimateShow();
    }

    private void OnClickOk()
    {
        if (m_InputField.text.Length < 3)
        {
            OnError("too short");
            return;
        }

        m_ErrorObject.SetActive(false);
        m_LoadOverlay.SetActive(true);

        AuthManager.Instance.Authenticate(m_InputField.text, AnimateClose, OnError);
    }

    private void OnClickSkip()
    {
        AuthManager.Instance.InitAsGuest();
        AnimateClose();
    }

    private void OnError(string msg)
    {
        m_LoadOverlay.SetActive(false);
        m_ErrorObject.SetActive(true);
        m_InputField.text = "";
    }

    private void AnimateShow()
    {
        LeanTween.cancel(m_AnimTweenId);
        m_MainRect.localScale = Vector3.zero;
        m_AnimTweenId = LeanTween.scale(m_MainRect.gameObject, Vector3.one*2, 0.4f)
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
        m_MainRect.localScale = Vector3.one*2;
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
