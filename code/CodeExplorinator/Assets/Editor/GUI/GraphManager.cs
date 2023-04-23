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
        public ClassNode focusedNode;

        private int shownDepth;
        private VisualElement graphRoot;
        private BreadthSearch breadthSearch;
        private HashSet<ClassNode> nodes;
        private HashSet<ClassNode> shownNodes;
        private List<ConnectionGUI> shownConnections;
        public GraphManager(List<ClassData> data, VisualElement graphRoot, int shownDepth)
        {
            this.shownDepth = shownDepth;
            this.graphRoot = graphRoot;
            breadthSearch = new BreadthSearch();

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
            focusedNode = classData.ClassNode;
            RedrawGraph();
        }

        public void UpdateReferenceDepth(int depth)
        {
            shownDepth = depth;
            RedrawGraph();
        }
        private void RedrawGraph()
        {
            breadthSearch.Reset();
            RedrawNodes();
            RedrawConnections();
        }

        private void RedrawNodes()
        {
            shownNodes = breadthSearch.GenerateSubgraph(nodes, focusedNode, shownDepth);
            SpringEmbedderAlgorithm.StartAlgorithm(shownNodes.ToList(), 100000, 1000);
            AppendShownNodesToGraphRoot();
        }

        private void RedrawConnections()
        {
            RemoveConnectionsFromGraphRoot();
            shownConnections = new List<ConnectionGUI>();

            foreach(ClassNode tip in shownNodes) //tip and foot need to be swapped
            {
                foreach (ClassNode foot in tip.outgoingConnections)
                {
                    if (tip == foot)
                    {
                        continue;
                    }
                    ClassNode shownFoot = shownNodes.Contains(foot) ? foot : null;
                    ConnectionGUI connection = new ConnectionGUI(tip, shownFoot);
                    connection.GenerateVisualElement();
                    shownConnections.Add(connection);
                }
            }

            /*
             foreach (ClassNode foot in shownNodes)
            {
                foreach (ClassNode tip in foot.ingoingConnections)
                {
                    ClassNode showntip = shownNodes.Contains(tip) ? tip : null;
                    ConnectionGUI connection = new ConnectionGUI(showntip, foot);
                    connection.GenerateVisualElement();
                    shownConnections.Add(connection);
                }
            }
             */
            
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