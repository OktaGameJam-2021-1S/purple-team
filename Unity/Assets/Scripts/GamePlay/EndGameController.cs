using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Pun;

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
            }
            else
            {
                _loseCanvas.gameObject.SetActive(true);
            }
        }
    }
}
