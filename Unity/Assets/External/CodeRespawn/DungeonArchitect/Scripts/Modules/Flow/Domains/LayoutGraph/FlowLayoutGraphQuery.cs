//$ Copyright 2015-21, Code Respawn Technologies Pvt Ltd - All Rights Reserved $//
using System.Collections.Generic;
using DungeonArchitect.Flow.Items;
using DungeonArchitect.Utils;
using UnityEngine;

namespace DungeonArchitect.Flow.Domains.Layout
{
    public class FlowLayoutGraphTraversal
    {
        private Dictionary<DungeonUID, FNodeInfo[]> outgoingNodes = new Dictionary<DungeonUID, FNodeInfo[]>();
        private Dictionary<DungeonUID, FNodeInfo[]> incomingNodes = new Dictionary<DungeonUID, FNodeInfo[]>();
        private Dictionary<DungeonUID, DungeonUID> teleporters = new Dictionary<DungeonUID, DungeonUID>();   // Node -> Node mapping of teleporters
        
        public void Build(FlowLayoutGraph graph)
        {
            outgoingNodes.Clear();
            incomingNodes.Clear();
            teleporters.Clear();

            if (graph == null)
            {
                return;
            }
            
            var outgoingList = new Dictionary<DungeonUID, List<FNodeInfo>>();
            var incomingList = new Dictionary<DungeonUID, List<FNodeInfo>>();
            
            foreach (var link in graph.Links)
            {
                if (link.state.type == FlowLayoutGraphLinkType.Unconnected)
                {
                    continue;
                }
                
                // Add outgoing nodes
                {
                    if (!outgoingList.ContainsKey(link.source))
                    {
                        outgoingList.Add(link.source, new List<FNodeInfo>());
                    }

                    var info = new FNodeInfo();
                    info.NodeId = link.destination;
                    info.LinkId = link.linkId;
                    info.Outgoing = true;
                    
                    outgoingList[link.source].Add(info);
                }
                
                // Add incoming nodes
                {
                    if (!incomingList.ContainsKey(link.destination))
                    {
                        incomingList.Add(link.destination, new List<FNodeInfo>());
                    }

                    var info = new FNodeInfo();
                    info.NodeId = link.source;
                    info.LinkId = link.linkId;
                    info.Outgoing = false;
                    
                    incomingList[link.destination].Add(info);
                }
            }

            // Finalize the incoming/outgoing list
            {
                foreach (var entry in outgoingList)
                {
                    outgoingNodes.Add(entry.Key, entry.Value.ToArray());
                }
                foreach (var entry in incomingList)
                {
                    incomingNodes.Add(entry.Key, entry.Value.ToArray());
                }
            }
            
            // Build the teleporter list
            {
                // Build a mapping of the teleporter item to their host node mapping
                var teleporterHostMap = new Dictionary<DungeonUID, FlowLayoutGraphNode>();     // Teleporter to owning node map
                foreach (var node in graph.Nodes)
                {
                    if (node == null || !node.active) continue;
                    foreach (var item in node.items)
                    {
                        if (item != null && item.type == FlowGraphItemType.Teleporter)
                        {
                            if (teleporterHostMap.ContainsKey(item.itemId))
                            {
                                teleporterHostMap.Remove(item.itemId);
                            }
                            teleporterHostMap.Add(item.itemId, node);
                        }
                    }
                }
                
                // Make another pass to build the teleporter list
                foreach (var node in graph.Nodes)
                {
                    if (node == null || !node.active) continue;
                    foreach (var item in node.items)
                    {
                        if (item != null && item.type == FlowGraphItemType.Teleporter)
                        {
                            if (item.referencedItemIds.Count > 0)
                            {
                                var otherTeleporterId = item.referencedItemIds[0];
                                if (teleporterHostMap.ContainsKey(otherTeleporterId))
                                {
                                    var teleNodeA = node;
                                    var teleNodeB = teleporterHostMap[otherTeleporterId];

                                    if (!teleporters.ContainsKey(teleNodeA.nodeId))
                                    {
                                        teleporters.Add(teleNodeA.nodeId, teleNodeB.nodeId);
                                    }
                                    if (!teleporters.ContainsKey(teleNodeB.nodeId))
                                    {
                                        teleporters.Add(teleNodeB.nodeId, teleNodeA.nodeId);
                                    }
                                } 
                            }
                        }
                    }
                }
            }
        }

        public FNodeInfo[] GetOutgoingNodes(DungeonUID nodeId)
        {
            if (outgoingNodes.ContainsKey(nodeId))
            {
                return outgoingNodes[nodeId];
            }

            return new FNodeInfo[0];
        }
        
        public FNodeInfo[] GetIncomingNodes(DungeonUID nodeId)
        {
            if (incomingNodes.ContainsKey(nodeId))
            {
                return incomingNodes[nodeId];
            }

            return new FNodeInfo[0];
        }
        
        public FNodeInfo[] GetConnectedNodes(DungeonUID nodeId)
        {
            var connectedNodes = new List<FNodeInfo>();
            connectedNodes.AddRange(GetOutgoingNodes(nodeId));
            connectedNodes.AddRange(GetIncomingNodes(nodeId));
            return connectedNodes.ToArray();
        }

        public bool GetTeleportNode(DungeonUID nodeId, out DungeonUID connectedNodeId)
        {
            if (!teleporters.ContainsKey(nodeId))
            {
                connectedNodeId = DungeonUID.Empty;
                return false;
            }

            connectedNodeId = teleporters[nodeId];
            return true;
        }
        
        public struct FNodeInfo
        {
            public DungeonUID NodeId;
            public DungeonUID LinkId;
            public bool Outgoing;
        }

    }

    public class FlowLayoutGraphQuery
    {
        public FlowLayoutGraphQuery(FlowLayoutGraph graph)
        {
            this.graph = graph;
            Build();
        }
        
        public FlowLayoutGraphTraversal Traversal
        {
            get => traversal;
        }

        public FlowLayoutGraph Graph
        {
            get => graph;
        }
        
        public FlowLayoutGraph GetGraph()
        {
            return graph;
        }
        
        public T GetGraphOfType<T>() where T : FlowLayoutGraph
        {
            return (T)graph;
        }

        public FlowLayoutGraphNode GetNode(DungeonUID nodeId)
        {
            return nodeMap.ContainsKey(nodeId) ? nodeMap[nodeId] : null;
        }

        public FlowLayoutGraphLink GetLink(DungeonUID linkId)
        {
            return linkMap.ContainsKey(linkId) ? linkMap[linkId] : null;
        }

        public FlowLayoutGraphNode GetSubNode(DungeonUID nodeId)
        {
            
            return subNodeMap.ContainsKey(nodeId) ? subNodeMap[nodeId] : null;
        }

        public DungeonUID GetNodeAtCoord(Vector3 coord)
        {
            return coordToNodeMap.ContainsKey(coord) ? coordToNodeMap[coord] : DungeonUID.Empty;
        }
        
        public void Rebuild()
        {
            Build();
        }
        private void Build()
        {
            nodeMap.Clear();
            linkMap.Clear();
            foreach (var node in graph.Nodes)
            {
                nodeMap.Add(node.nodeId, node);
                coordToNodeMap[node.coord] = node.nodeId;
                foreach (var subNode in node.MergedCompositeNodes)
                {
                    subNodeMap[subNode.nodeId] = subNode;
                }
            }
            
            foreach (var link in graph.Links)
            {
                linkMap.Add(link.linkId, link);
            }
            
            traversal.Build(graph);
        }
        
        private FlowLayoutGraph graph;
        private Dictionary<DungeonUID, FlowLayoutGraphNode> nodeMap = new Dictionary<DungeonUID, FlowLayoutGraphNode>();
        private Dictionary<DungeonUID, FlowLayoutGraphLink> linkMap = new Dictionary<DungeonUID, FlowLayoutGraphLink>();
        private FlowLayoutGraphTraversal traversal = new FlowLayoutGraphTraversal();
        private Dictionary<DungeonUID, FlowLayoutGraphNode> subNodeMap = new Dictionary<DungeonUID, FlowLayoutGraphNode>();
        private Dictionary<Vector3, DungeonUID> coordToNodeMap = new Dictionary<Vector3, DungeonUID>();
    }
}