//$ Copyright 2015-21, Code Respawn Technologies Pvt Ltd - All Rights Reserved $//
using DungeonArchitect.Flow.Impl.GridFlow;
using UnityEngine;

namespace DungeonArchitect.Builders.GridFlow
{
    public class GridFlowDungeonConfig : DungeonConfig
    {
        public GridFlowAsset flowAsset;
        public Vector3 gridSize = new Vector3(4, 4, 4);
        public bool Mode2D = false;
        
        public override bool IsMode2D()
        {
            return Mode2D;
        }
        
        public override bool HasValidConfig(ref string errorMessage)
        {
            if (flowAsset == null)
            {
                errorMessage = "Flow Asset is not assign in the configuration";
                return false;
            }
            return true;
        }

    }
}
