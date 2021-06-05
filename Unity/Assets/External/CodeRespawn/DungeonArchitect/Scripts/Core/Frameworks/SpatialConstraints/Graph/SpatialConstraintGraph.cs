//$ Copyright 2015-21, Code Respawn Technologies Pvt Ltd - All Rights Reserved $//
using UnityEngine;
using DungeonArchitect.Graphs;

namespace DungeonArchitect.SpatialConstraints
{
    [System.Serializable]
    public class SpatialConstraintGraph : Graph
    {
        [SerializeField]
        public SpatialConstraintAsset asset;

        public override void OnEnable()
        {
            base.OnEnable();
        }
    }
}
