using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace CodeExplorinator
{
    public static class BreadthSearch
    {
  
        public static List<MethodNode> AnalysedMethods;
        public static List<ClassNode> AnalysedClasses;

        static BreadthSearch()
        {
            AnalysedClasses = new List<ClassNode>();
            AnalysedMethods = new List<MethodNode>();
        }
        
        public static void Reset()
        {
            AnalysedClasses = new List<ClassNode>();
            AnalysedMethods = new List<MethodNode>();
        }

        /// <summary>
        /// Calculates the distance in a graph between two nodes
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns>the minimum amount of edges that need to be travelled to go from start to end node</returns>
        public static int CalculateDistance(IEnumerable<ClassNode> graph, ClassNode start, ClassNode end)
        {
            if(start == end) { return 0; }

            int depth = 0;
            
            HashSet<ClassNode> oldRound = start.ingoingConnections.Concat(start.outgoingConnections).ToHashSet();
            HashSet<ClassNode> newRound = new HashSet<ClassNode>();
            while(depth < graph.Count())
            {
                depth++;

                foreach(ClassNode node in oldRound)
                {
                    if(node == end)
                    {
                        goto CalculateDistanceEnd;
                    }

                    //Only add nodes to the next iteration which are not in the current one. 
                    newRound.UnionWith(node.ingoingConnections.Where(x => !oldRound.Contains(x)));
                    newRound.UnionWith(node.outgoingConnections.Where(x => !oldRound.Contains(x)));
                }

                oldRound = newRound;
                newRound = new HashSet<ClassNode>();
            }

            CalculateDistanceEnd:
            return depth;
        }

        /// <summary>
        /// Generates a subgraph from the full graph centered around the starting node with the given depth. It is not a real subgraph though because
        /// the "leaf ClassNodes" still contain edges that lead out of the subgraph. These edges are part of the original graph.
        /// </summary>
        /// <param name="startingNode">the node from which the breadth search will start</param>
        /// <param name="depth">How for away from the starting node should be gone away. If depth is 0 then only the starting node will be in the returned list. </param>
        /// <returns></returns>
        public static HashSet<ClassNode> GenerateClassSubgraph(IEnumerable<ClassNode> graph, ClassNode startingNode, int depth) //does graph even do anything??
        {
            HashSet<ClassNode> subgraph = new HashSet<ClassNode>();
            GenerateClassGraph(startingNode, depth, subgraph);
            return subgraph;
        }

        private static void GenerateClassGraph(ClassNode startingNode, int searchRadius, HashSet<ClassNode> result)
        {
            if (searchRadius < 0)
            {
                return;
            }
            
            //if the class was already analysed but we can still search, the node is not generated but the tree explored further
            foreach (var node in AnalysedClasses)
            {
                if (node == startingNode)
                {
                    if (node.IsLeaf && searchRadius > 0)
                    {
                        node.IsLeaf = false;
                        goto SkipOverGeneration;
                    }

                    return;
                }
            }
            
            startingNode.IsLeaf = searchRadius == 0;
            AnalysedClasses.Add(startingNode);
            result.Add(startingNode);

        SkipOverGeneration:

            //iterate though AllContainingClasses
            foreach (var connectedClass in startingNode.ClassData.AllConnectedClasses)
            {
                GenerateClassGraph(connectedClass.ClassNode, searchRadius - 1, result);
            }
        }
        
        /// <summary>
        /// Generates a subgraph from the full graph centered around the starting node with the given depth. It is not a real subgraph though because
        /// the "leaf ClassNodes" still contain edges that lead out of the subgraph. These edges are part of the original graph.
        /// </summary>
        /// <param name="startingNode">the node from which the breadth search will start</param>
        /// <param name="depth">How for away from the starting node should be gone away. If depth is 0 then only the starting node will be in the returned list. </param>
        /// <returns></returns>
        public static HashSet<MethodNode> GenerateMethodSubgraph(MethodNode startingNode, int depth)
        {
            HashSet<MethodNode> subgraph = new HashSet<MethodNode>();
            GenerateMethodGraph(startingNode, depth, subgraph);
            InvertDistanceNumbers(subgraph, depth);
            return subgraph;
        }

        private static void GenerateMethodGraph(MethodNode startingMethod, int searchRadius, HashSet<MethodNode> result)
        {
            if (searchRadius < 0)
            {
                return;
            }
            
            //if the class was already analysed but we can still search, the node is not generated but the tree explored further
            foreach (var node in AnalysedMethods)
            {
                if (node == startingMethod)
                {
                    if (node.IsLeaf && searchRadius > 0)
                    {
                        node.IsLeaf = false;
                        goto SkipOverGeneration;
                    }

                    return;
                }
            }
            
            startingMethod.IsLeaf = searchRadius == 0;
            AnalysedMethods.Add(startingMethod);
            result.Add(startingMethod);
            
            //set the distance to be able to show the distance visually (done in other classes)
            startingMethod.distanceFromFocusMethod = searchRadius;
            

            SkipOverGeneration:

            foreach (var connectedMethod in startingMethod.MethodData.AllConnectedMethods)
            {
                GenerateMethodGraph(connectedMethod.MethodNode,searchRadius - 1, result);
            }
            
        }

        
        /// <summary>
        /// Inverts the methodNodes.distanceFromFocusMethod so that 0 is the focusmethod, 1 are the methods direclty connected to it, the numbers increasing with distance
        /// </summary>
        private static void InvertDistanceNumbers(HashSet<MethodNode> allFoundMethodNodes, int searchRadius)
        {
            foreach (var node in allFoundMethodNodes)
            {
                node.distanceFromFocusMethod = searchRadius - node.distanceFromFocusMethod;
            }
        }
    }
}