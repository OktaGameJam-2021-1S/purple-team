//$ Copyright 2015-21, Code Respawn Technologies Pvt Ltd - All Rights Reserved $//
using System;
using System.Collections.Generic;
using System.Linq;
using DungeonArchitect.Flow.Domains;
using DungeonArchitect.Flow.Domains.Layout;
using DungeonArchitect.Flow.Domains.Layout.Pathing;
using DungeonArchitect.Flow.Impl.SnapGridFlow.Tasks;
using DungeonArchitect.Utils;
using UnityEngine;

namespace DungeonArchitect.Flow.Impl.SnapGridFlow
{
    public class SnapFlowLayoutNodeGroupGenerator : FlowLayoutNodeGroupGenerator
    {
        private NodeGroupSettings[] groupSettings;
        private static FLocalCoordBuilder coordBuilder = new FLocalCoordBuilder();

        public SnapFlowLayoutNodeGroupGenerator(SnapGridFlowModuleDatabase moduleDatabase)
        {
            if (moduleDatabase != null)
            {
	            // Build the group weights
                var groupWeights = new Dictionary<Vector3Int, float>();
                {
	                var groupCounts = new Dictionary<Vector3Int, int>();
	                foreach (var module in moduleDatabase.Modules)
	                {
		                var chunkSizes = new List<Vector3Int>();
		                chunkSizes.Add(module.NumChunks);
		                if (module.allowRotation)
		                {
			                var rotatedNumChunks = new Vector3Int(module.NumChunks.z, module.NumChunks.y, module.NumChunks.x);
			                chunkSizes.Add(rotatedNumChunks);
		                }
		                foreach (var chunkSize in chunkSizes)
		                {
			                if (!groupWeights.ContainsKey(chunkSize))
			                {
				                groupWeights[chunkSize] = 0;
			                }

			                groupWeights[chunkSize] += module.SelectionWeight;

			                if (!groupCounts.ContainsKey(chunkSize))
			                {
				                groupCounts[chunkSize] = 0;
			                }

			                groupCounts[chunkSize] = groupCounts[chunkSize] + 1;
		                }
	                }

	                // Average out the weights
	                var keys = groupWeights.Keys.ToArray();
	                foreach (var key in keys)
	                {
		                var value = groupWeights[key];
		                var count = groupCounts[key];
		                groupWeights[key] = value / count;
	                }
                }

                var settingList = new List<NodeGroupSettings>();
                foreach (var entry in groupWeights) {
                    
                    var setting = new NodeGroupSettings()
                    {
                        Weight = entry.Value,
                        GroupSize = entry.Key
                    };
                    settingList.Add(setting);
                }

                groupSettings = settingList.ToArray();
            }
            else
            {
                var setting = new NodeGroupSettings()
                {
                    Weight = 1,
                    GroupSize = new Vector3Int(1, 1, 1)

                };
                groupSettings = new NodeGroupSettings[] {setting};
            }
        }

        public override int GetMinNodeGroupSize()
        {
            if (groupSettings.Length == 0) return 1;

            int minGroupSize = int.MaxValue;
            foreach (var groupSetting in groupSettings) {
                var groupSize = groupSetting.GroupSize.x * groupSetting.GroupSize.y * groupSetting.GroupSize.z;
                minGroupSize = Mathf.Min(minGroupSize, groupSize);
            }
            return minGroupSize;
        }

        
        public override FlowLayoutPathNodeGroup[] Generate(FlowLayoutGraphQuery graphQuery, FlowLayoutGraphNode currentNode, System.Random random, HashSet<DungeonUID> visited)
        {
	        var Node = currentNode;
	        if (Node == null) {
		        return new FlowLayoutPathNodeGroup[0];
	        }

	        if (groupSettings.Length == 0) {
		        var nullGenerator = new NullFlowLayoutNodeGroupGenerator();
		        return nullGenerator.Generate(graphQuery, Node, random, visited);
	        }

	        var weightedGroupSettings = new List<NodeGroupSettings>(groupSettings);
            for (var i = 0; i < weightedGroupSettings.Count; i++)
            {
                var setting = weightedGroupSettings[i];
                setting.Weight += random.NextFloat() * 0.0001f;
                weightedGroupSettings[i] = setting;
            }

            var result = new List<FlowLayoutPathNodeGroup>();
	        foreach (var groupSetting in weightedGroupSettings)
            {
                Vector3Int[] LocalSurfaceCoords;
                Vector3Int[] LocalVolumeCoords;
		        coordBuilder.GetCoords(groupSetting.GroupSize, out LocalVolumeCoords, out LocalSurfaceCoords);
		        
		        foreach (var localSurfaceCoord in LocalSurfaceCoords) {
			        bool valid = true;
			        var baseCoord = MathUtils.RoundToVector3Int(Node.coord) - localSurfaceCoord;
			        foreach (var localVolumeCoord in LocalVolumeCoords) {
				        var groupNodeCoord = baseCoord + localVolumeCoord;

                        var testNodeId = graphQuery.GetNodeAtCoord(MathUtils.ToVector3(groupNodeCoord));
                        var testNode = graphQuery.GetNode(testNodeId);
 				        if (testNode == null || visited.Contains(testNode.nodeId) || testNode.active) {
					        valid = false;
					        break;
				        }
			        }

			        if (valid) {
				        // Add this group
                        var newGroup = new FlowLayoutPathNodeGroup();
				        newGroup.IsGroup = true;
				        newGroup.Weight = groupSetting.Weight;
				        foreach (var localVolumeCoord in LocalVolumeCoords) {
					        var nodeCoord = baseCoord + localVolumeCoord;
                            var groupNodeId = graphQuery.GetNodeAtCoord(nodeCoord);
                            var groupNode = graphQuery.GetNode(groupNodeId);
                            if (groupNode != null)
                            {
                                newGroup.GroupNodes.Add(groupNode.nodeId);
                            }
                        }
				        foreach (var surfCoord in LocalSurfaceCoords) {
					        var nodeCoord = baseCoord + surfCoord;
                            var groupNodeId = graphQuery.GetNodeAtCoord(nodeCoord);
                            var groupNode = graphQuery.GetNode(groupNodeId);
					        newGroup.GroupEdgeNodes.Add(groupNode.nodeId);
				        }
                        result.Add(newGroup);
			        }
		        }
	        }

	        MathUtils.Shuffle(result, random);
            return result.ToArray();
        }
        
        struct NodeGroupSettings
        {
            public float Weight;
            public Vector3Int GroupSize;
        }
        class FLocalCoordBuilder {
            public void GetCoords(Vector3Int groupSize, out Vector3Int[] outVolumeCoords, out Vector3Int[] outSurfaceCoords) {
                if (VolumeCoordsMap.ContainsKey(groupSize) && SurfaceCoordsMap.ContainsKey(groupSize))
                {
                    outVolumeCoords = VolumeCoordsMap[groupSize].ToArray();
                    outSurfaceCoords = SurfaceCoordsMap[groupSize].ToArray();
                    return;
                }

                var volumeCoords = new List<Vector3Int>();
                var surfaceCoords = new List<Vector3Int>();
			
                for (int dz = 0; dz < groupSize.z; dz++) {
                    for (int dy = 0; dy < groupSize.y; dy++) {
                        for (int dx = 0; dx < groupSize.x; dx++) {
                            volumeCoords.Add(new Vector3Int(dx, dy, dz));
                            if (dx == 0 || dx == groupSize.x - 1 ||
                                dy == 0 || dy == groupSize.y - 1 ||
                                dz == 0 || dz == groupSize.z - 1) {
                                surfaceCoords.Add(new Vector3Int(dx, dy, dz));
                            }
                        }
                    }
                }
                
                VolumeCoordsMap.Add(groupSize, volumeCoords);
                SurfaceCoordsMap.Add(groupSize, surfaceCoords);
                
                outVolumeCoords = volumeCoords.ToArray();
                outSurfaceCoords = surfaceCoords.ToArray();
            }
	
            private Dictionary<Vector3Int, List<Vector3Int>> VolumeCoordsMap = new Dictionary<Vector3Int, List<Vector3Int>>();
            private Dictionary<Vector3Int, List<Vector3Int>> SurfaceCoordsMap = new Dictionary<Vector3Int, List<Vector3Int>>();
        };
    }

    public class SnapFlowLayoutGraphConstraints : IFlowLayoutGraphConstraints
    {
        private SnapGridFlowModuleDatabase moduleDatabase;
        private ISGFLayoutTaskPathBuilder pathingTask; 

        public SnapFlowLayoutGraphConstraints(SnapGridFlowModuleDatabase moduleDatabase, ISGFLayoutTaskPathBuilder pathingTask)
        {
            this.moduleDatabase = moduleDatabase;
            this.pathingTask = pathingTask;
        }
        
        public bool IsValid(FlowLayoutGraphQuery graphQuery, FlowLayoutGraphNode node, FlowLayoutGraphNode[] incomingNodes)
        {
            var graph = graphQuery.Graph;
            if (graph == null) return false;
            Debug.Assert(node != null && node.pathIndex != -1);


            var allIncomingNodes = new HashSet<FlowLayoutGraphNode>(incomingNodes);

            foreach (var link in graph.Links)
            {
	            if (link.state.type == FlowLayoutGraphLinkType.Unconnected)
	            {
		            continue;
	            }

	            if (link.destination == node.nodeId)
	            {
		            var sourceNode = graphQuery.GetNode(link.source);
		            allIncomingNodes.Add(sourceNode);
	            }
	            if (link.source == node.nodeId)
	            {
		            var destNode = graphQuery.GetNode(link.destination);
		            allIncomingNodes.Add(destNode);
	            }
            }

            FlowLayoutPathNodeGroup Group;
            FFAGConstraintsLink[] ConstraintLinks;
            BuildNodeGroup(graphQuery, node, allIncomingNodes.ToArray(), out Group, out ConstraintLinks);
            
            var nodeSnapData = node.GetDomainData<FlowLayoutNodeSnapDomainData>();
            if (nodeSnapData == null || nodeSnapData.Categories.Length == 0) {
                return false;
            }

            return IsValid(graphQuery, Group, ConstraintLinks, nodeSnapData.Categories);
        }

        public bool IsValid(FlowLayoutGraphQuery graphQuery, FlowLayoutPathNodeGroup group, int pathIndex, int pathLength, FFAGConstraintsLink[] incomingNodes)
        {
            var allowedCategories = pathingTask.GetCategoriesAtNode(pathIndex, pathLength);
            return IsValid(graphQuery, group, incomingNodes.ToArray(), allowedCategories);
        }

        private bool IsValid(FlowLayoutGraphQuery graphQuery, FlowLayoutPathNodeGroup group, FFAGConstraintsLink[] incomingNodes, string[] allowedCategories)
        {
            if (group == null || group.GroupEdgeNodes.Count == 0 || group.GroupNodes.Count == 0) return false;

            // Build the input node assembly
            SgfModuleAssembly Assembly;
            FsgfModuleAssemblyBuilder.Build(graphQuery, group, incomingNodes, out Assembly);

            if (moduleDatabase != null) {
                foreach (var module in moduleDatabase.Modules) {
                    if (allowedCategories.Contains(module.Category)) {
                        var numRotatedAssemblies = module.RotatedAssemblies.Length;
                        for (int stepIdx = 0; stepIdx < numRotatedAssemblies; stepIdx++) {
                            var registeredAssembly = module.RotatedAssemblies[stepIdx];
                            SgfModuleAssemblySideCell[] DoorIndices;
                            if (registeredAssembly.CanFit(Assembly, out DoorIndices)) {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
        
        public static void BuildNodeGroup(FlowLayoutGraphQuery graphQuery, FlowLayoutGraphNode node, FlowLayoutGraphNode[] incomingNodes, 
                out FlowLayoutPathNodeGroup outGroup, out FFAGConstraintsLink[] outConstraintLinks)
        {
	        var graph = graphQuery.Graph;
	        outGroup = new FlowLayoutPathNodeGroup();
	        if (graph == null)
	        {
		        outGroup = null;
		        outConstraintLinks = new FFAGConstraintsLink[0];
		        return;
	        }
	        

			Vector3Int minCoord;
			Vector3Int maxCoord;
			if (node.MergedCompositeNodes.Count <= 1) {
				outGroup.IsGroup = false;
				outGroup.GroupNodes.Add(node.nodeId);
				outGroup.GroupEdgeNodes.Add(node.nodeId);
				minCoord = maxCoord = MathUtils.RoundToVector3Int(node.coord);
			}
			else {
				outGroup.IsGroup = true;
				var minCoordF = node.MergedCompositeNodes[0].coord;
				var maxCoordF = minCoordF;

				foreach (var subNode in node.MergedCompositeNodes) {
					minCoordF = MathUtils.ComponentMin(minCoordF, subNode.coord);
					maxCoordF = MathUtils.ComponentMax(maxCoordF, subNode.coord);
					outGroup.GroupNodes.Add(subNode.nodeId);
				}
				minCoord = MathUtils.RoundToVector3Int(minCoordF);
				maxCoord = MathUtils.RoundToVector3Int(maxCoordF);

				foreach (var subNode in node.MergedCompositeNodes) {
					var coord = MathUtils.RoundToVector3Int(subNode.coord);
					if (coord.x == minCoord.x || coord.y == minCoord.y || coord.z == minCoord.z ||
						coord.x == maxCoord.x || coord.y == maxCoord.y || coord.z == maxCoord.z) {
						outGroup.GroupEdgeNodes.Add(subNode.nodeId);
					}
				}
			}

			var constraintLinkList = new List<FFAGConstraintsLink>();
			
			foreach (var link in graph.Links) {
				if (link.state.type == FlowLayoutGraphLinkType.Unconnected) continue;

				var source = link.sourceSubNode.IsValid() ? link.sourceSubNode : link.source;
				var destination = link.destinationSubNode.IsValid() ? link.destinationSubNode : link.destination;

				var bHostsSource = outGroup.GroupNodes.Contains(source);
				var bHostsDest = outGroup.GroupNodes.Contains(destination);
				if (!bHostsSource && !bHostsDest) continue;
				if (bHostsSource && bHostsDest) continue;

				if (bHostsSource) {
					if (outGroup.GroupEdgeNodes.Contains(source)) {
						var sourceNode = graphQuery.GetNode(source);
						if (sourceNode == null) sourceNode = graphQuery.GetSubNode(source);
						var destinationNode = graphQuery.GetNode(destination);
						if (destinationNode == null) destinationNode = graphQuery.GetSubNode(destination);
						if (sourceNode != null && destinationNode != null) {
							constraintLinkList.Add(new FFAGConstraintsLink(sourceNode, destinationNode));
						}
					}
				}
				else if (bHostsDest) {
					if (outGroup.GroupEdgeNodes.Contains(destination)) {
						var sourceNode = graphQuery.GetNode(source);
						if (sourceNode == null) sourceNode = graphQuery.GetSubNode(source);
						var destinationNode = graphQuery.GetNode(destination);
						if (destinationNode == null) destinationNode = graphQuery.GetSubNode(destination);
						if (sourceNode != null && destinationNode != null) {
							constraintLinkList.Add(new FFAGConstraintsLink(destinationNode, sourceNode));
						}
					}
				}
			}

			var nodeByCoords = new Dictionary<Vector3Int, FlowLayoutGraphNode>();
			foreach (var graphNode in graph.Nodes) {
				if (graphNode.MergedCompositeNodes.Count > 0) {
					foreach (var subNode in graphNode.MergedCompositeNodes) {
						var coord = MathUtils.RoundToVector3Int(subNode.coord);
						nodeByCoords[coord] = subNode;
					}
				}
				else {
					var coord = MathUtils.RoundToVector3Int(graphNode.coord);
					nodeByCoords[coord] = graphNode;
				}
			}

			foreach (var incomingNode in incomingNodes) {
				if (incomingNode == null) continue;
				var innerCoord = MathUtils.RoundToVector3Int(incomingNode.coord);
				innerCoord.x = Mathf.Clamp(innerCoord.x, minCoord.x, maxCoord.x);
				innerCoord.y = Mathf.Clamp(innerCoord.y, minCoord.y, maxCoord.y);
				innerCoord.z = Mathf.Clamp(innerCoord.z, minCoord.z, maxCoord.z);
				if (nodeByCoords.ContainsKey(innerCoord))
				{
					var innerNode = nodeByCoords[innerCoord];
					constraintLinkList.Add(new FFAGConstraintsLink(innerNode, incomingNode));
				}
			}

			outConstraintLinks = constraintLinkList.ToArray();
        }
    }
    
    public class SnapFlowLayoutNodeCreationConstraint : IFlowLayoutNodeCreationConstraint
    {
        public bool CanCreateNodeAt(FlowLayoutGraphNode node, int totalPathLength, int currentPathPosition)
        {
            return true;
        }
    }

    public class FlowLayoutNodeSnapDomainData : IFlowDomainData
    {
        public string[] Categories = new string[0];
        public IFlowDomainData Clone()
        {
            var clone = new FlowLayoutNodeSnapDomainData();
            clone.Categories = new string[Categories.Length];
            Array.Copy(Categories, clone.Categories, Categories.Length);
            return clone;
        }
    }
    
    
    public class SnapGridFlowDomainExtension : IFlowDomainExtension
    {
	    public SnapGridFlowModuleDatabase ModuleDatabase;
    }
}