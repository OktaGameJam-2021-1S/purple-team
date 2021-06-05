using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Utils;

namespace GamePlay
{
    public class PlayerController : MonoBehaviourPun, IPunObservable
    {
        [SerializeField] private MovementController _movementController;
        [SerializeField] private PlayerLightController _playerLightController;

        /// <summary>
        /// Initialize the Player Controller
        /// </summary>
        public void Initialize(bool isLocalPlayer)
        {
            Utilities.ChangeObjectLayer(gameObject, LayerMask.NameToLayer((isLocalPlayer ? "MainCharacter" : "Character")));
            _playerLightController.Initialize(isLocalPlayer);
            _movementController.Initialize();
        }

        /// <summary>
        /// Process player input
        /// </summary>
        /// <param name="axis">Input axis</param>
        public void ProcessInput(Vector2 axis, bool interactButton)
        {
            _movementController.UpdateAxis(axis);
            _playerLightController.ProcessInput(interactButton);
        }

        public void UpdatePlayer(float deltaTime)
        {
            _playerLightController.UpdateLight(deltaTime);
        }

        public void UpdatePlayerPosition(float deltaTime)
        {
            _movementController.UpdatePosition(deltaTime);
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(_playerLightController.IsLightOn);
                stream.SendNext(_playerLightController.CurrentLightPower);
                stream.SendNext(PhotonNetwork.ServerTimestamp);
            }
            else
            {
                bool isLightOn = (bool) stream.ReceiveNext();
                float lightPower = (float) stream.ReceiveNext();
                int sendTimeStamp = (int) stream.ReceiveNext();

                float deltaTime = Utilities.DeltaTime(sendTimeStamp, PhotonNetwork.ServerTimestamp);

                _playerLightController.SyncLight(isLightOn, lightPower, deltaTime);
            }
        }
    }
}
