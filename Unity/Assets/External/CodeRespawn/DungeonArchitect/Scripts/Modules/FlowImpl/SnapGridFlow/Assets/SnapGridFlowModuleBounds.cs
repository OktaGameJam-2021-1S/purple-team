//$ Copyright 2015-21, Code Respawn Technologies Pvt Ltd - All Rights Reserved $//
using UnityEngine;

namespace DungeonArchitect.Flow.Impl.SnapGridFlow
{
    [System.Serializable]
    public class SnapGridFlowModuleBounds : ScriptableObject
    {
        [Tooltip("The world size of a module chunk (1x1x1).  A module can span multiple chunks (e.g 2x2x1)")]
        public Vector3 chunkSize = new Vector3(40, 20, 40);

        [Tooltip("How high do you want the door to be from the lower bounds. This will create a door visual indicator on the bounds actor, aiding your while designing your modules")]
        public float doorOffsetY = 5;

        [Tooltip("The color of the bounds wireframe. Use this bounds as a reference while designing your module prefabs")]
        public Color boundsColor = Color.red;

        [Tooltip("The color of the Door Info. Use this align the doors in your module prefabs")]
        public Color doorColor = Color.blue;
    }
}