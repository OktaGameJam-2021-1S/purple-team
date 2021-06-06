using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

using Networking;
using UnityEngine.SceneManagement;

using UnityEngine.Networking;
using Photon.Pun;

namespace Menu
{
    public class MatchMakingMenu : MonoBehaviour
    {

        [SerializeField] private Button playButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private Button leaderboardButton;

        [SerializeField] private Animator lamp1;
        [SerializeField] private Animator lamp2;

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

            while (true)
            {
                lamp1.SetBool("Finded", PhotonNetwork.InRoom);
                lamp2.SetBool("Finded", PhotonNetwork.PlayerList.Length > 1);

                yield return null;
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
                playButton.gameObject.SetActive(false);
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
            startMatchMakingOnce = true;
        }

        private void OnCancelMatchMaking()
        {
            startMatchMakingOnce = true;

            lamp1.gameObject.SetActive(false);
            lamp2.gameObject.SetActive(false);

            StopAllCoroutines();
        }
        #endregion
    }
}
