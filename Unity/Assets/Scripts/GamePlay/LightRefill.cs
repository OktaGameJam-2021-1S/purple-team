using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
    public class LightRefill : MonoBehaviour
    {
        [SerializeField] private float _refillAmount = 1;

        public float RefillAmount => _refillAmount;
    }
}
