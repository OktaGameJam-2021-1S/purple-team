using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

using Networking;
using UnityEngine.SceneManagement;

using UnityEngine.Networking;

namespace Menu
{
    public class MatchMakingMenu : MonoBehaviour
    {

        [SerializeField] private Button playButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private Button leaderboardButton;

        [SerializeField] private Image lamp1;
        [SerializeField] private Image lamp2;

        [SerializeField] private float findingMatchAnimationTime = 4;

        private MatchMakingManager _matchMakingManager;
        public string URL = "https://okta-team-purple.herokuapp.com/";
        private bool startMatchMakingOnce = true;
        private Coroutine loadingCoroutine;

        #region Coroutine
        private IEnumerator SendWebRequest()
        {
            UnityWebRequestAsyncOperation operation = UnityWebRequest.Get(URL).SendWebRequest();
            yield return operation;

            Debug.Log("HandShake!");
            Debug.Log(operation.webRequest.downloadHandler.text);
        }

        #endregion
        #region UNITY
        private void Awake()
        {
            _matchMakingManager = new MatchMakingManager();
            StartCoroutine(SendWebRequest());

            playButton.onClick.AddListener(OnStartMatchMakingClick);
            exitButton.onClick.AddListener(ExitButton);
            leaderboardButton.onClick.AddListener(LeaderBoardButton);
        }       

        private void OnEnable()
        {
            _matchMakingManager.MatchFoundedEvent += OnMatchFounded;
            _matchMakingManager.FailedToFindAMatchEvent += OnFailedToFindAMatch;
            _matchMakingManager.CancelMatchmakinEvent += OnCancelMatchMaking;
        }

        private void OnDisable()
        {
            _matchMakingManager.MatchFoundedEvent -= OnMatchFounded;
            _matchMakingManager.FailedToFindAMatchEvent -= OnFailedToFindAMatch;
            _matchMakingManager.CancelMatchmakinEvent -= OnCancelMatchMaking;

        }
        #endregion

        #region Coroutines

        private IEnumerator FindingMatchAnimationCoroutine()
        {
            lamp1.gameObject.SetActive(true);
            lamp2.gameObject.SetActive(true);
            while(true)
            {
                lamp2.gameObject.SetActive(!lamp2.gameObject.activeInHierarchy);
                float counter = 0;
                while(counter < findingMatchAnimationTime)
                {
                    counter += Time.deltaTime;
                    yield return null;
                }
            }
        }

        #endregion

        #region UI Callbacks
        public void OnStartMatchMakingClick()
        {			
            if (startMatchMakingOnce)
            {
                _matchMakingManager.StartMatchMaking(AuthManager.Instance.localDeviceId);
                startMatchMakingOnce = false;
                loadingCoroutine = StartCoroutine(FindingMatchAnimationCoroutine());
            }
        }

        public void ExitButton()
        {
            Application.Quit();
        }

        public void OnCancelMatchMakingClick()
        {
            _matchMakingManager.CancelMatchMaking();
        }
        public void LeaderBoardButton()
        {
            UIRankingScreen.OpenTop10();
        }
        #endregion

        #region MatchMaking Events Callbacks
        private void OnMatchFounded()
        {
            SceneManager.LoadScene("GamePlay");
            if (loadingCoroutine != null)
                loadingCoroutine = StartCoroutine(FindingMatchAnimationCoroutine());
        }

        private void OnFailedToFindAMatch()
        {

        }

        private void OnCancelMatchMaking()
        {

        }
        #endregion
    }
}
