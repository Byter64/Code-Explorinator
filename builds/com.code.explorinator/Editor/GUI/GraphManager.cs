﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Random = System.Random;

namespace CodeExplorinator
{
    public class GraphManager
    {
        public struct SerializationData
        {
            public int shownClassDepth;
            public int shownMethodDepth;
            public State state;
            public string[] focusedClassNodes;
            public string[] focusedMethodNodes;

            public SerializationData(int shownClassDepth, int shownMethodDepth, State state, HashSet<ClassNode> focusedClassNodes, HashSet<MethodNode> focusedMethodNodes)
            {
                this.shownClassDepth = shownClassDepth;
                this.shownMethodDepth = shownMethodDepth;
                this.state = state;
                this.focusedClassNodes = ToStringArray(focusedClassNodes);
                this.focusedMethodNodes = ToStringArray(focusedMethodNodes);
            }

            private static string[] ToStringArray(HashSet<ClassNode> classNodes)
            {
                ClassNode[] classNodesArray = classNodes.ToArray();
                string[] result = new string[classNodes.Count];

                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = classNodesArray[i].ClassData.ClassInformation.ToDisplayString();
                }
                return result;
            }

            private static string[] ToStringArray(HashSet<MethodNode> methodNode)
            {
                MethodNode[] methodNodesArray = methodNode.ToArray();
                string[] result = new string[methodNode.Count];

                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = methodNodesArray[i].MethodData.MethodSymbol.ToDisplayString();
                }
                return result;
            }

            public static HashSet<ClassNode> ToClassNodes(string[] displayStrings)
            {
                HashSet<ClassNode> result = new();

                foreach (string instance in displayStrings)
                {
                    IEnumerable<ClassNode> nodes = Instance.ClassNodes.Where(x => x.ClassData.ClassInformation.ToDisplayString().Equals(instance));
                    if (nodes.Count() == 1)
                    {
                        result.Add(nodes.First());
                    }
                }

                return result;
            }

            public static HashSet<MethodNode> ToMethodNodes(string[] displayStrings)
            {
                HashSet<MethodNode> result = new();

                foreach (string instance in displayStrings)
                {
                    IEnumerable<MethodNode> nodes = Instance.methodNodes.Where(x => x.MethodData.MethodSymbol.ToDisplayString().Equals(instance));
                    if (nodes.Count() == 1)
                    {
                        result.Add(nodes.First());
                    }
                }

                return result;
            }
        }

        public enum State
        {
            ClassLayer,
            MethodLayer
        }

        public static GraphManager Instance { get; private set; }

        /// <summary>
        /// all ClassNodes in the currently analyzed project
        /// </summary>
        public HashSet<ClassNode> ClassNodes { get; private set; } = new();

        public HashSet<ClassNode> FocusedClassNodes { get; private set; } = new();

        public GraphVisualizer graphVisualizer;

        /// <summary>
        /// The maximum distance from a node to its closest focused node, so that it is still shown
        /// </summary>
        private int shownClassDepth;
        private int shownMethodDepth;
        private State state;
        private Random random = new Random(0987653);
        private VisualElement graphRoot;
        private ClassGraph methodGraph;
        private HashSet<ClassNode> selectedClassNodes = new();
        private HashSet<MethodNode> focusedMethodNodes = new();
        private HashSet<MethodNode> selectedMethodNodes = new();
        private List<ClassGraph> classGraphs = new();

        /// <summary>
        /// all methodNodes in the currently analyzed project
        /// </summary>
        private HashSet<MethodNode> methodNodes = new();

        /// <summary>
        /// all methodNodes within the maxDistance from focusedMethodNode
        /// </summary>
        private HashSet<MethodNode> shownMethodNodes = new();


        public GraphManager(List<ClassData> data, VisualElement graphRoot, int shownDepth)
        {
            Instance = this;
            shownClassDepth = shownDepth;
            shownMethodDepth = shownDepth;
            this.graphRoot = graphRoot;
            graphVisualizer = new(graphRoot);

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

        public void Reinitialize(List<ClassData> data)
        {
            ClassNodes.Clear();
            methodNodes.Clear();

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

            #region Replace Old Data With New Data
            //FocusedNodes
            HashSet<ClassNode> newFocusedNodes = new();
            foreach (ClassNode node in FocusedClassNodes)
            {
                IEnumerable<ClassNode> equivalentNodes = ClassNodes.Where(x => x.ClassData.GetName() == node.ClassData.GetName());
                if (equivalentNodes.Count() > 0)
                {
                    newFocusedNodes.Add(equivalentNodes.First());
                }
            }
            FocusedClassNodes = newFocusedNodes;

            //SelectedNodes
            HashSet<ClassNode> newSelectedClassNodes = new();
            foreach (ClassNode node in selectedClassNodes)
            {
                IEnumerable<ClassNode> equivalentNodes = ClassNodes.Where(x => x.ClassData.GetName() == node.ClassData.GetName());
                if (equivalentNodes.Count() > 0)
                {
                    newSelectedClassNodes.Add(equivalentNodes.First());
                }
            }
            selectedClassNodes = newSelectedClassNodes;

            //SelectedMethodNodes
            HashSet<MethodNode> newSelectedMethodNodes = new();
            foreach (MethodNode node in selectedMethodNodes)
            {
                IEnumerable<MethodNode> equivalentNodes = methodNodes.Where(x => x.MethodData.GetName() == node.MethodData.GetName());
                if (equivalentNodes.Count() > 0)
                {
                    newSelectedMethodNodes.Add(equivalentNodes.First());
                }
            }
            selectedMethodNodes = newSelectedMethodNodes;

            //FocusedMethodNodes
            HashSet<MethodNode> newFocusedMethodNodes = new();
            foreach (MethodNode node in focusedMethodNodes)
            {
                IEnumerable<MethodNode> equivalentNodes = methodNodes.Where(x => x.MethodData.GetName() == node.MethodData.GetName());
                if (equivalentNodes.Count() > 0)
                {
                    newFocusedMethodNodes.Add(equivalentNodes.First());
                }
            }
            focusedMethodNodes = newFocusedMethodNodes;
            #endregion

            //Recalculate method graph and class graphs
            MenuGUI.Instance?.UpdateFocusedEntries();
            UpdateSubGraphs(FocusedClassNodes, shownClassDepth);
            UpdateMethodGraph(focusedMethodNodes, shownMethodDepth);

            //Update graph Visualizer
            if (state == State.ClassLayer)
            {
                //Focus on previously focused nodes with preserving the selected nodes
                HashSet<ClassNode> selectedClassNodes = new();
                selectedClassNodes.UnionWith(this.selectedClassNodes);
                this.selectedClassNodes.Clear();
                this.selectedClassNodes.UnionWith(FocusedClassNodes);

                AddSelectedClasses(this.selectedClassNodes);
                state = State.MethodLayer; //so that highlights are set properly in ApplySelectedClasses
                ApplySelectedClasses();
                this.selectedClassNodes.UnionWith(selectedClassNodes);
            }
            else if (state == State.MethodLayer)
            {
                //Focus on previously focused nodes with preserving the selected nodes
                HashSet<MethodNode> selectedMethodNodes = new();
                selectedMethodNodes.UnionWith(this.selectedMethodNodes);
                this.selectedMethodNodes.Clear();
                this.selectedMethodNodes.UnionWith(focusedMethodNodes);

                AddSelectedMethods(this.selectedMethodNodes);
                state = State.ClassLayer; //so that highlights are set properly in ApplySelectedMethods
                ApplySelectedMethods();
                this.selectedMethodNodes.UnionWith(selectedMethodNodes);
            }
        }

        public void AddSelectedClass(ClassNode selectedClass)
        {
            selectedClassNodes.Add(selectedClass);
        }

        public void AddSelectedClasses(IEnumerable<ClassNode> selectedClasses)
        {
            foreach (ClassNode node in selectedClasses)
            {
                AddSelectedClass(node);
            }
        }

        public void AddSelectedMethod(MethodNode selectedMethod)
        {
            selectedMethodNodes.Add(selectedMethod);
        }

        public void AddSelectedMethods(IEnumerable<MethodNode> selectedMethods)
        {
            foreach (MethodNode node in selectedMethods)
            {
                AddSelectedMethod(node);
            }
        }

        public void ApplySelectedClasses()
        {
            FocusedClassNodes.Clear();
            FocusedClassNodes.UnionWith(selectedClassNodes);
            selectedClassNodes.Clear();

            if (state == State.MethodLayer)
            {
                ChangeToClassLayer();
                graphVisualizer.ShowMethodLayer(false);
            }


            MenuGUI.Instance?.UpdateFocusedEntries();

            UpdateSubGraphs(FocusedClassNodes, shownClassDepth);
            ShowClassLayer();
        }

        public void ApplySelectedMethods()
        {

            focusedMethodNodes.Clear();
            focusedMethodNodes.UnionWith(selectedMethodNodes);
            selectedMethodNodes.Clear();

            if (state == State.ClassLayer)
            {
                ChangeToMethodLayer();
                graphVisualizer.ShowClassLayer(false);
            }

            UpdateMethodGraph(focusedMethodNodes, shownMethodDepth);
            ShowMethodLayer();
        }

        /// <summary>
        /// Changes the maxDistance for classes and adapts the UI to it
        /// </summary>
        /// <param name="depth"></param>
        public void ChangeClassDepth(int depth)
        {
            shownClassDepth = depth;
            if (state != State.ClassLayer)
            {
                ChangeToClassLayer();
                graphVisualizer.ShowMethodLayer(false);
            }

            UpdateSubGraphs(FocusedClassNodes, depth);
            ShowClassLayer();
        }

        /// <summary>
        /// Changes the maxDistance for methods and adapts the UI to it
        /// </summary>
        /// <param name="depth"></param>
        public void ChangeMethodDepth(int depth)
        {
            shownMethodDepth = depth;
            if (state != State.MethodLayer) { return; }

            UpdateMethodGraph(focusedMethodNodes, shownMethodDepth);
            ShowMethodLayer();
        }

        public string Serialize(bool prettyPrint)
        {
            SerializationData data = new SerializationData(shownClassDepth, shownMethodDepth, state, FocusedClassNodes, focusedMethodNodes);
            string result;
            if (prettyPrint)
            {
                result = JsonConvert.SerializeObject(data, Formatting.Indented);
            }
            else
            {
                result = JsonConvert.SerializeObject(data, Formatting.None);
            }
            return result;
        }

        public SerializationData DeSerialize(string jsonString)
        {
            if (jsonString == null)
            {
                throw new ArgumentNullException();
            }
            SerializationData data = JsonConvert.DeserializeObject<SerializationData>(jsonString);

            return data;
        }


        public HashSet<ConnectionGUI> FindAllConnections(ClassGUI classGUI)
        {
            HashSet<ConnectionGUI> result = new HashSet<ConnectionGUI>();

            ClassGraph containingGraph = null;
            switch (state)
            {
                case State.ClassLayer:
                    foreach (ClassGraph graph in classGraphs)
                    {
                        if (graph.Contains(classGUI))
                        {
                            containingGraph = graph;
                            break;
                        }
                    }
                    break;
                case State.MethodLayer:
                    if (methodGraph.Contains(classGUI))
                    {
                        containingGraph = methodGraph;
                    }
                    break;
            }

            if (containingGraph == null) { return null; }

            foreach (ConnectionGUI connection in containingGraph.connections)
            {
                if (connection.TipNode == classGUI.VisualElement || connection.FootNode == classGUI.VisualElement)
                {
                    result.Add(connection);
                }
            }

            return result;
        }


        private void ChangeToMethodLayer()
        {
            state = State.MethodLayer;

            foreach (ClassNode node in FocusedClassNodes)
            {
                node.classGUI.SetFocused(false);
            }

            foreach (MethodNode node in focusedMethodNodes)
            {
                node.MethodData.ContainingClass.ClassNode.classGUI.SetFocused(true);
            }
        }

        private void ChangeToClassLayer()
        {
            state = State.ClassLayer;

            foreach (MethodNode node in focusedMethodNodes)
            {
                node.MethodData.ContainingClass.ClassNode.classGUI.SetFocused(false);
            }

            foreach (ClassNode node in FocusedClassNodes)
            {
                node.classGUI.SetFocused(true);
            }


            focusedMethodNodes.Clear();
        }

        private void ShowMethodLayer()
        {
            HashSet<ClassGUI> classGUIs = GetAllClassGUIs(new HashSet<ClassGraph>() { methodGraph });
            HashSet<ConnectionGUI> connectionGUIs = GetAllConnectionGUIs(new HashSet<ClassGraph>() { methodGraph });
            HashSet<MethodGUI> methodGUIs = GetAllMethodGUIs(shownMethodNodes);
            HashSet<MethodGUI> focusedMethods = GetAllMethodGUIs(focusedMethodNodes);
            HashSet<MethodGUI> unfocusedMethods = new();

            unfocusedMethods.UnionWith(methodGUIs);
            unfocusedMethods.ExceptWith(focusedMethods);

            graphVisualizer.SetMethodLayer(classGUIs, connectionGUIs, focusedMethods, unfocusedMethods);
            graphVisualizer.ShowMethodLayer(true, methodGUIs);
        }

        private void ShowClassLayer()
        {
            HashSet<ClassGUI> classGUIs = GetAllClassGUIs(classGraphs);
            HashSet<ConnectionGUI> connectionGUIs = GetAllConnectionGUIs(classGraphs);
            HashSet<ClassGUI> focusedGUIs = new();
            HashSet<ClassGUI> unfocusedGUIs = new();
            focusedGUIs.UnionWith(GetAllClassGUIs(FocusedClassNodes));
            unfocusedGUIs.UnionWith(classGUIs);
            unfocusedGUIs.ExceptWith(focusedGUIs);

            graphVisualizer.SetClassLayer(focusedGUIs, unfocusedGUIs, connectionGUIs);
            graphVisualizer.ShowClassLayer(true);
        }

        private void UpdateSubGraphs(HashSet<ClassNode> focusClasses, int shownDepth)
        {
            classGraphs.Clear();
            classGraphs = GenerateOptimalSubgraphs(ClassNodes, focusClasses, shownDepth);
            HashSet<ClassNode> nodes = new();
            foreach (ClassGraph graph in classGraphs)
            {
                nodes.UnionWith(graph.classNodes);
            }
            EditorUtility.DisplayProgressBar("Updating scene", "Finding suboptimal layout", 0.8f);
            SpringEmbedderAlgorithm.StartAlgorithm(nodes);

            foreach (ClassGraph graph in classGraphs)
            {
                graph.GenerateConnectionsBetweenClasses();
                graph.GenerateVisualElementGraph();
            }

            EditorUtility.ClearProgressBar();
        }

        private void UpdateMethodGraph(HashSet<MethodNode> focusMethods, int shownDepth)
        {
            EditorUtility.DisplayProgressBar("Updating scene", "Generating graph", 0);
            BreadthSearch.Reset();
            shownMethodNodes.Clear();
            foreach (MethodNode methodNode in methodNodes)
            {
                methodNode.distanceFromFocusMethod = int.MinValue;
            }
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

            EditorUtility.DisplayProgressBar("Updating scene", "Layouting graph", 0.5f);
            //Create classgraph that contains each class which contains a shown method
            methodGraph = new ClassGraph(this, FindClassNodes(shownMethodNodes), focusedClasses, shownDepth);

            //Calculate new positions
            SpringEmbedderAlgorithm.StartMethodAlgorithm(methodGraph.classNodes, shownMethodNodes);
            methodGraph.GenerateMethodConnectionsBetweenClasses();
            methodGraph.GenerateVisualElementGraph();

            EditorUtility.ClearProgressBar();
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
            EditorUtility.DisplayProgressBar("Updating scene", "Generating subgraphs", 0);
            //Generate a subgraph for every focus node
            List<(ClassNode, HashSet<ClassNode>)> overlappingSubGraphs = new();
            foreach (ClassNode focusNode in focusNodes)
            {
                BreadthSearch.Reset();
                HashSet<ClassNode> subgraph = BreadthSearch.GenerateClassSubgraph(superGraph, focusNode, maxDistance);
                overlappingSubGraphs.Add((focusNode, subgraph));
            }

            EditorUtility.DisplayProgressBar("Updating scene", "Merging subgraphs", .6f);
            //Combine overlapping subgraphs
            List<(HashSet<ClassNode>, HashSet<ClassNode>)> subgraphs = new();
            foreach ((ClassNode, HashSet<ClassNode>) overlappingSubgraph in overlappingSubGraphs)
            {
                if (subgraphs.Count == 0) //first element
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
                        if (distance <= (2 * maxDistance) + 1)
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
                    subgraphs.Add(newGraph);
                }
            }

            EditorUtility.DisplayProgressBar("Updating scene", "Saving result", .7f);
            //Create ClassGraph instances
            List<ClassGraph> graphs = new();
            foreach ((HashSet<ClassNode> focusNodes, HashSet<ClassNode> graph) subgraph in subgraphs)
            {
                graphs.Add(new ClassGraph(this, subgraph.graph, subgraph.focusNodes, maxDistance));
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
                font = AssetDatabase.LoadAssetAtPath<Font>(Utilities.pathroot + "Editor/Fonts/DroidSansMono.ttf"),
                fontSize = 20,
                richText = true
            };

            GUIStyle methodStyle = new GUIStyle(classStyle);
            methodStyle.alignment = TextAnchor.UpperLeft;



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
                font = AssetDatabase.LoadAssetAtPath<Font>(Utilities.pathroot + "Editor/Fonts/DroidSansMono.ttf"),
                fontSize = 20,
                richText = true
            };
            methodStyle.alignment = TextAnchor.UpperLeft;

            MethodNode node = new MethodNode(methodData, methodGUI);
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

        private HashSet<ClassGUI> GetAllClassGUIs(IEnumerable<ClassGraph> classGraphs)
        {
            HashSet<ClassGUI> classGUI = new HashSet<ClassGUI>();
            foreach (ClassGraph classGraph in classGraphs)
            {
                classGUI.UnionWith(GetAllClassGUIs(classGraph.classNodes));
            }
            return classGUI;
        }

        private HashSet<ClassGUI> GetAllClassGUIs(HashSet<ClassNode> classNodes)
        {
            HashSet<ClassGUI> classGUI = new HashSet<ClassGUI>();
            foreach (ClassNode classNode in classNodes)
            {
                classGUI.Add(classNode.classGUI);
            }
            return classGUI;
        }

        private HashSet<ConnectionGUI> GetAllConnectionGUIs(IEnumerable<ClassGraph> classGraphs)
        {
            HashSet<ConnectionGUI> connectionGUIs = new HashSet<ConnectionGUI>();
            foreach (ClassGraph classGraph in classGraphs)
            {
                foreach (ConnectionGUI connection in classGraph.connections)
                {
                    connectionGUIs.Add(connection);
                }
            }
            return connectionGUIs;
        }
        private HashSet<MethodGUI> GetAllMethodGUIs(IEnumerable<MethodNode> methodNodes)
        {
            HashSet<MethodGUI> methodGUIs = new HashSet<MethodGUI>();
            foreach (MethodNode node in methodNodes)
            {
                methodGUIs.Add(node.MethodGUI);
            }
            return methodGUIs;
        }
    }
}