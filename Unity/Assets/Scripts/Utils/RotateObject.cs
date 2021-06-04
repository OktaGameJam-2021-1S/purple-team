using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public class RotateObject : MonoBehaviour
    {
        public Vector3 SpeedPerAxis;
        public bool Rotating = true;

        void FixedUpdate()
        {
            if (Rotating)
            {
                transform.eulerAngles += SpeedPerAxis * Time.fixedDeltaTime;
            }
        }
    }
}
