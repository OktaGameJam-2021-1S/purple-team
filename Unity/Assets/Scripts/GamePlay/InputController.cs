using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
    public class PlayerInput
    {
        public Vector2 axis;
        public bool interactButton;

        public void ClearInput()
        {
            axis = Vector2.zero;
            interactButton = false;
        }
    }

    public class InputController : MonoBehaviour
    {
        [SerializeField] private float _falloffAxis = 4f;
        private PlayerInput _playerInput;
        private Vector2 _targetAxis;

        public PlayerInput PlayerInput
        {
            get
            {
                return _playerInput;
            }
        }

        public void Initialize()
        {
            _playerInput = new PlayerInput();
        }

        public void UpdatePlayerInput()
        {
            _targetAxis = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            if (_targetAxis.magnitude > 1)
            {
                _targetAxis.Normalize();
            }

            if (_targetAxis.x == 0 && _targetAxis.y == 0)
            {
                _playerInput.axis = Vector2.MoveTowards(_playerInput.axis, _targetAxis, _falloffAxis * Time.deltaTime);
            }
            else
            {
                _playerInput.axis = _targetAxis;
            }

            _playerInput.interactButton = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button0);
        }        
    }
}
