//$ Copyright 2015-21, Code Respawn Technologies Pvt Ltd - All Rights Reserved $//
using System.Collections.Generic;
using System.Linq;
using DungeonArchitect.Flow.Domains.Layout;
using DungeonArchitect.Flow.Domains.Layout.Pathing;
using DungeonArchitect.Utils;
using UnityEngine;

namespace DungeonArchitect.Flow.Impl.SnapGridFlow
{
    class SgfModuleItemFitnessCalculator {
        public SgfModuleItemFitnessCalculator(SgfModuleDatabasePlaceableMarkerInfo[] moduleMarkers) {
            foreach (var info in moduleMarkers)
            {
                ModuleMarkers[info.placeableMarkerTemplate] = info.count;
            }
        }

        public int Calculate(string[] markerNames) {
            var availableMarkers = new Dictionary<PlaceableMarker, int>(ModuleMarkers);
            return Solve(markerNames, availableMarkers);
        }

        private static int Solve(string[] markerNames, Dictionary<PlaceableMarker, int> availableMarkers) {
            int numFailed;
            if (availableMarkers.Count > 0) {
                numFailed = SolveImpl(markerNames, 0, availableMarkers);
                Debug.Assert(numFailed >= 0);
            }
            else
            {
                numFailed = markerNames.Length;
            }

            const int FAIL_WEIGHT = 1000000;
            return numFailed * FAIL_WEIGHT;
        }

        // Returns the no. of items failed in the processed sub tree
        private static int SolveImpl(string[] markerNames, int index, Dictionary<PlaceableMarker, int> availableMarkers) {
            if (index == markerNames.Length) {
                return 0;
            }
            
            Debug.Assert(index >= 0 || index < markerNames.Length);

            int bestFrameFailCount = markerNames.Length;
            var markerName = markerNames[index];
            var keys = availableMarkers.Keys.ToArray();
            foreach (var key in keys) {
                var availableMarkerAsset = key;
                int count = availableMarkers[key];
                
                bool canAttachHere = count > 0 && availableMarkerAsset.supportedMarkers.Contains(markerName);
                int frameFailCount = canAttachHere ? 0 : 1;
                if (canAttachHere) {
                    count--;
                }
                frameFailCount += SolveImpl(markerNames, index + 1, availableMarkers);
                if (canAttachHere) {
                    count++;
                }

                availableMarkers[availableMarkerAsset] = count;

                if (frameFailCount < bestFrameFailCount) {
                    bestFrameFailCount = frameFailCount;
                }
                
                if (availableMarkerAsset.supportedMarkers.Length == 1 && availableMarkerAsset.supportedMarkers[0] == markerName) {
                    // Faster bailout
                    break;
                }
            } 
            
            return bestFrameFailCount;
        }

        private Dictionary<PlaceableMarker, int> ModuleMarkers = new Dictionary<PlaceableMarker, int>();
    };
    
    public struct SgfLayoutModuleResolverSettings
    {
        public int Seed;
        public Matrix4x4 BaseTransform;
        public float ModulesWithMinimumDoorsProbability;
        public SnapGridFlowModuleDatabase ModuleDatabase;
        public FlowLayoutGraph LayoutGraph;
    }
    
    public class SgfLayoutModuleResolver
    {
        class FModuleFitCandidate {
            public SgfModuleDatabaseItem ModuleItem;
            public Quaternion ModuleRotation;
            public int AssemblyIndex;
            public SgfModuleAssemblySideCell[] DoorIndices;
            public int Priority = 0;
        };

        
        public static bool Resolve(SgfLayoutModuleResolverSettings settings, out SgfModuleNode[] outModuleNodes)
        {
            if (settings.LayoutGraph == null || settings.ModuleDatabase == null || settings.ModuleDatabase.ModuleBoundsAsset == null)
            {
                outModuleNodes = new SgfModuleNode[]{};
                return false;
            }

            var graph = settings.LayoutGraph;
            var moduleDatabase = settings.ModuleDatabase;
            
            var moduleNodesById = new Dictionary<DungeonUID, SgfModuleNode>();
            var activeModuleDoorIndices = new Dictionary<DungeonUID, SgfModuleAssemblySideCell[]>();
            
            var random = new System.Random(settings.Seed);
            var graphQuery = new FlowLayoutGraphQuery(graph);
            foreach (var node in graph.Nodes) {
                if (!node.active) continue;
                
                FlowLayoutPathNodeGroup group;
                FFAGConstraintsLink[] constraintLinks;
                SnapFlowLayoutGraphConstraints.BuildNodeGroup(graphQuery, node, new FlowLayoutGraphNode[]{}, out group, out constraintLinks);

                var nodeId = node.nodeId;
                SgfModuleNode moduleNode;
                
                // Build the input node assembly
                SgfModuleAssembly nodeAssembly;
                FsgfModuleAssemblyBuilder.Build(graphQuery, group, constraintLinks, out nodeAssembly);

                var fitCandidates = new List<FModuleFitCandidate>();
                
                var categoryNames = new HashSet<string>();
                FlowLayoutNodeSnapDomainData nodeSnapData = node.GetDomainData<FlowLayoutNodeSnapDomainData>();
                if (nodeSnapData != null) {
                    categoryNames = new HashSet<string>(nodeSnapData.Categories);
                }
                else {
                    Debug.LogError("Snap Domain data missing in the abstract graph node");
                }
                
                var moduleItems = new List<SgfModuleDatabaseItem>();
                foreach (string categoryName in categoryNames) {
                    moduleItems.AddRange(moduleDatabase.GetCategoryModules(categoryName));
                }

                var desiredNodeMarkers = new List<string>();
                foreach (var nodeItem in node.items) {
                    if (nodeItem == null) continue;
                    var markerName = nodeItem.markerName.Trim();
                    if (markerName.Length > 0) {
                        desiredNodeMarkers.Add(markerName);
                    }
                }
                
                bool bChooseModulesWithMinDoors = random.NextFloat() < settings.ModulesWithMinimumDoorsProbability;

                var moduleIndices = MathUtils.GetShuffledIndices(moduleItems.Count, random);
                foreach (var moduleIdx in moduleIndices) {
                    var moduleInfo = moduleItems[moduleIdx];
                    if (moduleInfo == null) continue;
                    
                    var itemFitnessCalculator = new SgfModuleItemFitnessCalculator(moduleInfo.AvailableMarkers);
                    int itemFitness = itemFitnessCalculator.Calculate(desiredNodeMarkers.ToArray());
                    var moduleEntryWeight = Mathf.Clamp(moduleInfo.SelectionWeight, 0.0f, 1.0f);

                    var numAssemblies = moduleInfo.RotatedAssemblies.Length;
                    var shuffledAsmIndices = MathUtils.GetShuffledIndices(numAssemblies, random);
                    foreach (var asmIdx in shuffledAsmIndices) {
                        var moduleAssembly = moduleInfo.RotatedAssemblies[asmIdx];
                        SgfModuleAssemblySideCell[] doorIndices;
                        if (moduleAssembly.CanFit(nodeAssembly, out doorIndices))
                        {
                            var candidate = new FModuleFitCandidate();
                            fitCandidates.Add(candidate);
                            
                            candidate.ModuleItem = moduleInfo;
                            candidate.ModuleRotation = Quaternion.AngleAxis(asmIdx * -90, Vector3.up);
                            candidate.AssemblyIndex = asmIdx;
                            candidate.DoorIndices = doorIndices;
                            
                            var gap = 1000;
                            float connectionWeight = bChooseModulesWithMinDoors ? (moduleInfo.Connections.Length * gap) : 0;
                            float moduleWeight = random.Range(1 - moduleEntryWeight, 1.0f) * (gap - 1);
                            
                            candidate.Priority = Mathf.RoundToInt(itemFitness + connectionWeight + moduleWeight);
                        }
                    }
                }

                if (fitCandidates.Count == 0) {
                    outModuleNodes = new SgfModuleNode[]{};
                    return false;
                }
                
                // Find the best fit
                FModuleFitCandidate bestFit = null;
                {
                    var bestFitIndices = new List<int>();
                    var bestFitPriority = int.MaxValue;
                    for (int idx = 0; idx < fitCandidates.Count; idx++) {
                        var candidate = fitCandidates[idx];
                        if (bestFitPriority > candidate.Priority) {
                            bestFitIndices.Clear();
                            bestFitIndices.Add(idx);
                            bestFitPriority = candidate.Priority;
                        }
                        else if (bestFitPriority == candidate.Priority) {
                            bestFitIndices.Add(idx);
                        }
                    }
                    if (bestFitIndices.Count > 0) {
                        var bestFitIdx = bestFitIndices[random.Range(0, bestFitIndices.Count - 1)];
                        bestFit = fitCandidates[bestFitIdx];
                    }
                }
                
                Debug.Assert(bestFit != null);

                // Register using the best fit candidate
                {
                    moduleNode = CreateModuleNode(node, bestFit.ModuleItem);
                    var moduleRotation = bestFit.ModuleRotation;
                    var moduleDBItem = bestFit.ModuleItem;

                    var chunkSize = moduleDatabase.ModuleBoundsAsset.chunkSize;
                    var doorOffsetY = moduleDatabase.ModuleBoundsAsset.doorOffsetY;
                    
                    
                    var halfChunkSize = Vector3.Scale(MathUtils.ToVector3(moduleDBItem.NumChunks), chunkSize) * 0.5f;
                    {
                        //var localCenter = new Vector3(halfChunkSize.x, doorOffsetY, halfChunkSize.z);
                        var localCenter = halfChunkSize;
                        localCenter = moduleRotation * localCenter;
                        var desiredCenter = Vector3.Scale(node.coord, chunkSize);
                        var position = desiredCenter - localCenter;
                        moduleNode.WorldTransform = settings.BaseTransform * Matrix4x4.TRS(position, moduleRotation, Vector3.one);
                    }
                    
                    // Add the doors
                    activeModuleDoorIndices[nodeId] = bestFit.DoorIndices; 
                }
                
                // Register in lookup
                moduleNodesById[nodeId] = moduleNode;
            }

            foreach (var entry in activeModuleDoorIndices) {
                var moduleId = entry.Key;
                var doorSideCells = entry.Value;
                for (var i = 0; i < doorSideCells.Length; i++)
                {
                    foreach (var graphLink in graph.Links) {
                        if (graphLink.state.type == FlowLayoutGraphLinkType.Unconnected) continue;
                        if ((graphLink.source == doorSideCells[i].nodeId || graphLink.sourceSubNode == doorSideCells[i].nodeId)
                            && (graphLink.destination == doorSideCells[i].linkedNodeId || graphLink.destinationSubNode == doorSideCells[i].linkedNodeId)) {
                            // Outgoing Node
                            doorSideCells[i].linkId = graphLink.linkId;
                            break;
                        }
                        else if ((graphLink.source == doorSideCells[i].linkedNodeId || graphLink.sourceSubNode == doorSideCells[i].linkedNodeId)
                                 && (graphLink.destination == doorSideCells[i].nodeId || graphLink.destinationSubNode == doorSideCells[i].nodeId)) {
                            // Incoming Node
                            doorSideCells[i].linkId = graphLink.linkId;
                            break;
                        }
                    }
                }
            }

            foreach (var entry in activeModuleDoorIndices)
            {
                var nodeId = entry.Key;
                var moduleDoorCells = entry.Value;
                
                if (!moduleNodesById.ContainsKey(nodeId))
                {
                    continue;
                }

                var moduleInfo = moduleNodesById[nodeId];
                foreach (var doorInfo in moduleInfo.Doors)
                {
                    doorInfo.CellInfo = SgfModuleAssemblySideCell.Empty;
                }
                
                foreach (var doorCell in moduleDoorCells)
                {
                    var doorInfo = moduleInfo.Doors[doorCell.connectionIdx];
                    doorInfo.CellInfo = doorCell;
                }
            }
            
            // Link the module nodes together
            foreach (var graphLink in graph.Links) {
                if (graphLink == null || graphLink.state.type == FlowLayoutGraphLinkType.Unconnected) continue;
                
                var sourceId = graphLink.source;
                var destId = graphLink.destination;

                SgfModuleAssemblySideCell srcCell = SgfModuleAssemblySideCell.Empty;
                SgfModuleAssemblySideCell dstCell = SgfModuleAssemblySideCell.Empty;
                bool foundSrcCell = false;
                bool foundDstCell = false;
                
                {
                    if (activeModuleDoorIndices.ContainsKey(sourceId))
                    {
                        var sourceDoorCells = activeModuleDoorIndices[sourceId];
                        foreach (var sourceDoorCell in sourceDoorCells)
                        {
                            if (sourceDoorCell.linkId == graphLink.linkId)
                            {
                                srcCell = sourceDoorCell;
                                foundSrcCell = true;
                                break;
                            }
                        }
                    }

                    if (activeModuleDoorIndices.ContainsKey(destId))
                    {
                        var destDoorCells = activeModuleDoorIndices[destId];
                        foreach (var destDoorCell in destDoorCells)
                        {
                            if (destDoorCell.linkId == graphLink.linkId)
                            {
                                dstCell = destDoorCell;
                                foundDstCell = true;
                                break;
                            }
                        }
                    }
                }

                if (!foundSrcCell || !foundDstCell) {
                    outModuleNodes = new SgfModuleNode[]{};
                    return false;
                }
                
                if (!moduleNodesById.ContainsKey(sourceId) || !moduleNodesById.ContainsKey(destId)) {
                    outModuleNodes = new SgfModuleNode[]{};
                    return false;
                }

                var srcModule = moduleNodesById[sourceId];
                var dstModule = moduleNodesById[destId];
                var srcDoor = srcModule.Doors[srcCell.connectionIdx];
                var dstDoor = dstModule.Doors[dstCell.connectionIdx];

                srcDoor.ConnectedDoor = dstDoor;
                dstDoor.ConnectedDoor = srcDoor;

                srcModule.Outgoing.Add(srcDoor);
                dstModule.Incoming.Add(dstDoor);
            }

            outModuleNodes = moduleNodesById.Values.ToArray();
            return true;
        }

        private static SgfModuleNode CreateModuleNode(FlowLayoutGraphNode layoutNode, SgfModuleDatabaseItem item)
        {
            var node = new SgfModuleNode();
            node.ModuleInstanceId = layoutNode.nodeId;
            node.ModuleDBItem = item;
            node.LayoutNode = layoutNode;

            var nodeDoors = new List<SgfModuleDoor>();
            foreach (var doorInfo in item.Connections) {
                var door = new SgfModuleDoor();
                door.LocalTransform = doorInfo.Transform;
                door.Owner = node;
                nodeDoors.Add(door);
            }

            node.Doors = nodeDoors.ToArray();

            return node;
        }
    }
}