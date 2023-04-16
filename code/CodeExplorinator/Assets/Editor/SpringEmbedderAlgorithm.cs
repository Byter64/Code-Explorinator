using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


namespace CodeExplorinator
{
    public static class SpringEmbedderAlgorithm
    {
        public static void CalculateOld(List<ClassNode> nodes, double threshold, int iterations)
        {
            int t = 1;

           

            while (t <= iterations)
            {
                foreach (var node in nodes)
                {
                    if (node.F.magnitude > threshold) //somehow the threshold does nothing, even when 1?
                    {
                        return;
                    }
                }
                
                
                foreach (var node in nodes)
                {
                    Vector2 resultRepulsion = Vector2.zero;
                    Vector2 resultSpring = Vector2.zero;
                    DetermineConnectionBetweenNodes(node,nodes);
                    
                    foreach (var connectedNode in node.ConnectedNodes)
                    {
                        resultRepulsion += ForceSpring(connectedNode,node);
                    }

                    foreach (var notConnectedNode in node.NotConnectedNodes)
                    {
                        resultSpring += ForceRepulsion(notConnectedNode,node);
                    }

                    node.F = resultRepulsion + resultSpring;
                }

                float cooling1 = coolingFactor(t, iterations);

                foreach (var node in nodes)
                {
                    node.classGUI.VisualElement.style.marginLeft = node.classGUI.VisualElement.style.marginLeft.value.value + cooling1 * node.F.x;
                    node.classGUI.VisualElement.style.marginTop = node.classGUI.VisualElement.style.marginTop.value.value + cooling1 * node.F.y;
                    
                    Debug.Log("Node "+ node.ClassData.GetName() + ": " + node.classGUI.VisualElement.style.marginLeft + "/" + node.classGUI.VisualElement.style.marginTop);
                }

                t++;
            }
            
            //CleanupMiddlePoint(nodes);
        }
        
        //this method calculates the position of the node, it still doesnt check if the node is placed out of bounds
        public static void StartAlgorithm(List<ClassNode> nodes, double threshold, int iterations)
        {
            int t = 1;

           

            while (t <= iterations)
            {
                foreach (var node in nodes)
                {
                    if (node.F.magnitude > threshold)
                    {
                        return;
                    }
                }
                
                
                foreach (var node in nodes)
                {
                    Vector2 resultRepulsion = Vector2.zero;
                    Vector2 resultSpring = Vector2.zero;
                    DetermineConnectionBetweenNodes(node,nodes);
                    
                    foreach (var connectedNode in node.ConnectedNodes)
                    {
                        resultRepulsion += ForceSpring(connectedNode,node);
                    }

                    foreach (var notConnectedNode in node.NotConnectedNodes)
                    {
                        resultSpring += ForceRepulsion(notConnectedNode,node);
                    }

                    node.F = resultRepulsion + resultSpring;
                }

                float cooling = coolingFactor(t, iterations);

                foreach (var node in nodes)
                {
                    node.position.x += cooling * node.F.x;
                    node.position.y += cooling * node.F.y;
                    
                    //Debug.Log("Node "+ node.ClassData.GetName() + ": " + node.position.x + "/" + node.position.y);
                }

                t++;
            }

            //as we incorporated the height and width into the Node.position Vector2, we now have to undo this:
            foreach (var node in nodes)
            {
                node.classGUI.VisualElement.style.marginLeft = node.position.x - node.classGUI.VisualElement.style.width.value.value * 0.5f;
                node.classGUI.VisualElement.style.marginTop = node.position.y - node.classGUI.VisualElement.style.height.value.value * 0.5f;
            }
            
        }

        private static void DetermineConnectionBetweenNodes(ClassNode analysedNode, List<ClassNode> allNodes)
        {
            
            foreach (var connectedClass in analysedNode.ClassData.AllConnectedClasses)
            {
                foreach (var node in allNodes)
                {
                    //if its not the same class:
                    if (node.ClassData != analysedNode.ClassData)
                    {
                        if (node.ClassData == connectedClass)
                        {
                            if (!analysedNode.ConnectedNodes.Contains(node))
                            {
                                analysedNode.ConnectedNodes.Add(node);
                            }
                        }
                        else
                        {
                            if (!analysedNode.NotConnectedNodes.Contains(node))
                            {
                                analysedNode.NotConnectedNodes.Add(node);
                            }
                        }
                    }
                    
                }
            }
            
        }

        private static Vector2 ForceRepulsion(ClassNode u, ClassNode v, float normFactor = 5f)
        {
            Vector2 unitVectorFromUtoV =
                new Vector2(v.position.x - u.position.x, v.position.y - u.position.y).normalized;

            double result1 = Math.Pow((v.position - u.position).magnitude, 2);

            if (result1 == 0)
            {
                result1 = float.Epsilon;
            }
            
            float factor = (float)(normFactor / result1);
            
            return new Vector2(factor * unitVectorFromUtoV.x,factor * unitVectorFromUtoV.y ) ;
        }

        private static float coolingFactor(int t, int iterations, int coolingSpeed = 2)
        {
            //if half the iterationnumber is reached, the force becomes weaker

            if (t == 0)
            {
                return 1;
            }
            return iterations % (coolingSpeed * t);
        }

        private static Vector2 ForceSpring(ClassNode u, ClassNode v, float normFactor = 1, float idealSpringLength = 1000f)
        {
            
            if (idealSpringLength == 0)
            {
                idealSpringLength = float.Epsilon;
            }

            Vector2 unitVectorFromVtoU =
                new Vector2(u.position.x - v.position.x, u.position.y - v.position.y).normalized;

            double result1 = (u.position - v.position).magnitude / idealSpringLength;
            if (result1 == 0)
            {
                result1 = float.Epsilon;
            }

            double result2 = Math.Log10(result1);

            if (result2 == 0)
            {
                result2 = float.Epsilon;
            }

            float factor = normFactor * (float) result2; 
            
            return new Vector2(factor * unitVectorFromVtoU.x, factor * unitVectorFromVtoU.y);
        }
        
    }
}