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
    public class GraphManager
    {
        /// <summary>
        /// all ClassNodes in the currently analyzed project
        /// </summary>
        public HashSet<ClassNode> ClassNodes { get; private set; }

        /// <summary>
        /// the currently focused on focusNode in the oldFocusNode
        /// </summary>
        public ClassNode focusedClassNode;

        private HashSet<ClassNode> focusedClassNodes;

        private HashSet<ClassNode> selectedClassNodes;

        /// <summary>
        /// The currently focused methodNode in the oldFocusNode. This is null if the method layer is inactive
        /// </summary>
        public MethodNode focusedMethodNode;

        /// <summary>
        /// The maximum nodePair as edges from the focused nodes until which other nodes are still shown
        /// </summary>
        private int shownDepth;

        /// <summary>
        /// The root visualElement of the editor window
        /// </summary>
        private VisualElement graphRoot;

        /// <summary>
        /// all methodNodes in the currently analyzed project
        /// </summary>
        private HashSet<MethodNode> methodNodes;

        /// <summary>
        /// all methodNodes within the maxDistance from focusedMethodNode
        /// </summary>
        private HashSet<MethodNode> shownMethodNodes;

        /// <summary>
        /// all ClassNodes within the maxDistance from focusedMethodNode
        /// </summary>
        private HashSet<ClassNode> shownClassNodes;

        /// <summary>
        /// A list will all currently displayed connections between nodes
        /// </summary>
        private List<ConnectionGUI> shownConnections;

        private List<ClassGraph> classGraphs;
        
        /// <summary>
        /// random with a seed to spawn the classes random, but always on the same spot (i chose the seed number randomly)
        /// </summary>
        private Random random = new Random(0987653); 

        public GraphManager(List<ClassData> data, VisualElement graphRoot, int shownDepth)
        {
            this.shownDepth = shownDepth;
            this.graphRoot = graphRoot;

            ClassNodes = new HashSet<ClassNode>();
            methodNodes = new HashSet<MethodNode>();
            shownMethodNodes = new HashSet<MethodNode>();
            classGraphs = new List<ClassGraph>();
            focusedClassNodes = new HashSet<ClassNode>();
            selectedClassNodes = new HashSet<ClassNode>();
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
            shownConnections = new List<ConnectionGUI>();

            //Assign first focused class
            AddSelectedClass(ClassNodes.Where(x => x.ClassData.GetName().ToLower().Contains("classdata")).First());
            FocusOnSelectedClasses();
        }

        #region NEWSHIT
        public void AddSelectedClass(ClassNode selectedClass)
        {
            selectedClassNodes.Add(selectedClass);
        }

        public void FocusOnSelectedClasses()
        {
            focusedClassNodes.Clear();
            focusedClassNodes.UnionWith(selectedClassNodes);
            ChangeGraph(selectedClassNodes, shownDepth);
            selectedClassNodes.Clear();
        }

        /// <summary>
        /// Changes the maxDistance and adapts the UI to it
        /// </summary>
        /// <param name="depth"></param>
        public void ChangeDepth(int depth)
        {
            shownDepth = depth;
            ChangeGraph(focusedClassNodes, depth);
        }

        private void ChangeGraph(HashSet<ClassNode> focusClasses, int shownDepth)
        {
            RemoveGraphGUI();
            UpdateSubGraphs(focusClasses, shownDepth);
            AddGraphGUI();
        }

        private void RemoveGraphGUI()
        {
            foreach (ClassGraph graph in classGraphs)
            {
                if (graphRoot == graph.root.parent)
                {
                    graphRoot.Remove(graph.root);
                }
            }
        }

        private void UpdateSubGraphs(HashSet<ClassNode> focusClasses, int shownDepth)
        {
            classGraphs.Clear();
            classGraphs = GenerateOptimalSubgraphs(ClassNodes, focusClasses, shownDepth);
        }

        private void AddGraphGUI()
        {
            foreach (ClassGraph graph in classGraphs)
            {
                graphRoot.Add(graph.root);
            }
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

                //Look for a subgraph with which the overlappingSubgraph could be overlapping
                (HashSet<ClassNode> focusNodes, HashSet<ClassNode> graph) overlappedSubGraph = (null, null);
                foreach ((HashSet<ClassNode> focusNodes, HashSet<ClassNode> graph) subgraph in subgraphs)
                {
                    foreach (ClassNode focusNode in subgraph.focusNodes)
                    {
                        int distance = BreadthSearch.CalculateDistance(superGraph, focusNode, overlappingSubgraph.Item1);
                        //If two focus nodes are closer equal 2 * maxDistance, their graphs need to overlap
                        //If two focus nodes distance is equal to 2 * maxDistance + 1, their graphs are adjacent. In this case we also want to merge them
                        if (distance >= 2 * maxDistance + 1) 
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
        #endregion

        /// <summary>
        /// Changes the focus method and adapths the UI to it
        /// </summary>
        /// <param name="methodData"></param>
        public void UpdateFocusMethod(MethodData methodData)
        {
            focusedMethodNode = methodData.MethodNode;
            RedrawMethodGraph();
            focusedMethodNode.MethodData.ContainingClass.ClassNode.classGUI.VisualElement.BringToFront();
        }

        /// <summary>
        /// Draws the method layer
        /// </summary>
        private void RedrawMethodGraph()
        {
            RedrawMethodNodes();
            RedrawConnectionsForMethods();
        }


        private void RedrawMethodNodes()
        {
            foreach (MethodNode node in shownMethodNodes)
            {
                node.MethodGUI.ShowBackground(false);
            }

            BreadthSearch.Reset();

            shownMethodNodes = BreadthSearch.GenerateMethodSubgraph(focusedMethodNode, shownDepth);
            foreach (MethodNode node in shownMethodNodes)
            {
                node.MethodGUI.ShowBackground(true);
            }

            shownClassNodes = FindClassNodes(shownMethodNodes);

            SpringEmbedderAlgorithm.StartMethodAlgorithm(shownClassNodes, shownMethodNodes);
            AppendShownNodesToGraphRoot();
        }


        /// <summary>
        /// redraws the connections between methods and removes all other lines
        /// </summary>
        private void RedrawConnectionsForMethods()
        {
            RemoveConnectionsFromGraphRoot();
            shownConnections.Clear();

            foreach (MethodNode foot in shownMethodNodes)
            {
                if (foot.IsLeaf)
                {
                    foreach (MethodNode tip in foot.outgoingConnections)
                    {
                        if (foot == tip)
                        {
                            continue;
                        }

                        VisualElement shownTip = shownMethodNodes.Contains(tip) ? tip.MethodGUI.VisualElement : null;
                        ConnectionGUI connection = new ConnectionGUI(foot.MethodGUI.VisualElement,
                            shownTip);
                        connection.GenerateVisualElement(false, true);
                        shownConnections.Add(connection);
                    }

                    foreach (MethodNode tip in foot.ingoingConnections) //tip and foot here have opposite meanings
                    {
                        if (foot == tip)
                        {
                            continue;
                        }

                        VisualElement shownTip = shownMethodNodes.Contains(tip) ? tip.MethodGUI.VisualElement : null;
                        ConnectionGUI connection = new ConnectionGUI(shownTip,
                            foot.MethodGUI.VisualElement);
                        connection.GenerateVisualElement(false, true);
                        shownConnections.Add(connection);
                    }
                }
                else
                {
                    foreach (MethodNode tip in foot.outgoingConnections)
                    {
                        if (foot == tip)
                        {
                            continue;
                        }

                        ConnectionGUI connection =
                            new ConnectionGUI(foot.MethodGUI.VisualElement, tip.MethodGUI.VisualElement);
                        connection.GenerateVisualElement(false, true);
                        shownConnections.Add(connection);
                    }
                }
            }

            AppendShownConnectionsToGraphRoot();
        }


        /// <summary>
        /// Removes all VisualElements that are connections
        /// </summary>
        private void RemoveConnectionsFromGraphRoot()
        {
            foreach (ConnectionGUI connection in shownConnections)
            {
                try
                {
                    graphRoot.Remove(connection.VisualElement);
                }
                catch (ArgumentException)
                {
                }
            }
        }

        /// <summary>
        /// Adds all VisualElements that are connections to the editor Window
        /// </summary>
        private void AppendShownConnectionsToGraphRoot()
        {
            foreach (ConnectionGUI connection in shownConnections)
            {
                graphRoot.Add(connection.VisualElement);
            }
        }

        /// <summary>
        /// Adds all VisualElements that are ClassNodes to the editor Window
        /// </summary>
        private void AppendShownNodesToGraphRoot()
        {
            foreach (ClassNode node in ClassNodes)
            {
                if (graphRoot == node.classGUI.VisualElement.parent)
                {
                    graphRoot.Remove(node.classGUI.VisualElement);
                }
            }

            foreach (ClassNode node in shownClassNodes)
            {
                graphRoot.Add(node.classGUI.VisualElement);
            }
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