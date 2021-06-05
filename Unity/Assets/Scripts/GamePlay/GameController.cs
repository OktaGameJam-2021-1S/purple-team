using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private CameraController _cameraController;
        [SerializeField] private InputController _inputController;
        private List<PlayerController> _playersList;
        private PlayerController _localPlayer;
        private LostKid _lostKid;

        private enum GameState
        {
            Idle,
            Main,
            End
        }

        private GameState _gameState = GameState.Idle;

        public void Initialize(PlayerController localPlayer, List<PlayerController> players, LostKid lostKid)
        {
            _gameState = GameState.Idle;
            _localPlayer = localPlayer;
            _playersList = players;
            _lostKid = lostKid;

            _lostKid.Initialize(_playersList);
            _cameraController.Initialize(_localPlayer.transform);
            _inputController.Initialize();

            _localPlayer.Initialize(true);

            foreach (PlayerController player in _playersList)
            {
                if (player == _localPlayer) continue;
                player.Initialize(false);
            }
        }

        public void StartGame()
        {
            _gameState = GameState.Main;
            //TODO: Add code to start the game
        }

        private void ProcessInput(PlayerInput playerInput)
        {
            _localPlayer.ProcessInput(playerInput.axis, playerInput.interactButton);
        }

        private void FixedUpdate()
        {
            if (_gameState != GameState.Main) return;

            _localPlayer.UpdatePlayerPosition(Time.fixedDeltaTime);
            _cameraController.UpdateCameraPosition();
        }

        private void Update()
        {
            if (_gameState != GameState.Main) return;

            _inputController.UpdatePlayerInput();
            ProcessInput(_inputController.PlayerInput);

            foreach(PlayerController player in _playersList)
            {
                player.UpdatePlayer(Time.deltaTime);
            }
        }
    }
}
