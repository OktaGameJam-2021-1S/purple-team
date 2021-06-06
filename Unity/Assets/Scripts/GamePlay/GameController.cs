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
        [SerializeField] private CreaturesAIManager _creaturesAIManager;
        [SerializeField] private AudioSource _ambientSound;

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

        public void Initialize(PlayerController localPlayer, List<PlayerController> players, List<CreatureAI> creatures, LostKid lostKid, ExitCave exitCave)
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
            _creaturesAIManager.Initialize(this, creatures, _playersList);

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

        public void PlayerDied(PlayerController player)
        {
            _gameState = GameState.End;
            _endGameController.ShowLoseGame();
        }

        public void RescuedKid()
        {
            _gameState = GameState.End;
            _ambientSound.Stop();
            _endGameController.ShowWinGame(_playersList[0].UserID, _playersList.Count > 1? _playersList[1].UserID : "Invalid");
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
            _localPlayer.UpdatePlayerAnimation();

            foreach(PlayerController player in _playersList)
            {
                player.UpdatePlayer(Time.deltaTime);
            }

#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.P))
            {
                RescuedKid();
            } else if (Input.GetKeyDown(KeyCode.L))
            {
                PlayerDied(_localPlayer);
            }
#endif
        }
    }
}
