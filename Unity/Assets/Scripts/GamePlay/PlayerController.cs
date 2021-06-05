using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Utils;

namespace GamePlay
{
    public class PlayerController : MonoBehaviourPun
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
        public void ProcessInput(Vector2 axis)
        {
            _movementController.UpdateAxis(axis);
        }

        public void UpdatePlayer(float deltaTime)
        {
            _movementController.UpdatePosition(deltaTime);
        }
    }
}
