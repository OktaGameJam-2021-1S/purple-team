using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace GamePlay
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] FollowObject _followObject;
        public void Initialize(Transform target)
        {
            _followObject.SetTarget(target);
        }

        public void UpdateCameraPosition()
        {
            _followObject.ForceUpdate();
        }
    }
}
