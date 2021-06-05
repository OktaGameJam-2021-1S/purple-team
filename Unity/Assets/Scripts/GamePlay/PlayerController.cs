using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

namespace GamePlay
{
    public class PlayerController : PhotonView
    {
        [SerializeField] private MovementController _movementController;

        /// <summary>
        /// Initialize the Player Controller
        /// </summary>
        public void Initialize()
        {
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

        public void UpdatePlayerPosition(float deltaTime)
        {
            _movementController.UpdatePosition(deltaTime);
        }
    }
}
