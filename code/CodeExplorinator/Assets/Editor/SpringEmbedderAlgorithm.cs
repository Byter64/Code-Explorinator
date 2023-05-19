using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


namespace CodeExplorinator
{
    public static class SpringEmbedderAlgorithm
    {
        
        public static void StartAlgorithm(List<ClassNode> nodes, double threshold, int iterations)
        {
            int t = 1;

            DetermineConnectionBetweenNodes(nodes);


            while (t <= iterations)
            {
                foreach (var node in nodes)
                {
                    if (node.F.magnitude > threshold)
                    {
                        return;
                    }
                }

                //Berechne für alle Knoten die insgesamte Kraft, die auf sie wirkt
                foreach (var node in nodes)
                {
                    Vector2 resultRepulsion = Vector2.zero;
                    Vector2 resultSpring = Vector2.zero;

                    foreach (var connectedNode in node.ConnectedNodes)
                    {
                        //Anziehung 
                        resultSpring += ForceSpring(connectedNode, node);
                    }

                    foreach (var notConnectedNode in node.NotConnectedNodes)
                    {
                        //Abstoßung
                        resultRepulsion += ForceRepulsion(notConnectedNode, node);
                    }

                    node.F = resultRepulsion + resultSpring;
                }

                float cooling = coolingFactor(t, iterations);

                //Wende die Kraft auf die Position (als Geschwindigkeit??) an
                foreach (var node in nodes)
                {
                    node.position.x += cooling * node.F.x;
                    node.position.y += cooling * node.F.y;

                    //Debug.Log("Node "+ node.ClassData.GetName() + ": " + node.position.x + "/" + node.position.y);
                }

                t++;
            }

            //Aktualisier Position auf VisualElements
            //as we incorporated the height and width into the Node.position Vector2, we now have to undo this:
            foreach (var node in nodes)
            {
                node.classGUI.VisualElement.style.marginLeft =
                    node.position.x - node.classGUI.VisualElement.style.width.value.value * 0.5f;
                node.classGUI.VisualElement.style.marginTop =
                    node.position.y - node.classGUI.VisualElement.style.height.value.value * 0.5f;
            }
        }

        private static void DetermineConnectionBetweenNodes(List<ClassNode> allNodes)
        {
            foreach (var analysedNode in allNodes)
            {
                analysedNode.ConnectedNodes.Clear();
                analysedNode.NotConnectedNodes.Clear();

                foreach (var connectedClass in analysedNode.ClassData.AllConnectedClasses)
                {
                    foreach (var randomNode in allNodes)
                    {
                        //if the method is a leaf, we have to check whether the connection leads to a drawn class or not
                        if (analysedNode.IsLeaf)
                        {
                            //if its not the same class:
                            if (randomNode.ClassData != analysedNode.ClassData &&
                                allNodes.Contains(connectedClass.ClassNode))
                            {
                                if (randomNode.ClassData == connectedClass)
                                {
                                    analysedNode.ConnectedNodes.Add(randomNode);
                                }
                                else
                                {
                                    analysedNode.NotConnectedNodes.Add(randomNode);
                                }
                            }
                        }
                        else
                        {
                            //if its not the same class:
                            if (randomNode.ClassData != analysedNode.ClassData)
                            {
                                if (randomNode.ClassData == connectedClass)
                                {
                                    analysedNode.ConnectedNodes.Add(randomNode);
                                }
                                else
                                {
                                    analysedNode.NotConnectedNodes.Add(randomNode);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void StartMethodAlgorithm(HashSet<ClassNode> nodes, HashSet<MethodNode> allMethods,
            double threshold, int iterations)
        {
            int t = 1;

            DetermineConnectionsBetweenMethodNodes(allMethods);

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

                    foreach (var methodNode in node.MethodNodes)
                    {
                        foreach (var connectedNode in methodNode.ConnectedNodes)
                        {
                            resultSpring += ForceSpring(connectedNode.MethodData.ContainingClass.ClassNode, node, 0.5f, 500f);
                        }
                    }

                    foreach (var methodNode in node.MethodNodes)
                    {
                        foreach (var notConnectedNode in methodNode.NotConnectedNodes)
                        {
                            resultRepulsion += ForceRepulsion(notConnectedNode.MethodData.ContainingClass.ClassNode, node, 1f);
                        }
                    }

                    node.F = resultRepulsion + resultSpring;
                }

                float cooling = coolingFactor(t, iterations);

                foreach (var node in nodes)
                {
                    node.position.x += cooling * node.F.x;
                    node.position.y += cooling * node.F.y;

                    //Debug.Log("Node " + node.ClassData.GetName() + ": " + node.position.x + "/" +
                    //          node.position.y);
                }

                t++;
            }

            //as we incorporated the height and width into the Node.position Vector2, we now have to undo this:
            foreach (var node in nodes)
            {
                node.classGUI.VisualElement.style.marginLeft =
                    node.position.x - node.classGUI.VisualElement.style.width.value.value * 0.5f;
                node.classGUI.VisualElement.style.marginTop =
                    node.position.y - node.classGUI.VisualElement.style.height.value.value * 0.5f;
            }
        }

        private static void DetermineConnectionsBetweenMethodNodes(HashSet<MethodNode> allNodes)
        {
            foreach (var method in allNodes)
            {
                method.ConnectedNodes.Clear();
                method.NotConnectedNodes.Clear();

                foreach (var connectedMethod in method.MethodData.AllConnectedMethods)
                {
                    if (connectedMethod.MethodNode != method)
                    {
                        foreach (var randomNode in allNodes)
                        {
                            //if the method is a leaf, we have to check whether the connection leads to a drawn class or not
                            if (method.IsLeaf)
                            {
                                if (method != randomNode && allNodes.Contains(connectedMethod.MethodNode))
                                {
                                    if (randomNode == connectedMethod.MethodNode)
                                    {
                                        method.ConnectedNodes.Add(randomNode);
                                    }
                                    else
                                    {
                                        method.NotConnectedNodes.Add(randomNode);
                                    }
                                }
                            }
                            else
                            {
                                if (method != randomNode)
                                {
                                    if (randomNode == connectedMethod.MethodNode)
                                    {
                                        method.ConnectedNodes.Add(randomNode);
                                    }
                                    else
                                    {
                                        method.NotConnectedNodes.Add(randomNode);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the repulsion of v that is generated by u
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <param name="repulsionConstant"></param>
        /// <returns></returns>
        private static Vector2 ForceRepulsion(ClassNode u, ClassNode v, float repulsionConstant = 4f)
        {
            Vector2 direction = (v.position - u.position);

            //factor = repulsionContant / (direction.magnitude²)
            float factor = direction.magnitude * direction.magnitude;
            factor = (factor == 0) ? float.Epsilon : factor;
            
            if (float.IsInfinity(factor)) factor = float.IsPositiveInfinity(factor) ? float.MaxValue : float.MinValue; //unnecessary
            factor = repulsionConstant / factor;
            if (float.IsInfinity(factor)) factor = float.IsPositiveInfinity(factor) ? float.MaxValue : float.MinValue; //probably unnecessary

            direction.Normalize();
            Vector2 result = factor * direction;
            if (float.IsInfinity(result.x)) result.x = float.IsPositiveInfinity(result.x) ? float.MaxValue : float.MinValue; //probably unnecessary
            if (float.IsInfinity(result.y)) result.y = float.IsPositiveInfinity(result.y) ? float.MaxValue : float.MinValue; //probably unnecessary

            return result;
        }

        private static float coolingFactor(int t, int iterations, int coolingSpeed = 3)
        {
            //if half the iterationnumber is reached, the force becomes weaker

            if (t == 0) return 1;
            if (coolingSpeed == 0) return 1;

            return iterations % (coolingSpeed * t);
        }

        /// <summary>
        /// Returns the attraction of v that is generated by u
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <param name="attractionConstant"></param>
        /// <param name="idealSpringLength"></param>
        /// <returns></returns>
        private static Vector2 ForceSpring(ClassNode u, ClassNode v, float attractionConstant = 1.6f, float idealSpringLength = 600f) //the bigger the ideal length, the shorter the lines; 500 is ca. the height of the standard class / the smaller the norm the stronger the force
        {
            if (idealSpringLength == 0) idealSpringLength = float.Epsilon;

            Vector2 direction = u.position - v.position;

            //factor = attractionConstant * log10(direction.magnitude / idealSpringLength)
            float factor = direction.magnitude / idealSpringLength;
            factor = (factor == 0) ? float.Epsilon : factor;
            if (float.IsInfinity(factor)) factor = float.IsPositiveInfinity(factor) ? float.MaxValue : float.MinValue; //probably uneccesary

            factor = Mathf.Log10(factor);

            if (factor == 0) factor = float.Epsilon; //probably uneccesary 
            if (float.IsInfinity(factor)) factor = float.IsPositiveInfinity(factor) ? float.MaxValue : float.MinValue; //probably uneccesary

            factor = attractionConstant * factor;
            if (float.IsInfinity(factor)) factor = float.IsPositiveInfinity(factor) ? float.MaxValue : float.MinValue; //probably uneccesary

            direction.Normalize();
            Vector2 result = factor * direction;
            if (float.IsInfinity(result.x)) result.x = float.IsPositiveInfinity(result.x) ? float.MaxValue : float.MinValue; //probably uneccesary
            if (float.IsInfinity(result.y)) result.y = float.IsPositiveInfinity(result.y) ? float.MaxValue : float.MinValue; //probably uneccesary

            return result;
        }
    }
}