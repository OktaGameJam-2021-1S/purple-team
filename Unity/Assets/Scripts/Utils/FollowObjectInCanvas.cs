using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public class FollowObjectInCanvas : FollowObject
    {
        protected override void UpdatePosition()
        {
            Vector2 targetScreenPosition = ScreenResolutionConverter.Camera.WorldToScreenPoint(_target.position);
            Vector2 offsetInPixels = ScreenResolutionConverter.ConvertValueToCurrentResolution(_offset);
            transform.position = targetScreenPosition + offsetInPixels;
        }
    }
}
