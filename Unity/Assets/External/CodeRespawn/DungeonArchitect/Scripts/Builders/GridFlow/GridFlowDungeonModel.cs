//$ Copyright 2015-21, Code Respawn Technologies Pvt Ltd - All Rights Reserved $//
using UnityEngine;
using DungeonArchitect.Flow.Domains.Layout;
using DungeonArchitect.Flow.Domains.Tilemap;

namespace DungeonArchitect.Builders.GridFlow
{
    public class GridFlowDungeonModel : DungeonModel
    {
        /// <summary>
        /// The high level node based layout graph
        /// </summary>
        [HideInInspector]
        public FlowLayoutGraph layoutGraph;

        /// <summary>
        /// Rasterized tilemap representation of the abstract graph
        /// </summary>
        [HideInInspector]
        public FlowTilemap tilemap;

        /// <summary>
        /// Walls in the grid flow builder can either take up a full tile or are built as edges
        /// This is controlled from the "Initial Tilemap" node's property in the grid flow execution graph
        /// </summary>
        [HideInInspector]
        public bool wallsAsEdges = false;
    }
}
