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

        public void ShowWinGame(string userID1, string userID2, int time)
        {
            time = time / 1000;
            int score = Mathf.Clamp(1200 - time, 0, 1200);
            _winCanvas.gameObject.SetActive(true);
            _winCanvas.GetComponent<PlayableDirector>().Play();
            StartCoroutine(WaitSecondsAndCallback(5.2f, delegate ()
            {
                userID1 = "uid1";
                userID2 = "uid2";
                print("Saving Score:" + score + " for users: " + userID1 + " and: " + userID2);
                RankingManager.Instance.SaveScore(userID1, userID2, score, delegate (RankingManager.MatchScore m)
                {
                    print("Opening last match UI");
                    UIRankingScreen.OpenLastMatch();
                });
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

        private IEnumerator WaitSecondsAndCallback(float seconds, Action callback = null)
        {
            yield return new WaitForSeconds(seconds);
            callback?.Invoke();
        }
    }
}
