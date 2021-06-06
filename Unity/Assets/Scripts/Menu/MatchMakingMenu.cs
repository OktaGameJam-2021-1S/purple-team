using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Networking;
using UnityEngine.SceneManagement;

using UnityEngine.Networking;

namespace Menu
{
    public class MatchMakingMenu : MonoBehaviour
    {
        private MatchMakingManager _matchMakingManager;
        public string URL = "https://okta-team-purple.herokuapp.com/";

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

        #region UI Callbacks
        public void OnStartMatchMakingClick()
        {
            _matchMakingManager.StartMatchMaking(AuthManager.Instance.localDeviceId);
        }

        public void OnCancelMatchMakingClick()
        {
            _matchMakingManager.CancelMatchMaking();
        }
        #endregion

        #region MatchMaking Events Callbacks
        private void OnMatchFounded()
        {
            SceneManager.LoadScene("GamePlay");
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
