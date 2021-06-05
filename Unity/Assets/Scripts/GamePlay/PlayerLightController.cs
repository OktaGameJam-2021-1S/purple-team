using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
    public class PlayerLightController : MonoBehaviour
    {
        [SerializeField] private Light _pointLight;
        [SerializeField] private Light _flashLight;

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
        }
    }
}
