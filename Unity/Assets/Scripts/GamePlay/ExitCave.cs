using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GamePlay
{
    public class ExitCave : MonoBehaviour
    {
        private GameController _gameController;

        public void Initialize(GameController gameController)
        {
            _gameController = gameController;
        }

        private void OnTriggerEnter(Collider other)
        {
            PlayerController player = other.GetComponent<PlayerController>();

            if (player != null && player.HasKid)
            {
                _gameController.RescuedKid();
            }
        }
    }
}
