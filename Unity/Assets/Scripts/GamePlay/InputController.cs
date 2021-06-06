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
        private Camera _camera;

        public PlayerInput PlayerInput
        {
            get
            {
                return _playerInput;
            }
        }

        public void Initialize(Camera cam = null)
        {
            _playerInput = new PlayerInput();
            if (cam == null)
                _camera = Camera.main;
            else
                _camera = cam;
        }

        public void UpdatePlayerInput()
        {
            _targetAxis = RotateVector(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), -_camera.transform.parent.eulerAngles.y);

            if (_targetAxis.magnitude > 1)
            {
                _targetAxis.Normalize();
            }

            _playerInput.axis = Vector2.MoveTowards(_playerInput.axis, _targetAxis, _falloffAxis * Time.deltaTime);

            _playerInput.interactButton = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button0);
        }

        private Vector2 RotateVector(float horizontal, float vertical, float degrees)
        {
            float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

            float tx = horizontal;
            float ty = vertical;
            horizontal = (cos * tx) - (sin * ty);
            vertical = (sin * tx) + (cos * ty);

            return new Vector2(horizontal, vertical);
        }
    }
}
