//$ Copyright 2015-21, Code Respawn Technologies Pvt Ltd - All Rights Reserved $//
using System.Collections.Generic;
using DungeonArchitect.Flow.Items;
using DungeonArchitect.Utils;
using UnityEngine;

namespace DungeonArchitect.Flow.Domains.Layout.Pathing
{
    public interface IFlowLayoutNodeCreationConstraint
    {
        bool CanCreateNodeAt(FlowLayoutGraphNode node, int totalPathLength, int currentPathPosition);
    }

    public class NullFlowLayoutNodeCreationConstraint : IFlowLayoutNodeCreationConstraint
    {
        public bool CanCreateNodeAt(FlowLayoutGraphNode node, int totalPathLength, int currentPathPosition)
        {
            return true;
        }
    }
    
    public class FlowLayoutStaticGrowthState {
        public FlowLayoutGraph Graph;
        public FlowLayoutGraphQuery GraphQuery;
        public FlowLayoutGraphNode HeadNode = null;
        public List<FlowLayoutGraphNode> SinkNodes = new List<FlowLayoutGraphNode>();
        public System.Random Random;
        public int MinPathSize;
        public int MaxPathSize;
        public Color NodeColor;
        public string PathName;
        public string StartNodePathNameOverride = "";
        public string EndNodePathNameOverride = "";
        public IFlowLayoutGraphConstraints GraphConstraint;
        public FlowLayoutNodeGroupGenerator NodeGroupGenerator;
        public IFlowLayoutNodeCreationConstraint NodeCreationConstraint;
    };

    public class FlowLayoutGrowthStatePathItem {
        public DungeonUID NodeId;
        public DungeonUID PreviousNodeId;

        public FlowLayoutGrowthStatePathItem Clone()
        {
            var clone = new FlowLayoutGrowthStatePathItem();
            clone.NodeId = NodeId;
            clone.PreviousNodeId = PreviousNodeId;
            return clone;
        }
    };

    public class FlowLayoutGrowthState {
        public List<FlowLayoutGrowthStatePathItem> Path = new List<FlowLayoutGrowthStatePathItem>();
        public HashSet<DungeonUID> Visited = new HashSet<DungeonUID>();
        public List<FlowLayoutGraphNodeGroup> NodeGroups = new List<FlowLayoutGraphNodeGroup>();
        public FlowLayoutGraphNode TailNode = null;

        public FlowLayoutGrowthState Clone()
        {
            var clone = new FlowLayoutGrowthState();
            clone.Visited = new HashSet<DungeonUID>(Visited);
            clone.TailNode = TailNode;
            
            foreach (var path in Path)
            {
                clone.Path.Add(path.Clone());
            }
            
            foreach (var group in NodeGroups)
            {
                clone.NodeGroups.Add(group.Clone());
            }
            
            return clone;
        }
    };

    class FlowLayoutGraphPathUtils {
    
        //static bool GrowPath(const UFlowAbstractNode* CurrentNode, const FFlowAGStaticGrowthState& StaticState, FFlowAGGrowthState& OutState);
        public static void FinalizePath(FlowLayoutStaticGrowthState staticState, FlowLayoutGrowthState state)
        {
            var path = state.Path;

            if (path.Count == 0) {
                return;
            }
            
            // Create merged node groups
            foreach (var groupInfo in state.NodeGroups) {
                CreateMergedCompositeNode(staticState.GraphQuery, groupInfo);
            }
            
            FlowLayoutGraph graph = staticState.GraphQuery.GetGraph();
            var childToParentMap = new Dictionary<DungeonUID, DungeonUID>(); // [ChildNodeId -> ParentNodeId]
            foreach (var parentNode in graph.Nodes) {
                if (parentNode.MergedCompositeNodes.Count > 1) {
                    foreach (var childNode in parentNode.MergedCompositeNodes) {
                        childToParentMap[childNode.nodeId] = parentNode.nodeId;
                    }
                }
            }

            var pathLength = path.Count;
            for (int i = 0; i < pathLength; i++) {
                var pathItem = path[i];
                var origNodeId = pathItem.NodeId;
                var origPrevNodeId = pathItem.PreviousNodeId;
                if (childToParentMap.ContainsKey(pathItem.NodeId))
                {
                    pathItem.NodeId = childToParentMap[pathItem.NodeId];
                }

                if (childToParentMap.ContainsKey(pathItem.PreviousNodeId))
                {
                    pathItem.PreviousNodeId = childToParentMap[pathItem.PreviousNodeId];
                }
                
                FlowLayoutGraphNode pathNode = staticState.GraphQuery.GetNode(pathItem.NodeId);
                if (pathNode == null) continue;
                pathNode.active = true;
                pathNode.color = staticState.NodeColor;
                pathNode.pathIndex = i;
                pathNode.pathLength = pathLength;

                string pathName;
                if (i == 0 && staticState.StartNodePathNameOverride.Length > 0) {
                    pathName = staticState.StartNodePathNameOverride;
                }
                else if (i == path.Count - 1 && staticState.EndNodePathNameOverride.Length > 0) {
                    pathName = staticState.EndNodePathNameOverride;
                }
                else {
                    pathName = staticState.PathName;
                }
                pathNode.pathName = pathName;
                

                // Link the path nodes
                if (i > 0) {
                    var linkSrc = pathItem.PreviousNodeId;
                    var linkDst = pathItem.NodeId;
                    var linkSrcSub = origPrevNodeId;
                    var linkDstSub = origNodeId;
                    
                    var possibleLinks = staticState.Graph.GetLinks(linkSrc, linkDst, true);
                    foreach (var possibleLink in possibleLinks) {
                        if (possibleLink == null) continue;
                        if (possibleLink.source == linkSrc && possibleLink.destination == linkDst) {
                            bool bValid = (!possibleLink.sourceSubNode.IsValid() || possibleLink.sourceSubNode == linkSrcSub);
                            bValid &= (!possibleLink.destinationSubNode.IsValid() || possibleLink.destinationSubNode == linkDstSub);

                            // Found the correct link
                            if (bValid) {
                                possibleLink.state.type = FlowLayoutGraphLinkType.Connected;
                                break;
                            }
                        }
                        else if (possibleLink.source == linkDst && possibleLink.destination == linkSrc) {
                            bool bValid = (!possibleLink.sourceSubNode.IsValid() || possibleLink.sourceSubNode == linkDstSub);
                            bValid &= (!possibleLink.destinationSubNode.IsValid() || possibleLink.destinationSubNode == linkSrcSub);

                            // Found the correct link
                            if (bValid) {
                                possibleLink.state.type = FlowLayoutGraphLinkType.Connected;
                                possibleLink.ReverseDirection();
                                break;
                            }
                        }
                    }
                }
            }

            // Setup the start / end links
            if (staticState.HeadNode != null) {
                var linkSrc = staticState.HeadNode.nodeId;
                var linkDst = path[0].NodeId;
                var headLink = staticState.Graph.GetLink(linkSrc, linkDst, true);
                if (headLink != null) {
                    headLink.state.type = FlowLayoutGraphLinkType.Connected;
                    if (headLink.source == linkDst && headLink.destination == linkSrc) {
                        headLink.ReverseDirection();
                    }
                }
            }

            // Find the end node, if any so that it can merge back to the specified branch (specified in variable EndOnPath)
            if (state.TailNode != null) {
                var linkSrc = path[path.Count - 1].NodeId;
                var linkDst = state.TailNode.nodeId;
                var tailLink = staticState.Graph.GetLink(linkSrc, linkDst, true);
                if (tailLink != null) {
                    tailLink.state.type = FlowLayoutGraphLinkType.Connected;
                    if (tailLink.source == linkDst && tailLink.destination == linkSrc) {
                        tailLink.ReverseDirection();
                    }
                }
            }
        }
        
        static FlowLayoutGraphNode CreateMergedCompositeNode(FlowLayoutGraphQuery graphQuery, FlowLayoutGraphNodeGroup nodeGroup)
        {
            if (nodeGroup.GroupNodes.Count <= 1) {
                return null;
            }
    
            var graph = graphQuery.GetGraph();
            var subNodes = new HashSet<FlowLayoutGraphNode>();
            var subNodeIds = new HashSet<DungeonUID>();
            var subItems = new HashSet<FlowItem>();

            var previewLocation = Vector3.zero;
            var coord = Vector3.zero;
            foreach (var subNodeId in nodeGroup.GroupNodes) {
                FlowLayoutGraphNode subNode = graph.GetNode(subNodeId);
                if (subNode == null) continue;
                subNodes.Add(subNode);
                subNodeIds.Add(subNodeId);
                foreach (var item in subNode.items)
                {
                    subItems.Add(item);
                }
                coord += subNode.coord;
                previewLocation += subNode.position;
            }
            var numSubNodes = subNodes.Count;
            if (numSubNodes > 0) {
                coord /= numSubNodes;
                previewLocation /= numSubNodes;

                FlowLayoutGraphNode newNode = graph.CreateNode();
                newNode.active = true;
                newNode.items = new List<FlowItem>(subItems);
                newNode.coord = coord;
                newNode.position = previewLocation;
                newNode.MergedCompositeNodes = new List<FlowLayoutGraphNode>(subNodes);

                // Remove all the sub nodes from the graph 
                foreach (FlowLayoutGraphNode subNode in subNodes) {
                    graph.Nodes.Remove(subNode);
                }

                foreach (FlowLayoutGraphLink link in graph.Links) {
                    if (subNodeIds.Contains(link.source)) {
                        link.sourceSubNode = link.source;
                        link.source = newNode.nodeId;
                    }
                    if (subNodeIds.Contains(link.destination)) {
                        link.destinationSubNode = link.destination;
                        link.destination = newNode.nodeId;
                    }
                }

                var filteredLinks = new List<FlowLayoutGraphLink>();
                foreach (var link in graph.Links)
                {
                    if (link.source != link.destination)
                    {
                        filteredLinks.Add(link);
                    }
                }

                graph.Links = filteredLinks;
                graphQuery.Rebuild();
                return newNode;
            }

            return null;
        }
    };

    public class FlowLayoutPathNodeGroup 
    {
        public bool IsGroup = false;
        public float Weight = 1.0f;
        public List<DungeonUID> GroupNodes = new List<DungeonUID>();        // The list of nodes that belong to this node
        public List<DungeonUID> GroupEdgeNodes = new List<DungeonUID>();     // The list of nodes on the edge of the group (so they can connect to other nodes)
    };


    public abstract class FlowLayoutNodeGroupGenerator
    {
        public abstract FlowLayoutPathNodeGroup[] Generate(FlowLayoutGraphQuery graphQuery, FlowLayoutGraphNode currentNode, System.Random random, HashSet<DungeonUID> visited);
        
        public virtual int GetMinNodeGroupSize() { return 1; }
    }

    public class NullFlowLayoutNodeGroupGenerator : FlowLayoutNodeGroupGenerator
    {
        public override FlowLayoutPathNodeGroup[] Generate(FlowLayoutGraphQuery graphQuery, FlowLayoutGraphNode currentNode, System.Random random, HashSet<DungeonUID> visited)
        {
            if (currentNode == null)
            {
                return new FlowLayoutPathNodeGroup[0];
            }

            var group = new FlowLayoutPathNodeGroup();
            group.IsGroup = false;
            group.GroupNodes.Add(currentNode.nodeId);
            group.GroupEdgeNodes.Add(currentNode.nodeId);
            return new FlowLayoutPathNodeGroup[] { group };
        }
    }
    
    class FlowLayoutPathStackFrame {
        public FlowLayoutGraphNode CurrentNode;
        public FlowLayoutGraphNode IncomingNode;
        public FlowLayoutGrowthState State = new FlowLayoutGrowthState();
    };

    class FFlowLayoutPathingSystemResult
    {
        public FFlowLayoutPathingSystemResult()
        {
        }

        public FFlowLayoutPathingSystemResult(FlowLayoutGrowthState state, FlowLayoutStaticGrowthState staticState)
        {
            this.State = state;
            this.StaticState = staticState;
        }

        public FlowLayoutGrowthState State;
        public FlowLayoutStaticGrowthState StaticState;
    }
    
    class FlowPathGrowthSystem : StackSystem<FlowLayoutPathStackFrame, FlowLayoutStaticGrowthState, FFlowLayoutPathingSystemResult>
    {
        public FlowPathGrowthSystem(FlowLayoutStaticGrowthState staticState) : base(staticState)
        {
        }
    }

    /**
     * Maintains a list of growth systems and runs them in parallel till the first solution is found.
     * This also avoids a single solution to from getting stuck and taking a very long time,
     * as multiple paths are being explored at the same time
    */
    class FFlowAgPathingSystem
    {
        public bool FoundResult
        {
            get => foundResult;
        }

        public bool Timeout
        {
            get => timeout;
        }

        public FFlowLayoutPathingSystemResult Result
        {
            get => result;
        }

        public FFlowAgPathingSystem(long maxFramesToProcess)
        {
            this.maxFramesToProcess = maxFramesToProcess;
        }

        public void RegisterGrowthSystem(FlowLayoutGraphNode startNode, FlowLayoutStaticGrowthState staticState, int count = 1)
        {
            Debug.Assert(count > 0);

            for (int i = 0; i < count; i++)
            {
                var initFrame = new FlowLayoutPathStackFrame();
                initFrame.CurrentNode = startNode;
                initFrame.IncomingNode = null;
                var growthSystem = new FlowPathGrowthSystem(staticState);
                growthSystem.Initialize(initFrame);
                growthSystems.Add(growthSystem);
            }
        }

        public void Execute(int numParallelSearches)
        {
            numParallelSearches = Mathf.Max(numParallelSearches, 1);

            frameCounter = 0;
            for (int i = 0; i < growthSystems.Count; i += numParallelSearches)
            {
                var startIdx = i;
                var endIdx = Mathf.Min(i + numParallelSearches - 1, growthSystems.Count - 1);
                ExecuteImpl(startIdx, endIdx);

                if (foundResult || timeout)
                {
                    break;
                }
            }
        }
        
        private void ExecuteImpl(int startIdx, int endIdx)
        {
            bool running = true;
            while (running && !timeout && !foundResult)
            {
                running = false;
                for (int i = startIdx; i <= endIdx; i++)
                {
                    var growthSystem = growthSystems[i];
                    if (growthSystem.Running)
                    {
                        growthSystem.ExecuteStep(FlowLayoutPathStackGrowthTask.Execute);

                        running |= growthSystem.Running;
                        if (growthSystem.FoundResult)
                        {
                            foundResult = true;
                            result = growthSystem.Result;
                            break;
                        }

                        frameCounter++;
                        if (frameCounter >= maxFramesToProcess)
                        {
                            timeout = true;
                            break;
                        }
                    }
                }
            }
        }


        private List<FlowPathGrowthSystem> growthSystems = new List<FlowPathGrowthSystem>();
        private bool foundResult = false;
        private bool timeout = false;
        private long frameCounter = 0;
        private long maxFramesToProcess = 0;
        private FFlowLayoutPathingSystemResult result;
    }

    class FlowLayoutPathStackGrowthTask
    {
        public static void Execute(FlowLayoutPathStackFrame inFrameState, FlowLayoutStaticGrowthState staticState,
                StackSystem<FlowLayoutPathStackFrame, FlowLayoutStaticGrowthState, FFlowLayoutPathingSystemResult> stackSystem)
        {
            Debug.Assert(staticState.MinPathSize > 0 && staticState.MaxPathSize > 0);
            Debug.Assert(staticState.GraphQuery != null);

            var state = inFrameState.State;
            var currentNode = inFrameState.CurrentNode;
            var incomingNode = inFrameState.IncomingNode;

            var pathIndex = state.Path.Count;
            var pathLength = Mathf.Clamp(pathIndex + 1, staticState.MinPathSize, staticState.MaxPathSize);
            if (pathIndex == 0 && staticState.HeadNode != null) {
                // Check if we can connect from the head node to this node
                if (!staticState.GraphConstraint.IsValid(staticState.GraphQuery, staticState.HeadNode, new FlowLayoutGraphNode[]{currentNode})) {
                    return;
                }
            }

            if (staticState.NodeCreationConstraint != null) {
                if (!staticState.NodeCreationConstraint.CanCreateNodeAt(currentNode, pathLength, pathIndex)) {
                    return;
                }
            }

            bool bFirstNodeInPath = (pathIndex == 0);

            var baseIncomingConstraintLinks = new List<FFAGConstraintsLink>();
            if (bFirstNodeInPath && staticState.HeadNode != null) {
                var headSubNode = staticState.HeadNode;
                if (staticState.HeadNode.MergedCompositeNodes.Count > 1) {
                    foreach (var graphLink in staticState.Graph.Links) {
                        if (graphLink.state.type != FlowLayoutGraphLinkType.Unconnected) continue;
                        if (graphLink.source == currentNode.nodeId && graphLink.destination == staticState.HeadNode.nodeId) {
                            headSubNode = staticState.GraphQuery.GetSubNode(graphLink.destinationSubNode);
                            Debug.Assert(headSubNode != null);
                            break;
                        }
                        else if (graphLink.source == staticState.HeadNode.nodeId && graphLink.destination == currentNode.nodeId) {
                            headSubNode = staticState.GraphQuery.GetSubNode(graphLink.sourceSubNode);
                            Debug.Assert(headSubNode != null);
                            break;
                        }
                    }
                }
                baseIncomingConstraintLinks.Add(new FFAGConstraintsLink(currentNode, headSubNode));
            }
            if (incomingNode != null) {
                baseIncomingConstraintLinks.Add(new FFAGConstraintsLink(currentNode, incomingNode));
            }

            Debug.Assert(staticState.NodeGroupGenerator != null);
            FlowLayoutPathNodeGroup[] possibleNodeGroupsArray = staticState.NodeGroupGenerator.Generate(staticState.GraphQuery, currentNode, staticState.Random, state.Visited);
            var possibleNodeGroups = new List<FlowLayoutPathNodeGroup>(possibleNodeGroupsArray);

            MathUtils.Shuffle(possibleNodeGroups, staticState.Random);

            {
                float selectionThreshold = staticState.Random.NextFloat();
                var hiPriorityGroups = new List<FlowLayoutPathNodeGroup>();
                for (int i = 0; i < possibleNodeGroups.Count; i++) {
                    if (possibleNodeGroups[i].Weight > selectionThreshold) {
                        hiPriorityGroups.Add(possibleNodeGroups[i]);
                        possibleNodeGroups.RemoveAt(i);
                        i--;
                    }
                }
                possibleNodeGroups.AddRange(hiPriorityGroups);
            }
            
            foreach (var growthNodeGroup in possibleNodeGroups) {
                // Check if we can use this newly created group node by connecting in to it
                if (!staticState.GraphConstraint.IsValid(staticState.GraphQuery, growthNodeGroup, pathIndex, pathLength, baseIncomingConstraintLinks.ToArray())) continue;

                FlowLayoutGrowthState nextState = state.Clone();

                // Update the frame path and visited state
                foreach (var groupNode in growthNodeGroup.GroupNodes)
                {
                    nextState.Visited.Add(groupNode);
                }
                
                var pathFrame = new FlowLayoutGrowthStatePathItem();
                pathFrame.NodeId = currentNode.nodeId;
                pathFrame.PreviousNodeId = incomingNode != null ? incomingNode.nodeId : DungeonUID.Empty;
                nextState.Path.Add(pathFrame);

                // Add path node group info
                if (growthNodeGroup.IsGroup) {
                    var nodeGroup = new FlowLayoutGraphNodeGroup();
                    nodeGroup.GroupId = DungeonUID.NewUID();
                    nodeGroup.GroupNodes = growthNodeGroup.GroupNodes;
                    nextState.NodeGroups.Add(nodeGroup);
                }

                // Check if we reached the desired path size
                if (nextState.Path.Count >= staticState.MinPathSize) {
                    // Check if we are near the sink node, if any
                    if (staticState.SinkNodes.Count == 0) {
                        // No sink nodes defined.
                        var result = new FFlowLayoutPathingSystemResult(nextState, staticState);
                        stackSystem.FinalizeResult(result);
                        return;
                    }

                    {
                        var sinkNodeIndices = MathUtils.GetShuffledIndices(staticState.SinkNodes.Count, staticState.Random);
                        var groupEdgeNodeIndices = MathUtils.GetShuffledIndices(growthNodeGroup.GroupEdgeNodes.Count, staticState.Random);
                        foreach (var groupEdgeNodeIndex in groupEdgeNodeIndices) {
                            var groupEdgeNodeId = growthNodeGroup.GroupEdgeNodes[groupEdgeNodeIndex];
                            var connectedNodeIds = staticState.Graph.GetConnectedNodes(groupEdgeNodeId);
                            var connectedNodeIndices = MathUtils.GetShuffledIndices(connectedNodeIds.Length, staticState.Random);
                            foreach (var connectedNodeIndex in connectedNodeIndices) {
                                var connectedNodeId = connectedNodeIds[connectedNodeIndex];
                                var connectedNode = staticState.GraphQuery.GetNode(connectedNodeId);
                                foreach (var sinkNodeIndex in sinkNodeIndices) {
                                    var sinkNode = staticState.SinkNodes[sinkNodeIndex];
                                    if (sinkNode == null) continue;

                                    if (nextState.Path.Count == 1 && sinkNode == staticState.HeadNode) {
                                        // If the path node size is 1, we don't want to connect back to the head node
                                        continue;
                                    }
                                    
                                    if (connectedNode == sinkNode) {
                                        var groupEdgeNode = staticState.GraphQuery.GetNode(groupEdgeNodeId);
                                        // TODO: Iterate through the edge nodes and check if we can connect to the tail node
                                        var incomingConstraintLinks = new List<FFAGConstraintsLink>(baseIncomingConstraintLinks);
                                        var connectedSubNode = connectedNode;
                                        if (connectedNode.MergedCompositeNodes.Count > 1) {
                                            foreach (var graphLink in staticState.Graph.Links) {
                                                if (graphLink.state.type != FlowLayoutGraphLinkType.Unconnected) continue;
                                                if (graphLink.source == groupEdgeNodeId && graphLink.destination == connectedNodeId) {
                                                    connectedSubNode = staticState.GraphQuery.GetSubNode(graphLink.destinationSubNode);
                                                    Debug.Assert(connectedSubNode != null);
                                                    break;
                                                }
                                                else if (graphLink.source == connectedNodeId && graphLink.destination == groupEdgeNodeId) {
                                                    connectedSubNode = staticState.GraphQuery.GetSubNode(graphLink.sourceSubNode);
                                                    Debug.Assert(connectedSubNode != null);
                                                    break;
                                                }
                                            }
                                        }
                                        incomingConstraintLinks.Add(new FFAGConstraintsLink(groupEdgeNode, connectedSubNode));
                                        if (!staticState.GraphConstraint.IsValid(
                                            staticState.GraphQuery, growthNodeGroup, pathIndex, pathLength, incomingConstraintLinks.ToArray()))
                                        {
                                            continue;
                                        }

                                        var sinkIncomingNodes = new List<FlowLayoutGraphNode>() { groupEdgeNode };
                                        if (sinkNode == staticState.HeadNode) {
                                            // The sink and the head are the same. Add the first node to the connected list
                                            var firstNodeInPath = staticState.GraphQuery.GetNode(nextState.Path[0].NodeId);
                                            if (firstNodeInPath != null) {
                                                sinkIncomingNodes.Add(firstNodeInPath);
                                            }
                                        }

                                        if (!staticState.GraphConstraint.IsValid(staticState.GraphQuery, sinkNode, sinkIncomingNodes.ToArray()))
                                            continue;

                                        nextState.TailNode = sinkNode;
                                        stackSystem.FinalizeResult(new FFlowLayoutPathingSystemResult(nextState, staticState));
                                        return;
                                    }
                                }
                            }
                        }
                    }

                    if (nextState.Path.Count == staticState.MaxPathSize) {
                        // no sink nodes nearby and we've reached the max path size
                        return;
                    }
                }


                // Try to grow from each outgoing node
                {
                    var groupEdgeNodeIndices = MathUtils.GetShuffledIndices(growthNodeGroup.GroupEdgeNodes.Count, staticState.Random);
                    foreach (var groupEdgeNodeIndex in groupEdgeNodeIndices) {
                        var groupEdgeNodeId = growthNodeGroup.GroupEdgeNodes[groupEdgeNodeIndex];
                        var connectedNodeIds = staticState.Graph.GetConnectedNodes(groupEdgeNodeId);
                        var connectedNodeIndices = MathUtils.GetShuffledIndices(connectedNodeIds.Length, staticState.Random);
                        foreach (var connectedNodeIndex in connectedNodeIndices) {
                            var connectedNodeId = connectedNodeIds[connectedNodeIndex];
                            if (nextState.Visited.Contains(connectedNodeId)) continue;

                            var connectedNode = staticState.GraphQuery.GetNode(connectedNodeId);
                            if (connectedNode == null) continue;
                            if (connectedNode.active) continue;
                            var groupEdgeNode = staticState.GraphQuery.GetNode(groupEdgeNodeId);

                            var incomingConstraintLinks = new List<FFAGConstraintsLink>(baseIncomingConstraintLinks);
                            incomingConstraintLinks.Add(new FFAGConstraintsLink(groupEdgeNode, connectedNode));
                            if (!staticState.GraphConstraint.IsValid(staticState.GraphQuery, growthNodeGroup, pathIndex, pathLength, incomingConstraintLinks.ToArray())) {
                                continue;
                            }

                            var nextFrame = new FlowLayoutPathStackFrame();
                            nextFrame.State = nextState;
                            nextFrame.CurrentNode = connectedNode;
                            nextFrame.IncomingNode = groupEdgeNode;
                            stackSystem.PushFrame(nextFrame);
                        }
                    }
                }
            }
        }
    }
}
