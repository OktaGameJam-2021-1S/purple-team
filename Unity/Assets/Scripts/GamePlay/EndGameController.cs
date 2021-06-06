using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Pun;
using UnityEngine.Playables;

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

        public void ShowEndGame(bool win)
        {
            if (win)
            {
                _winCanvas.gameObject.SetActive(true);
                //_winCanvas.GetComponent<PlayableDirector>().Play();
            }
            else
            {
                _loseCanvas.gameObject.SetActive(true);
                _loseCanvas.GetComponent<PlayableDirector>().Play();
            }
        }
    }
}
