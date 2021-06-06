using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Networking;
using UnityEngine.SceneManagement;

namespace Menu
{
    public class MatchMakingMenu : MonoBehaviour
    {
        private MatchMakingManager _matchMakingManager;

        #region UNITY
        private void Awake()
        {
            _matchMakingManager = new MatchMakingManager();
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
