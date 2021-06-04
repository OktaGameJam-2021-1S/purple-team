using UnityEngine;

namespace Utils
{
    public class ScreenResolutionConverter
    {
        /// <summary>
        /// Base resolution. Use this as a reference and recalculate values for different screen resolutions
        /// </summary>
        public static Vector2 BaseResolution { get; private set; } = new Vector2(1920, 1080);

        /// <summary>
        /// The current screen resolution
        /// </summary>
        public static Vector2 CurrentScreenResolution
        {
            get
            {
                if (Camera == null) return BaseResolution;

                return new Vector2(_camera.pixelWidth, _camera.pixelHeight);
            }
        }
        
        /// <summary>
        /// Main Camera
        /// </summary>
        public static Camera Camera
        {
            get
            {
                if (_camera == null)
                {
                    _camera = Camera.main;
                }

                return _camera;
            }
        }
        private static Camera _camera; // private reference to the main camera

        /// <summary>
        /// Convert the pixel value in base resolution to current resolution using height or width as base
        /// </summary>
        /// <param name="value">Value in pixels from base resoltuion</param>
        /// <param name="useHeightAsBase">Inform if it is to use height or width as base</param>
        /// <returns></returns>
        public static float ConvertValueToCurrentResolution(float value, bool useHeightAsBase = true)
        {
            float proportion = useHeightAsBase ?
                 CurrentScreenResolution.y / BaseResolution.y:
                 CurrentScreenResolution.x / BaseResolution.x;

            return value * proportion;
        }

        /// <summary>
        /// Convert the pixel values in base resolution to current resolution
        /// </summary>
        /// <param name="value">Values in pixels from base resolution</param>
        /// <returns></returns>
        public static Vector2 ConvertValueToCurrentResolution(Vector2 value)
        {
            return new Vector2(ConvertValueToCurrentResolution(value.x, false), ConvertValueToCurrentResolution(value.y, true));
        }

        /// <summary>
        /// Convert the pixel values in base resolution to current resolution using height or width as base
        /// </summary>
        /// <param name="value">Values in pixels from base resolution</param>
        /// <param name="useHeightAsBase">Inform if it is to use height or width as base</param>
        /// <returns></returns>
        public static Vector2 ConvertValueToCurrentResolution(Vector2 value, bool useHeightAsBase)
        {
            return new Vector2(ConvertValueToCurrentResolution(value.x, useHeightAsBase), ConvertValueToCurrentResolution(value.y, useHeightAsBase));
        }
    }
}
