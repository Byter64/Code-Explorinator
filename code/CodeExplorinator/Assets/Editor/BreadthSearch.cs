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
        /// Generates a subgraph from the full graph centered around the starting node with the given depth. It is not a real subgraph though because
        /// the "leaf nodes" still contain edges that lead out of the subgraph. These edges are part of the original graph.
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
            //if we wanted to perfectly run through all nodes we would have to save the highest searchradius that was gone trough, and go through
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
            foreach (var connectedClass in startingNode.ConnectedNodes) //connected nodes could be made to a hashset
            {
                GenerateClassGraph(connectedClass, searchRadius - 1, result);
            }
            */


        }
        
        /// <summary>
        /// Generates a subgraph from the full graph centered around the starting node with the given depth. It is not a real subgraph though because
        /// the "leaf nodes" still contain edges that lead out of the subgraph. These edges are part of the original graph.
        /// </summary>
        /// <param name="startingNode">the node from which the breadth search will start</param>
        /// <param name="depth">How for away from the starting node should be gone away. If depth is 0 then only the starting node will be in the returned list. </param>
        /// <returns></returns>
        public static HashSet<MethodNode> GenerateMethodSubgraph(MethodNode startingNode, int depth)
        {
            HashSet<MethodNode> subgraph = new HashSet<MethodNode>();
            GenerateMethodGraph(startingNode, depth, subgraph);
            return subgraph;
        }

        private static void GenerateMethodGraph(MethodNode startingMethod, int searchRadius, HashSet<MethodNode> result)
        {
            if (searchRadius < 0)
            {
                return;
            }
            
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
    }
}