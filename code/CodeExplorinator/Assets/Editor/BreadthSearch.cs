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
    public class BreadthSearch
    {
        /// <summary>
        /// Nachricht von Yannik: Da AnalyzedClasses nirgends gelesen wird. Hab ich die nicht genommen, sondern AnalyzedNodes. Die Funktionalitšt
        /// von AnalyzedClasses hab ich sicherheitshalber trotzdem aufrecht erhalten
        /// </summary>
        public List<ClassData> AnalysedClasses;
        public List<MethodData> AnalysedMethods;
        public List<ClassNode> AnalysedNodes;

        public BreadthSearch()
        {
            AnalysedNodes = new List<ClassNode>();
            AnalysedClasses = new List<ClassData>();
            AnalysedMethods = new List<MethodData>();
        }

        private ClassData GetStartingClass(List<ClassData> classDatas)
        {
            //do something accually reasonable here
            return classDatas[0];
        }

        private MethodData GetStartingMethod(List<MethodData> methodDatas)
        {
            //do something accually reasonable here
            return methodDatas[0];
        }

        public void Reset()
        {
            AnalysedNodes = new List<ClassNode>();
            AnalysedClasses = new List<ClassData>();
            AnalysedMethods = new List<MethodData>();
        }

        /// <summary>
        /// Generates a subgraph from the full graph centered around the starting node with the given depth. It is not a real subgraph though because
        /// the "leaf nodes" still contain edges that lead out of the subgraph. These edges are part of the original graph.
        /// </summary>
        /// <param name="startingNode">the node from which the breadth search will start</param>
        /// <param name="depth">How for away from the starting node should be gone away. If depth is 0 then only the starting node will be in the returned list. </param>
        /// <returns></returns>
        public HashSet<ClassNode> GenerateSubgraph(IEnumerable<ClassNode> graph, ClassNode startingNode, int depth)
        {
            HashSet<ClassNode> subgraph = new HashSet<ClassNode>();
            GenerateClassGraph(startingNode, depth, subgraph);
            return subgraph;
        }

        private void GenerateClassGraph(ClassNode startingNode, int searchRadius, HashSet<ClassNode> result)
        {
            if (searchRadius < 0)
            {
                return;
            }

            /*
            //hopefully this doesnt need to be synchronized?
            if (AnalysedClasses.Contains(startingClass))
            {
                return;
            }
            */

            //if the class was already analysed but we can still search, the node is not generated but the tree explored further
            //impacts the searchtime negatively tho
            //if we wanted to perfectly run through all nodes we would have to save the highest searchradius that was gone trough, and go through
            //the node again if our current searchradius is higher. i think that could cause performance issuses if a lot of circular references are present
            foreach (var node in AnalysedNodes)
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

            bool isLeafNode = searchRadius == 0;
            startingNode.IsLeaf = isLeafNode;
            AnalysedClasses.Add(startingNode.ClassData);
            AnalysedNodes.Add(startingNode);
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

            foreach (var connectedClass in startingNode.ConnectedNodes)
            {
                GenerateClassGraph(connectedClass, searchRadius - 1, result);
            }


        }

        public void GenerateMethodGraph(MethodData startingMethod, int searchRadius)
        {
            if (searchRadius < 0)
            {
                return;
            }

            //hopefully this doesnt need to be synchronized?
            if (AnalysedMethods.Contains(startingMethod))
            {
                return;
            }

            AnalysedMethods.Add(startingMethod);

            //checking incoming and outgoing references

            foreach (var methodInvocation in startingMethod.InvokedByExternal)
            {
                //instantiate reference to method and maybe even the containing class
                GenerateMethodGraph(methodInvocation.ContainingMethod, searchRadius - 1);
            }

            foreach (var methodInvocation in startingMethod.IsInvokingExternalMethods)
            {
                //instantiate reference to method and maybe even the containing class
                GenerateMethodGraph(methodInvocation.ReferencedMethod, searchRadius - 1);
            }

            foreach (var fieldAccess in startingMethod.IsAccessingExternalField)
            {
                // just instantiate the class if needed, this is a dead end and not currently shown
            }

            foreach (var propertyAccess in startingMethod.IsAccessingExternalProperty)
            {
                // just instantiate the class if needed, this is a dead end and not currently shown
            }
        }
    }
}