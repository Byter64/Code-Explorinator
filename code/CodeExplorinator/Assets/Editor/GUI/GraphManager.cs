﻿using Codice.Client.BaseCommands.CheckIn;
using Codice.Client.Common.TreeGrouper;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Random = System.Random;

namespace CodeExplorinator
{
    public class GraphVisualizer
    {
        public enum State
        {
            ClassLayer,
            MethodLayer
        }

        /// <summary>
        /// all ClassNodes in the currently analyzed project
        /// </summary>
        public HashSet<ClassNode> ClassNodes { get; private set; }

        private State state;

        private HashSet<ClassNode> focusedClassNodes;

        private HashSet<ClassNode> selectedClassNodes;

        private HashSet<MethodNode> focusedMethodNodes;

        private HashSet<MethodNode> selectedMethodNodes;

        /// <summary>
        /// The currently focused methodNode in the oldFocusNode. This is null if the method layer is inactive
        /// </summary>
        public MethodNode focusedMethodNode;

        /// <summary>
        /// The maximum nodePair as edges from the focused nodes until which other nodes are still shown
        /// </summary>
        private int shownClassDepth;

        private int shownMethodDepth;

        /// <summary>
        /// The root visualElement of the editor window
        /// </summary>
        private VisualElement graphRoot;

        /// <summary>
        /// all classes that have a method from shownMethodNodes
        /// </summary>
        private ClassGraph methodGraph;

        /// <summary>
        /// all methodNodes in the currently analyzed project
        /// </summary>
        private HashSet<MethodNode> methodNodes;

        /// <summary>
        /// all methodNodes within the maxDistance from focusedMethodNode
        /// </summary>
        private HashSet<MethodNode> shownMethodNodes;

        private List<ClassGraph> classGraphs;
        
        /// <summary>
        /// randomize via a seed to spawn the classes. TODO: make actually random
        /// </summary>
        private Random random = new Random(0987653); 

        public GraphVisualizer(List<ClassData> data, VisualElement graphRoot, int shownDepth)
        {
            shownClassDepth = shownDepth;
            shownMethodDepth = shownDepth;
            this.graphRoot = graphRoot;

            ClassNodes = new HashSet<ClassNode>();
            methodNodes = new HashSet<MethodNode>();
            shownMethodNodes = new HashSet<MethodNode>();
            classGraphs = new List<ClassGraph>();
            focusedClassNodes = new HashSet<ClassNode>();
            selectedClassNodes = new HashSet<ClassNode>();
            focusedMethodNodes = new HashSet<MethodNode>();
            selectedMethodNodes = new HashSet<MethodNode>();
            //populate the lists with data
            foreach (ClassData @class in data)
            {
                ClassNodes.Add(GenerateNode(@class));
            }

            ClassNode.CopyRerefencesFromClassData(ClassNodes);
            foreach (ClassNode classNode in ClassNodes)
            {
                foreach (MethodGUI methodGUI in classNode.classGUI.methodGUIs)
                {
                    MethodNode node = GenerateNode(methodGUI.data, methodGUI);
                    node.MethodData.ContainingClass.ClassNode.MethodNodes.Add(node);
                    methodNodes.Add(node);
                }
            }


            MethodNode.CopyRerefencesFromMethodData(methodNodes);
            state = State.ClassLayer;
        }

        public void AddSelectedClass(ClassNode selectedClass)
        {
            selectedClassNodes.Add(selectedClass);
        }

        public void FocusOnSelectedClasses()
        {
            focusedClassNodes.Clear();
            focusedClassNodes.UnionWith(selectedClassNodes);
            ChangeClassGraphs(selectedClassNodes, shownClassDepth);
            selectedClassNodes.Clear();
        }

        public void AddSelectedMethod(MethodNode selectedMethod)
        {
            selectedMethodNodes.Add(selectedMethod);
        }

        public void FocusOnSelectedMethods()
        {
            focusedMethodNodes.Clear();
            focusedMethodNodes.UnionWith(selectedMethodNodes);
            ChangeMethodGraph(selectedMethodNodes, shownMethodDepth);
            selectedMethodNodes.Clear();
        }

        /// <summary>
        /// Changes the maxDistance for claesses and adapts the UI to it
        /// </summary>
        /// <param name="depth"></param>
        public void ChangeClassDepth(int depth)
        {
            shownClassDepth = depth;
            if(state != State.ClassLayer) { return; }

            ChangeClassGraphs(focusedClassNodes, depth);
        }

        /// <summary>
        /// Changes the maxDistance for methods and adapts the UI to it
        /// </summary>
        /// <param name="depth"></param>
        public void ChangeMethodDepth(int depth)
        {
            shownMethodDepth = depth;
            if (state != State.MethodLayer) { return; }

            ChangeMethodGraph(focusedMethodNodes, depth);
        }

        public void ChangeToMethodLayer()
        {
            HideClassGraphs();

            state = State.MethodLayer;
        }

        public void ChangeToClassLayer()
        {
            ShowClassGraphs();
            if(methodGraph != null && methodGraph.Root.parent == graphRoot)
            {
                graphRoot.Remove(methodGraph.Root);
            }
            state = State.ClassLayer;
        }

        private void ChangeClassGraphs(HashSet<ClassNode> focusClasses, int shownDepth)
        {
            RemoveClassGraphGUIs();
            UpdateSubGraphs(focusClasses, shownDepth);
            AddGraphGUI();
        }
        private void RemoveClassGraphGUIs()
        {
            foreach (ClassGraph graph in classGraphs)
            {
                if (graphRoot == graph.Root.parent)
                {
                    graphRoot.Remove(graph.Root);
                }
            }
        }

        private void UpdateSubGraphs(HashSet<ClassNode> focusClasses, int shownDepth)
        {
            classGraphs.Clear();
            classGraphs = GenerateOptimalSubgraphs(ClassNodes, focusClasses, shownDepth);
            HashSet<ClassNode> nodes = new();
            foreach(ClassGraph graph in classGraphs)
            {
                nodes.UnionWith(graph.classNodes);
            }

            SpringEmbedderAlgorithm.StartAlgorithm(nodes.ToList());

            foreach (ClassGraph graph in classGraphs)
            {
                nodes.UnionWith(graph.classNodes);

                graph.GenerateConnectionsBetweenClasses();
                graph.GenerateVisualElementGraph();
            }
        }

        private void AddGraphGUI()
        {
            foreach (ClassGraph graph in classGraphs)
            {
                graphRoot.Add(graph.Root);
            }
        }

        private void ChangeMethodGraph(HashSet<MethodNode> focusMethods, int shownDepth)
        {
            RemoveMethodGraphGUI();
            UpdateMethodGraph(focusMethods, shownDepth);
            graphRoot.Add(methodGraph.Root);
        }

        private void RemoveMethodGraphGUI()
        {
            if (methodGraph == null || methodGraph.Root == null) { return; }
            if (graphRoot.Contains(methodGraph.Root))
            {
                graphRoot.Remove(methodGraph.Root);
            }
        }

        private void HideClassGraphs()
        {
            foreach (ClassGraph graph in classGraphs)
            {
                graph.Root.visible = false;
            }
        }

        public void ShowClassGraphs()
        {
            foreach (ClassGraph graph in classGraphs)
            {
                graph.Root.visible = true;
            }
        }

        private void UpdateMethodGraph(HashSet<MethodNode> focusMethods, int shownDepth)
        {
            foreach (MethodNode node in shownMethodNodes)
            {
                node.MethodGUI.ShowBackground(false);
            }
            BreadthSearch.Reset();
            shownMethodNodes.Clear();
            //Generate graph
            foreach (MethodNode node in focusMethods)
            {
                shownMethodNodes.UnionWith(BreadthSearch.GenerateMethodSubgraph(node, shownDepth));
            }
            //Find the containing class for each focused class
            HashSet<ClassNode> focusedClasses = new();
            foreach (MethodNode node in focusMethods)
            {
                focusedClasses.Add(node.MethodData.ContainingClass.ClassNode);
            }
            foreach (MethodNode node in shownMethodNodes)
            {
                node.MethodGUI.ShowBackground(true);
            }
            foreach(MethodNode node in shownMethodNodes)
            {
                node.MethodData.ContainingClass.ClassNode.classGUI.VisualElement.visible = true;
            }
            //Create classgraph that contains each class which contains a shown method
            methodGraph = new ClassGraph(FindClassNodes(shownMethodNodes), focusedClasses, shownDepth);

            //Backup positions of currently shown classes
            foreach (ClassGraph graph in classGraphs)
            {
                foreach (ClassNode node in graph.classNodes)
                {
                    node.classGUI.MakePositionBackup();
                }
            }
            //Calculate new positions
            SpringEmbedderAlgorithm.StartMethodAlgorithm(methodGraph.classNodes, shownMethodNodes);
            methodGraph.GenerateConnectionsBetweenMethods(shownMethodNodes);
            methodGraph.GenerateVisualElementGraph();
        }

        /// <summary>
        /// Generates subgraph so that the following holds true: 
        /// 1. Every subgraph contains at least one focused node.
        /// 2. The distance of any node to its closest focused node is smaller than maxDistance
        /// 3. Every node is in exactly one subgraph
        /// 4. No subgraph has an outer node, which has an edge that leads to a node which is in another subgraph
        /// </summary>
        /// <param name="superGraph"></param>
        /// <param name="focusNodes"></param>
        /// <param name="maxDistance"></param>
        /// <returns></returns>
        private List<ClassGraph> GenerateOptimalSubgraphs(HashSet<ClassNode> superGraph, HashSet<ClassNode> focusNodes, int maxDistance)
        {
            //Generate a subgraph for every focus node
            List<(ClassNode, HashSet<ClassNode>)> overlappingSubGraphs = new();
            foreach(ClassNode focusNode in focusNodes)
            {
                BreadthSearch.Reset();
                HashSet<ClassNode> subgraph = BreadthSearch.GenerateClassSubgraph(superGraph, focusNode, maxDistance);
                overlappingSubGraphs.Add((focusNode, subgraph));
            }

            //Combine overlapping subgraphs
            List<(HashSet<ClassNode>, HashSet<ClassNode>)> subgraphs = new();
            foreach((ClassNode, HashSet<ClassNode>) overlappingSubgraph in overlappingSubGraphs)
            {
                if(subgraphs.Count == 0) //first element
                {
                    var newGraph = (new HashSet<ClassNode> { overlappingSubgraph.Item1 }, overlappingSubgraph.Item2);
                    subgraphs.Add(newGraph);
                    continue;
                }

                //Look for a subgraph with which the overlappingSubgraph is overlapping
                (HashSet<ClassNode> focusNodes, HashSet<ClassNode> graph) overlappedSubGraph = (null, null);
                foreach ((HashSet<ClassNode> focusNodes, HashSet<ClassNode> graph) subgraph in subgraphs)
                {
                    foreach (ClassNode focusNode in subgraph.focusNodes)
                    {
                        int distance = BreadthSearch.CalculateDistance(superGraph, focusNode, overlappingSubgraph.Item1);
                        //If two focus nodes are closer equal 2 * maxDistance, their graphs need to overlap
                        //If two focus nodes distance is equal to 2 * maxDistance + 1, their graphs are adjacent. In this case we also want to merge them
                        if (distance <= 2 * maxDistance + 1) 
                        {
                            overlappedSubGraph = subgraph;
                            goto foundOverlap;
                        }
                    }
                }

                foundOverlap:
                if (overlappedSubGraph != (null, null))
                {
                    overlappedSubGraph.graph.UnionWith(overlappingSubgraph.Item2);
                    overlappedSubGraph.focusNodes.Add(overlappingSubgraph.Item1);
                }
                else
                {
                    var newGraph = (new HashSet<ClassNode> { overlappingSubgraph.Item1 }, overlappingSubgraph.Item2);
                    subgraphs.Add(newGraph); //Darf nicht im foreach über subgraphs passieren!!!
                }
            }

            //Create ClassGraph instances
            List<ClassGraph> graphs = new();
            foreach((HashSet<ClassNode> focusNodes, HashSet<ClassNode> graph) subgraph in subgraphs)
            {
                graphs.Add(new ClassGraph(subgraph.graph, subgraph.focusNodes, maxDistance));
            }

            return graphs;
        }

        /// <summary>
        /// Generates a node out of a given classdata
        /// </summary>
        /// <param name="classData"></param>
        /// <returns></returns>
        private ClassNode GenerateNode(ClassData classData)
        {
            GUIStyle classStyle = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Editor/Fonts/DroidSansMono.ttf"),
                fontSize = 20,
                richText = true
            };

            GUIStyle methodStyle = new GUIStyle(classStyle);
            methodStyle.alignment = TextAnchor.UpperLeft;

            
            
            //The ClassGUI should not be generated in here but should rather be given to this method as a parameter!!!!
            ClassGUI classGUI = new ClassGUI(
                new Vector2(random.Next(-500, 500) - graphRoot.style.marginLeft.value.value,
                    random.Next(-500, 500) - graphRoot.style.marginTop.value.value), classData, classStyle,
                methodStyle, methodStyle, this);
            ClassNode node = new ClassNode(classData, classGUI);
            classData.ClassNode = node;
            return node;
        }

        /// <summary>
        /// Generates a methodNode out of a given methodData
        /// </summary>
        /// <param name="methodData"></param>
        /// <returns></returns>
        private MethodNode GenerateNode(MethodData methodData, MethodGUI methodGUI)
        {
            GUIStyle methodStyle = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Editor/Fonts/DroidSansMono.ttf"),
                fontSize = 20,
                richText = true
            };
            methodStyle.alignment = TextAnchor.UpperLeft;

            MethodNode node = new MethodNode(methodData, methodGUI);
            //Debug.Log("Visualelement: " + node.methodGUI.style.marginLeft + "/" + node.methodGUI.style.marginTop);
            methodData.MethodNode = node;

            return node;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="methodNodes"></param>
        /// <returns>All ClassNodes that are parents of at least one of the given MethodNodes</returns>
        private HashSet<ClassNode> FindClassNodes(HashSet<MethodNode> methodNodes)
        {
            HashSet<ClassNode> classNodes = new HashSet<ClassNode>();

            foreach (MethodNode methodNode in methodNodes)
            {
                classNodes.Add(methodNode.MethodData.ContainingClass.ClassNode);
            }

            return classNodes;
        }
    }
}