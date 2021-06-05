using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
    public class PlayerLightController : MonoBehaviour
    {
        [SerializeField] private float _lightDuration = 20;
        [SerializeField] private Light _pointLight;
        [SerializeField] private Light _flashLight;

        /// <summary>
        /// Current power of the light
        /// </summary>
        public float CurrentLightPower
        { 
            get 
            { 
                if (IsLightOn) return _lightPower;
                return 0;
            } 
        }
        /// <summary>
        /// Max duration of the light
        /// </summary>
        public float LightDuration => _lightDuration;
        /// <summary>
        /// Inform if the light is currently on
        /// </summary>
        public bool IsLightOn { get; private set; } = true;

        private float _maxLightInsensity = 0;
        private float _lightPower = 1;
        private LightRefill _closeRefill;

        public void Initialize(bool isLocalPlayer)
        {
            if (isLocalPlayer)
            {
                _flashLight.cullingMask = _flashLight.cullingMask | 1 << LayerMask.NameToLayer("Character");
            }
            else
            {
                _pointLight.gameObject.SetActive(false);
                _flashLight.cullingMask = _flashLight.cullingMask | 1 << LayerMask.NameToLayer("MainCharacter");
            }

            _maxLightInsensity = _flashLight.intensity;
        }

        public void ProcessInput(bool refillLightButton)
        {
            if (refillLightButton && _closeRefill != null)
            {
                RefillLight(_closeRefill.RefillAmount);
            }
        }

        public void TurnOnLight()
        {
            UpdateLightIntensity();
            IsLightOn = true;
            _flashLight.enabled = true;
        }

        public void TurnOffLight()
        {
            IsLightOn = false;
            _flashLight.enabled = false;
        }

        public void UpdateLight(float deltaTime)
        {
            if (IsLightOn)
            {
                _lightPower = Mathf.MoveTowards(CurrentLightPower, 0, deltaTime / LightDuration);
                UpdateLightIntensity();
            }
        }

        public void RefillLight(float amount)
        {
            _lightPower = Mathf.Clamp01(CurrentLightPower + amount);
            UpdateLightIntensity();
        }

        public void SyncLight(bool isLightOn, float lightPower, float deltaTime)
        {
            if (isLightOn)
            {
                TurnOnLight();                
            }
            else
            {
                TurnOffLight();
            }

            _lightPower = lightPower;
            UpdateLight(deltaTime);
            UpdateLightIntensity();
        }

        private void UpdateLightIntensity()
        {
            _flashLight.intensity = _maxLightInsensity * CurrentLightPower;
        }

        private void OnTriggerEnter(Collider other)
        {
            LightRefill lightRefill = other.GetComponent<LightRefill>();

            if (lightRefill != null)
            {
                _closeRefill = lightRefill;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (_closeRefill != null && other.gameObject == _closeRefill.gameObject)
            {
                _closeRefill = null;
            }
        }
    }
}