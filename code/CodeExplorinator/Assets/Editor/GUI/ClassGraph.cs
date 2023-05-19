using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace CodeExplorinator
{
    /// <summary>
    /// A container for a connected graph that consists of ClassNodes
    /// As long as this graph stays connected, it may contain multiple focused ClassNodes
    /// </summary>
    public class ClassGraph
    {
        /// <summary>
        /// This graph's root element
        /// </summary>
        public VisualElement root { get; private set; }

        /// <summary>
        /// All ClassNodes that this graph contains
        /// </summary>
        private HashSet<ClassNode> classNodes;

        /// <summary>
        /// All currently focused Classes in this graph
        /// </summary>
        private HashSet<ClassNode> focusedClassNodes;

        private HashSet<ConnectionGUI> connections;

        /// <summary>
        /// The maximum distance from a ClassNode to its closest focused ClassNode (0 == only focused classes)
        /// </summary>
        private int classDepth;

        public ClassGraph(HashSet<ClassNode> classNodes, HashSet<ClassNode> focusedClassNodes, int classDepth)
        {
            root = new VisualElement();
            connections = new HashSet<ConnectionGUI>();

            this.classNodes = classNodes;
            this.focusedClassNodes = focusedClassNodes;
            this.classDepth = classDepth;

            DoAutoLayout();
            GenerateConnections();
            GenerateVisualElementGraph();
        }

        private void DoAutoLayout()
        {
            SpringEmbedderAlgorithm.StartAlgorithm(classNodes.ToList());
        }

        private void GenerateVisualElementGraph()
        {
            foreach (ConnectionGUI connection in connections)
            {
                root.Add(connection.VisualElement);
            }
            foreach (ClassNode node in classNodes)
            {
                root.Add(node.classGUI.VisualElement);
            }
        }

        private void GenerateConnections()
        {
            foreach (ClassNode foot in classNodes)
            {
                if (foot.IsLeaf)
                {
                    foreach (ClassNode tip in foot.outgoingConnections)
                    {
                        if (foot == tip)
                        {
                            continue;
                        }

                        VisualElement shownTip = classNodes.Contains(tip) ? tip.classGUI.VisualElement : null;
                        ConnectionGUI connection = new ConnectionGUI(foot.classGUI.VisualElement, shownTip);
                        connection.GenerateVisualElement();
                        connections.Add(connection);
                    }

                    foreach (ClassNode tip in foot.ingoingConnections) //tip and foot here have opposite meanings
                    {
                        if (foot == tip)
                        {
                            continue;
                        }

                        VisualElement shownTip = classNodes.Contains(tip) ? tip.classGUI.VisualElement : null;
                        ConnectionGUI connection = new ConnectionGUI(shownTip,
                            foot.classGUI.VisualElement);
                        connection.GenerateVisualElement();
                        connections.Add(connection);
                    }

                    foreach (var parent in foot.ClassData.ExtendingOrImplementingClasses)
                    {
                        ConnectionGUI connection = new ConnectionGUI(foot.classGUI.VisualElement,
                            classNodes.Contains(parent.ClassNode)
                                ? parent.ClassNode.classGUI.VisualElement
                                : null);
                        connection.GenerateVisualElement(true);
                        connections.Add(connection);
                    }

                    foreach (var child in
                             foot.ClassData.ChildClasses) //tip(aka child) and foot here have opposite meanings
                    {
                        ConnectionGUI connection = new ConnectionGUI(
                            classNodes.Contains(child.ClassNode) ? child.ClassNode.classGUI.VisualElement : null,
                            foot.classGUI.VisualElement);
                        connection.GenerateVisualElement(true);
                        connections.Add(connection);
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

                        ConnectionGUI connection =
                            new ConnectionGUI(foot.classGUI.VisualElement, tip.classGUI.VisualElement);
                        connection.GenerateVisualElement();
                        connections.Add(connection);
                    }

                    foreach (var parent in foot.ClassData.ExtendingOrImplementingClasses)
                    {
                        ConnectionGUI connection = new ConnectionGUI(foot.classGUI.VisualElement,
                            parent.ClassNode.classGUI.VisualElement);
                        connection.GenerateVisualElement(true);
                        connections.Add(connection);
                    }
                }
            }
        }
    }
}