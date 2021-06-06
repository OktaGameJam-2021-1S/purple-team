using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Pun;
using UnityEngine.Playables;
using System;

namespace GamePlay
{
    public class EndGameController : MonoBehaviour
    {
        [SerializeField] Canvas _winCanvas;
        [SerializeField] Canvas _loseCanvas;

        public void Initialize()
        {

        }

        public void OnBackToMenuClick()
        {
            PhotonNetwork.Disconnect();
            SceneManager.LoadScene("Menu");
        }

        public void ShowWinGame(string userID1, string userID2)
        {
            _winCanvas.gameObject.SetActive(true);
            _winCanvas.GetComponent<PlayableDirector>().Play();
            StartCoroutine(WaitAnimation(_winCanvas.GetComponent<PlayableDirector>(),
            delegate ()
            {
                //TODO: Save score and show leadboard
            }));
        }

        public void ShowLoseGame()
        {
            _loseCanvas.gameObject.SetActive(true);
            _loseCanvas.GetComponent<PlayableDirector>().Play();
        }

        private IEnumerator WaitAnimation(PlayableDirector playable, Action finished = null)
        {
            while (playable.state == PlayState.Playing)
            {
                yield return null;
            }

            finished?.Invoke();
        }
    }
}
