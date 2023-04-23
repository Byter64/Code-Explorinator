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
        public ClassNode focusedClassNode;
        public MethodNode focusedMethodNode; //this is never set PLEASE IMPLEMENT

        private int shownDepth;
        private VisualElement graphRoot;
        private HashSet<ClassNode> nodes;
        private HashSet<ClassNode> shownNodes;
        private List<ConnectionGUI> shownConnections;
        public GraphManager(List<ClassData> data, VisualElement graphRoot, int shownDepth)
        {
            this.shownDepth = shownDepth;
            this.graphRoot = graphRoot;

            nodes = new HashSet<ClassNode>();
            foreach(ClassData @class in data)
            {
                nodes.Add(GenerateNode(@class));
            }
            ClassNode.CopyRerefencesFromClassData(nodes);

            shownConnections = new List<ConnectionGUI>();
            //UpdateFocusClass(nodes.Where(x => x.ClassData.ToString().ToLower().Contains("classdata")).First().ClassData);
            
            UpdateFocusClass(nodes.First().ClassData);
        }

        public void UpdateFocusClass(ClassData classData)
        {
            focusedClassNode = classData.ClassNode;
            RedrawGraph();
        }

        public void UpdateReferenceDepth(int depth)
        {
            shownDepth = depth;
            RedrawGraph();
        }
        private void RedrawGraph()
        {
            BreadthSearch.Reset();
            RedrawNodes();
            RedrawConnections();
        }

        private void RedrawNodes()
        {
            shownNodes = BreadthSearch.GenerateClassSubgraph(nodes, focusedClassNode, shownDepth);
            SpringEmbedderAlgorithm.StartAlgorithm(shownNodes.ToList(), 100000, 1000);
            AppendShownNodesToGraphRoot();
        }

        private void RedrawConnections()
        {
            RemoveConnectionsFromGraphRoot();
            shownConnections = new List<ConnectionGUI>();

            foreach(ClassNode foot in shownNodes)
            {
                if (foot.IsLeaf)
                {
                    foreach (ClassNode tip in foot.outgoingConnections)
                    {
                        if (foot == tip)
                        {
                            continue;
                        }
                        ClassNode shownTip = shownNodes.Contains(tip) ? tip : null;
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
                        
                        ClassNode shownTip = shownNodes.Contains(tip) ? tip : null;
                        ConnectionGUI connection = new ConnectionGUI(shownTip, foot);
                        connection.GenerateVisualElement();
                        shownConnections.Add(connection);
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
                }
               
            }

            AppendShownConnectionsToGraphRoot();
        }

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

        private void AppendShownConnectionsToGraphRoot()
        {
            foreach (ConnectionGUI connection in shownConnections)
            {
                graphRoot.Add(connection.VisualElement);
            }
        }

        private void AppendShownNodesToGraphRoot()
        {
            foreach (ClassNode node in nodes)
            {
                if (graphRoot == node.classGUI.VisualElement.parent)
                {
                    graphRoot.Remove(node.classGUI.VisualElement);
                }
            }

            foreach(ClassNode node in shownNodes)
            {
                graphRoot.Add(node.classGUI.VisualElement);
            }
        }

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
            //Debug.Log("Visualelement: " + node.classGUI.style.marginLeft + "/" + node.classGUI.style.marginTop);
            classData.ClassNode = node;
            return node;
        }

        public void DrawMethodGraph()
        {
            HashSet<MethodNode> analysedMethodNodes = BreadthSearch.GenerateMethodSubgraph(focusedMethodNode, shownDepth);
            HashSet<ClassNode> analysedClasses = AddMethodsToClasses(analysedMethodNodes);
            SpringEmbedderAlgorithm.StartMethodAlgorithm(analysedClasses,analysedMethodNodes,100000,1000);
        }

        private HashSet<ClassNode> AddMethodsToClasses(HashSet<MethodNode> methodNodes)
        {
            HashSet<ClassNode> classNodes = new HashSet<ClassNode>();
            
            foreach (var methodNode in methodNodes)
            {
                methodNode.MethodData.ContainingClass.ClassNode.MethodNodes.Add(methodNode);
                classNodes.Add(methodNode.MethodData.ContainingClass.ClassNode);
            }

            return classNodes;
        }

        public void AddNode(ClassNode node)
        {
            nodes.Add(node);
        }

        public void AddNodes(IEnumerable<ClassNode> nodes)
        {
            foreach (ClassNode node in nodes)
            {
                this.nodes.Add(node);
            }
        }

        public void RemoveNode(ClassNode node)
        {
            nodes.Remove(node);
        }

        public void RemoveNodes(IEnumerable<ClassNode> nodes)
        {
            foreach(ClassNode node in nodes.Where(x => this.nodes.Contains(x)))
            {
                this.nodes.Remove(node);
            }
        }
    }
}