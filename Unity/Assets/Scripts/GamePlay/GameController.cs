using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private CameraController _cameraController;
        private List<PlayerController> _playersList;
        private PlayerController _localPlayer;

        private enum GameState
        {
            Idle,
            Main,
            End
        }

        private GameState _gameState = GameState.Idle;

        public void Initialize(PlayerController localPlayer, List<PlayerController> players)
        {
            _localPlayer = localPlayer;
            _playersList = players;

            _cameraController.Initialize(_localPlayer.transform);
            _gameState = GameState.Idle;
        }

        public void StartGame()
        {
            _gameState = GameState.Main;
            //TODO: Add code to start the game
        }

        private void ProcessInput()
        {
            Vector2 axis = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            if (axis.magnitude > 1)
            {
                axis.Normalize();
            }

            _localPlayer.ProcessInput(axis);
        }

        private void Update()
        {
            if (_gameState != GameState.Main) return;

            ProcessInput();
            _localPlayer.UpdatePlayerPosition(Time.deltaTime);
            _cameraController.UpdateCameraPosition();

        }
    }
}
