//$ Copyright 2015-21, Code Respawn Technologies Pvt Ltd - All Rights Reserved $//
using DungeonArchitect.Flow.Impl.SnapGridFlow;

namespace DungeonArchitect.Builders.SnapGridFlow
{
    public class SnapGridFlowConfig : DungeonConfig
    {
        public SnapGridFlowAsset flowGraph;
        public SnapGridFlowModuleDatabase moduleDatabase;
        
        public override bool HasValidConfig(ref string errorMessage)
        {
            if (flowGraph == null)
            {
                errorMessage = "Flow Graph asset is not assigned";
                return false;
            }
            
            if (moduleDatabase == null)
            {
                errorMessage = "Module Database asset is not assigned";
                return false;
            }

            return true;
        }
        
    }
}