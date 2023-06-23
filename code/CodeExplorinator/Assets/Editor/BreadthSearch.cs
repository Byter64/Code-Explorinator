using Microsoft.CodeAnalysis;
using UnityEngine;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Graphs;
using Codice.Client.BaseCommands.CheckIn;

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
            //This is not very nicely implemented
            HashSet<ClassNode> oldRound = start.ingoingConnections.Concat(start.outgoingConnections).ToHashSet();
            HashSet<ClassNode> newRound = new HashSet<ClassNode>();
            while(depth < graph.Count()) // :/
            {
                depth++;

                foreach(ClassNode node in oldRound)
                {
                    if(node == end)
                    {
                        goto CalculateDistanceEnd;
                    }

                    //Only add nodes to the next iteration which are not in the current one. 
                    //This doesn't guarantee that one node is only stepped through once but it should still catch a notable amount of these cases
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
            //impacts the searchtime negatively tho
            //if we wanted to perfectly run through all ClassNodes we would have to save the highest searchradius that was gone trough, and go through
            //the node again if our current searchradius is higher. i think that could cause performance issuses if a lot of circular references are present
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

            //checking incoming and outgoing references

            /*
            
            foreach (var fieldReference in startingClass.IsReferencingExternalClassAsField)
            {
                //instantiate fieldReference.FieldContainingReference.ContainingClass
                GenerateClassGraph(fieldReference.ReferencedClass, searchRadius - 1);
            }

            foreach (var propertyReference in startingClass.IsReferencingExternalClassAsProperty)
            {
                //instantiate propertyReference.PropertyContainingReference.ContainingClass
                GenerateClassGraph(propertyReference.ReferencedClass, searchRadius - 1);
            }

            foreach (var fieldReference in startingClass.ReferencedByExternalClassField)
            {
                //instantiate fieldReference.FieldContainingReference.ContainingClass
                GenerateClassGraph(fieldReference.FieldContainingReference.ContainingClass, searchRadius - 1);
            }

            foreach (var propertyReference in startingClass.ReferencedByExternalClassProperty)
            {
                //instantiate propertyReference.PropertyContainingReference.ContainingClass
                GenerateClassGraph(propertyReference.PropertyContainingReference.ContainingClass, searchRadius - 1);
            }
            */

            //or just iterate though AllContainingClasses:

            foreach (var connectedClass in startingNode.ClassData.AllConnectedClasses)
            {
                GenerateClassGraph(connectedClass.ClassNode, searchRadius - 1, result);
            }
            
            /*
            foreach (var connectedClass in startingNode.ConnectedNodes) //connected ClassNodes could be made to a hashset
            {
                GenerateClassGraph(connectedClass, searchRadius - 1, result);
            }
            */


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
            //impacts the searchtime negatively tho
            //if we wanted to perfectly run through all ClassNodes we would have to save the highest searchradius that was gone trough, and go through
            //the node again if our current searchradius is higher. i think that could cause performance issuses if a lot of circular references are present
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
            

            /*
            //checking incoming and outgoing references

            foreach (var methodInvocation in startingMethod.MethodData.InvokedByExternal)
            {
                //instantiate reference to method and maybe even the containing class
                GenerateMethodGraph(methodInvocation.ContainingMethod, searchRadius - 1, result);
            }

            foreach (var methodInvocation in startingMethod.MethodData.IsInvokingExternalMethods)
            {
                //instantiate reference to method and maybe even the containing class
                GenerateMethodGraph(methodInvocation.ReferencedMethod, searchRadius - 1, result);
            }

            foreach (var fieldAccess in startingMethod.MethodData.IsAccessingExternalField)
            {
                // just instantiate the class if needed, this is a dead end and not currently shown
            }

            foreach (var propertyAccess in startingMethod.MethodData.IsAccessingExternalProperty)
            {
                // just instantiate the class if needed, this is a dead end and not currently shown
            }
            */

            foreach (var connectedMethod in startingMethod.MethodData.AllConnectedMethods)
            {
                GenerateMethodGraph(connectedMethod.MethodNode,searchRadius - 1, result);
            }
            
        }

        
        /// <summary>
        /// This method inverts the methodNodes.distanceFromFocusMethod so that 0 is the focusmethod, 1 are the methods direclty connected to it, the numbers increasing with distance
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