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
        public VisualElement Root { get; private set; }

        /// <summary>
        /// All ClassNodes that this graph contains
        /// </summary>
        public  HashSet<ClassNode> classNodes;

        public HashSet<ConnectionGUI> connections;

        /// <summary>
        /// All currently focused Classes in this graph
        /// </summary>
        private HashSet<ClassNode> focusedClassNodes;

        /// <summary>
        /// The maximum distance from a ClassNode to its closest focused ClassNode (0 == only focused classes)
        /// </summary>
        private int classDepth;

        private GraphManager graphManager;

        public ClassGraph(GraphManager graphManager, HashSet<ClassNode> classNodes, HashSet<ClassNode> focusedClassNodes, int classDepth)
        {
            Root = new VisualElement();
            connections = new HashSet<ConnectionGUI>();

            this.graphManager = graphManager;
            this.classNodes = classNodes;
            this.focusedClassNodes = focusedClassNodes;
            this.classDepth = classDepth;
        }

        public void GenerateVisualElementGraph()
        {
            foreach (ConnectionGUI connection in connections)
            {
                Root.Add(connection.VisualElement);
            }
            foreach (ClassNode node in classNodes)
            {
                Root.Add(node.classGUI.VisualElement);
            }
        }

        public void GenerateConnectionsBetweenClasses()
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
                        ConnectionGUI connection = new ConnectionGUI(graphManager, foot.classGUI.VisualElement, shownTip);
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
                        ConnectionGUI connection = new ConnectionGUI(graphManager, shownTip,
                            foot.classGUI.VisualElement);
                        connection.GenerateVisualElement();
                        connections.Add(connection);
                    }

                    foreach (var parent in foot.ClassData.ExtendingOrImplementingClasses)
                    {
                        ConnectionGUI connection = new ConnectionGUI(graphManager, foot.classGUI.VisualElement,
                            classNodes.Contains(parent.ClassNode)
                                ? parent.ClassNode.classGUI.VisualElement
                                : null, true);
                        connection.GenerateVisualElement();
                        connections.Add(connection);
                    }

                    foreach (var child in
                             foot.ClassData.ChildClasses) //tip(aka child) and foot here have opposite meanings
                    {
                        ConnectionGUI connection = new ConnectionGUI(graphManager,
                            classNodes.Contains(child.ClassNode) ? child.ClassNode.classGUI.VisualElement : null,
                            foot.classGUI.VisualElement, true);
                        connection.GenerateVisualElement();
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
                            new ConnectionGUI(graphManager, foot.classGUI.VisualElement, tip.classGUI.VisualElement);
                        connection.GenerateVisualElement();
                        connections.Add(connection);
                    }

                    foreach (var parent in foot.ClassData.ExtendingOrImplementingClasses)
                    {
                        ConnectionGUI connection = new ConnectionGUI(graphManager, foot.classGUI.VisualElement,
                            parent.ClassNode.classGUI.VisualElement, true);
                        connection.GenerateVisualElement();
                        connections.Add(connection);
                    }
                }
            }
        }

        /*
        //this code could be obsolete
        public void GenerateConnectionsBetweenMethods(HashSet<MethodNode> shownMethodNodes)
        {
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
                        ConnectionGUI connection = new ConnectionGUI(graphManager, foot.MethodGUI.VisualElement,
                            shownTip, false, true);
                        connection.GenerateVisualElement();
                        connections.Add(connection);
                    }

                    foreach (MethodNode tip in foot.ingoingConnections) //tip and foot here have opposite meanings
                    {
                        if (foot == tip)
                        {
                            continue;
                        }

                        VisualElement shownTip = shownMethodNodes.Contains(tip) ? tip.MethodGUI.VisualElement : null;
                        ConnectionGUI connection = new ConnectionGUI(graphManager,shownTip,
                            foot.MethodGUI.VisualElement, false, true);
                        connection.GenerateVisualElement();
                        connections.Add(connection);
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
                            new ConnectionGUI(graphManager, foot.MethodGUI.VisualElement, tip.MethodGUI.VisualElement, false, true);
                        connection.GenerateVisualElement();
                        connections.Add(connection);
                    }
                }
            }
        }
        */

        public void GenerateMethodConnectionsBetweenClasses()
        {
             foreach (ClassNode foot in classNodes)
             {
                 foreach (ClassNode tip in foot.ConnectedNodes)
                 {
                     if (foot == tip)
                     {
                         continue;
                     }
                     
                     ConnectionGUI connection = new ConnectionGUI(graphManager, foot.classGUI.VisualElement, tip.classGUI.VisualElement, false, false, false);
                     connection.GenerateVisualElement();
                     connections.Add(connection);
                 }
             }
        }

        public bool Contains(ClassGUI classGUI)
        {
            bool isPartOfGraph = false;
            foreach(ClassNode node in classNodes)
            {
                if(node.classGUI == classGUI)
                {
                    return true;
                }
            }

            return false;
        }
    }
}