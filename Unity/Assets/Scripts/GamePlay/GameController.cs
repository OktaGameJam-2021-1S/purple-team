using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GamePlay
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private CameraController _cameraController;
        [SerializeField] private InputController _inputController;
        [SerializeField] private EndGameController _endGameController;
        private List<PlayerController> _playersList;
        private PlayerController _localPlayer;
        private LostKid _lostKid;
        private ExitCave _exitCave;

        private enum GameState
        {
            Idle,
            Main,
            End
        }

        private GameState _gameState = GameState.Idle;

        public void Initialize(PlayerController localPlayer, List<PlayerController> players, LostKid lostKid, ExitCave exitCave)
        {
            _gameState = GameState.Idle;
            _localPlayer = localPlayer;
            _playersList = players;
            _lostKid = lostKid;
            _exitCave = exitCave;

            _exitCave.Initialize(this);
            _lostKid.Initialize(_playersList);
            _cameraController.Initialize(_localPlayer.transform);
            _inputController.Initialize();
            _endGameController.Initialize();

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

        public void RescuedKid()
        {
            _gameState = GameState.End;
            _endGameController.ShowEndGame(true);
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
