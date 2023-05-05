using Codice.Client.Common.TreeGrouper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.UIElements;

namespace CodeExplorinator
{
    public class GraphManager
    {
        /// <summary>
        /// the currently focused on classNode in the graph
        /// </summary>
        public ClassNode focusedClassNode;

        /// <summary>
        /// The currently focused methodNode in the graph. This is null if the method layer is inactive
        /// </summary>
        public MethodNode focusedMethodNode;

        /// <summary>
        /// The maximum distance as edges from the focused nodes until which other nodes are still shown
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
        /// all methodNodes within the shownDepth from focusedMethodNode
        /// </summary>
        private HashSet<MethodNode> shownMethodNodes;

        /// <summary>
        /// all classNodes in the currently analyzed project
        /// </summary>
        private HashSet<ClassNode> classNodes;

        /// <summary>
        /// all classNodes within the shownDepth from focusedMethodNode
        /// </summary>
        private HashSet<ClassNode> shownClassNodes;

        /// <summary>
        /// A list will all currently displayed connections between nodes
        /// </summary>
        private List<ConnectionGUI> shownConnections;

        public GraphManager(List<ClassData> data, VisualElement graphRoot, int shownDepth)
        {
            this.shownDepth = shownDepth;
            this.graphRoot = graphRoot;

            classNodes = new HashSet<ClassNode>();
            methodNodes = new HashSet<MethodNode>();
            shownMethodNodes = new HashSet<MethodNode>();
            //populate the lists with data
            foreach(ClassData @class in data)
            {
                classNodes.Add(GenerateNode(@class));
            }
            ClassNode.CopyRerefencesFromClassData(classNodes);
            foreach(ClassNode classNode in classNodes)
            {
                foreach (MethodGUI methodGUI in classNode.classGUI.methodGUIs)
                {
                    methodNodes.Add(GenerateNode(methodGUI.data, methodGUI));
                }
            }
            MethodNode.CopyRerefencesFromMethodData(methodNodes);
            shownConnections = new List<ConnectionGUI>();
            //UpdateFocusClass(classNodes.Where(x => x.ClassData.ToString().ToLower().Contains("classdata")).First().ClassData);
            
            //Assign first focused class
            UpdateFocusClass(classNodes.First().ClassData);
        }

        /// <summary>
        /// Changes the focus class and adapts the UI to it
        /// </summary>
        /// <param name="classData"></param>
        public void UpdateFocusClass(ClassData classData)
        {
            focusedClassNode = classData.ClassNode;
            RedrawGraph();
            focusedClassNode.classGUI.VisualElement.BringToFront();
        }

        /// <summary>
        /// Changes the shownDepth and adapts the UI to it
        /// </summary>
        /// <param name="depth"></param>
        public void UpdateReferenceDepth(int depth)
        {
            shownDepth = depth;
            RedrawGraph();
        }

        /// <summary>
        /// Changes the focus method and adapths the UI to it
        /// </summary>
        /// <param name="methodData"></param>
        public void UpdateFocusMethod(MethodData methodData)
        {
            focusedMethodNode = methodData.MethodNode;
            DrawMethodGraph();
            focusedMethodNode.MethodData.ContainingClass.ClassNode.classGUI.VisualElement.BringToFront();
        }

        /// <summary>
        /// Redraws all VisualElements within the graph
        /// </summary>
        private void RedrawGraph()
        {
            BreadthSearch.Reset();
            RedrawNodes();
            RedrawConnections();
        }

        /// <summary>
        /// Redraws all VisualElements that are nodes
        /// </summary>
        private void RedrawNodes()
        {
            shownClassNodes = BreadthSearch.GenerateClassSubgraph(classNodes, focusedClassNode, shownDepth);
            SpringEmbedderAlgorithm.StartAlgorithm(shownClassNodes.ToList(), 100000, 1000);
            AppendShownNodesToGraphRoot();
        }

        /// <summary>
        /// Redraws all VisualElements that are connections
        /// </summary>
        private void RedrawConnections()
        {
            RemoveConnectionsFromGraphRoot();
            shownConnections = new List<ConnectionGUI>();

            foreach(ClassNode foot in shownClassNodes)
            {
                if (foot.IsLeaf)
                {
                    foreach (ClassNode tip in foot.outgoingConnections)
                    {
                        if (foot == tip)
                        {
                            continue;
                        }
                        ClassNode shownTip = shownClassNodes.Contains(tip) ? tip : null;
                        ConnectionGUI connection = new ConnectionGUI(foot, shownTip);
                        connection.GenerateVisualElement();
                        shownConnections.Add(connection);
                    }
                    
                    foreach (ClassNode tip in foot.ingoingConnections) //tip and foot here have opposite meanings
                    {
                        if (foot == tip)
                        {
                            continue;
                        }
                        
                        ClassNode shownTip = shownClassNodes.Contains(tip) ? tip : null;
                        ConnectionGUI connection = new ConnectionGUI(shownTip, foot);
                        connection.GenerateVisualElement();
                        shownConnections.Add(connection);
                    }

                    foreach (var parent in foot.ClassData.ExtendingOrImplementingClasses)
                    {
                        if (shownClassNodes.Contains(parent.ClassNode))
                        {
                            ConnectionGUI connection = new ConnectionGUI(foot, parent.ClassNode);
                            connection.GenerateVisualElement(true);
                            shownConnections.Add(connection);
                        }
                        else
                        {
                            ConnectionGUI connection = new ConnectionGUI(foot, null);
                            connection.GenerateVisualElement();
                            shownConnections.Add(connection);
                        }
                    }
                }
                else
                {
                    foreach (ClassNode tip in foot.outgoingConnections)
                    {
                        if (foot == tip)
                        {
                            continue;
                        }

                        ConnectionGUI connection = new ConnectionGUI(foot, tip);
                        connection.GenerateVisualElement();
                        shownConnections.Add(connection);
                    }

                    foreach (var parent in foot.ClassData.ExtendingOrImplementingClasses)
                    {
                        ConnectionGUI connection = new ConnectionGUI(foot, parent.ClassNode);
                        connection.GenerateVisualElement(true);
                        shownConnections.Add(connection);
                    }
                }
            }

            AppendShownConnectionsToGraphRoot();
        }

        /// <summary>
        /// Draws the method layer
        /// </summary>
        private void DrawMethodGraph()
        {
            foreach(MethodNode node in shownMethodNodes)
            {
                node.MethodGUI.ShowBackground(false);
            }

            shownMethodNodes = BreadthSearch.GenerateMethodSubgraph(focusedMethodNode, shownDepth);
            foreach (MethodNode node in shownMethodNodes)
            {
                node.MethodGUI.ShowBackground(true);
            }
            //HashSet<ClassNode> classesWithMethods = AddMethodsToClasses(shownMethodNodes);
            //SpringEmbedderAlgorithm.StartMethodAlgorithm(classesWithMethods, shownMethodNodes, 100000, 1000);
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
                catch (ArgumentException) { }
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
        /// Adds all VisualElements that are classNodes to the editor Window
        /// </summary>
        private void AppendShownNodesToGraphRoot()
        {
            foreach (ClassNode node in classNodes)
            {
                if (graphRoot == node.classGUI.VisualElement.parent)
                {
                    graphRoot.Remove(node.classGUI.VisualElement);
                }
            }

            foreach(ClassNode node in shownClassNodes)
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
            ClassGUI classGUI = new ClassGUI(new Vector2(UnityEngine.Random.Range(-50, 50) - graphRoot.style.marginLeft.value.value, UnityEngine.Random.Range(-50, 50) - graphRoot.style.marginTop.value.value), classData, classStyle, methodStyle, methodStyle, this);
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
        /// <returns>A set of all classes which have been assigned a method by this method</returns>
        private HashSet<ClassNode> AddMethodsToClasses(HashSet<MethodNode> methodNodes)
        {
            HashSet<ClassNode> classNodes = new HashSet<ClassNode>();
            
            foreach (MethodNode methodNode in methodNodes)
            {
                methodNode.MethodData.ContainingClass.ClassNode.MethodNodes.Add(methodNode);
                classNodes.Add(methodNode.MethodData.ContainingClass.ClassNode);
            }

            return classNodes;
        }

        public void AddNode(ClassNode node)
        {
            classNodes.Add(node);
        }

        public void AddNodes(IEnumerable<ClassNode> nodes)
        {
            foreach (ClassNode node in nodes)
            {
                this.classNodes.Add(node);
            }
        }

        public void RemoveNode(ClassNode node)
        {
            classNodes.Remove(node);
        }

        public void RemoveNodes(IEnumerable<ClassNode> nodes)
        {
            foreach(ClassNode node in nodes.Where(x => this.classNodes.Contains(x)))
            {
                this.classNodes.Remove(node);
            }
        }
    }
}