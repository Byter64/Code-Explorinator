using System.Collections;
using System.Collections.Generic;
using System.Drawing;
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
            foreach(ClassNode node in classNodes.Where(x => x.IsLeaf))
            {
                int connectionsThatLeadOutsideTheGraph = 0;
                int connectionsThatOriginateOutsideTheGraph = 0;
                foreach(ClassNode otherNode in node.outgoingConnections)
                {
                    if(classNodes.Contains(otherNode) == false)
                    {
                        connectionsThatLeadOutsideTheGraph++;
                        continue;
                    }

                    ConnectionGUI connection = new ConnectionGUI(graphManager, node.classGUI.VisualElement, otherNode.classGUI.VisualElement);
                    connections.Add(connection);
                }

                foreach (ClassNode otherNode in node.ingoingConnections)
                {
                    if (classNodes.Contains(otherNode) == false)
                    {
                        connectionsThatOriginateOutsideTheGraph++;
                        continue;
                    }
                }

                foreach (ClassData otherNode in node.ClassData.ExtendingOrImplementingClasses)
                {
                    if(classNodes.Contains(otherNode.ClassNode) == false)
                    {
                        connectionsThatLeadOutsideTheGraph++;
                        continue;
                    }

                    ConnectionGUI connection = new ConnectionGUI(graphManager, node.classGUI.VisualElement, 
                                                                otherNode.ClassNode.classGUI.VisualElement, true);
                    connections.Add(connection);
                }

                foreach (ClassData otherNode in node.ClassData.ChildClasses)
                {
                    if (classNodes.Contains(otherNode.ClassNode) == false)
                    {
                        connectionsThatOriginateOutsideTheGraph++;
                        continue;
                    }
                }

                if(connectionsThatLeadOutsideTheGraph != 0)
                {
                    ConnectionGUI implicatingArrowAway = new ConnectionGUI(graphManager, node.classGUI.VisualElement, false);
                    connections.Add(implicatingArrowAway);
                }

                if(connectionsThatOriginateOutsideTheGraph != 0)
                {
                    ConnectionGUI implicatingArrowToward = new ConnectionGUI(graphManager, node.classGUI.VisualElement, true);
                    connections.Add(implicatingArrowToward);
                }
            }

            foreach (ClassNode node in classNodes.Where(x => !x.IsLeaf))
            {
                foreach (ClassNode otherNode in node.outgoingConnections)
                {
                    ConnectionGUI connection = new ConnectionGUI(graphManager, node.classGUI.VisualElement, otherNode.classGUI.VisualElement);
                    connections.Add(connection);
                }

                foreach (ClassData otherNode in node.ClassData.ExtendingOrImplementingClasses)
                {
                    ConnectionGUI connection = new ConnectionGUI(graphManager, node.classGUI.VisualElement,
                                                                 otherNode.ClassNode.classGUI.VisualElement, true);
                    connections.Add(connection);
                }
            }
        }

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
                     connections.Add(connection);
                 }
             }
        }

        public bool Contains(ClassGUI classGUI)
        {
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