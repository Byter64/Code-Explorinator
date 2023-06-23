using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;


namespace CodeExplorinator
{
    public static class SpringEmbedderAlgorithm
    {
        /// <summary>
        /// The force threshold of the class spring algorithm that stops it if the forces fall under its value.
        /// Threshold 0.01 is too big, maybe 0.001 too small.
        /// </summary>
        private const double thresholdOfClassAlgo = 0.00001;
        /// <summary>
        /// The iterations that the class spring algorithm is supposed to do.
        /// </summary>
        private const int iterationsOfClassAlgo = 10000;
        
        /// <summary>
        /// The force threshold of the method spring algorithm that stops it if the forces fall under its value.
        /// </summary>
        private const double thresholdOfMethodAlgo = 0.01;
        /// <summary>
        /// The iterations that the method spring algorithm is supposed to do.
        /// </summary>
        private const int iterationsOfMethodAlgo = 1000;
        
        /// <summary>
        /// The bigger the number, the stronger the repulsion force between two not connected nodes.
        /// 4 delivers okay results
        /// </summary>
        private const float repulsionConstant = 4000f;
        /// <summary>
        /// The bigger the number, the stronger the attraction force between two connected nodes.
        /// 2 delivers okay results
        /// </summary>
        private const float attractionConstant = 2000f;
        
        /// <summary>
        /// CANNOT BE ZERO!
        /// This is the ideal length of a connection between nodes. The connections that go to nowhere are 500 long, the smallest class has a height of ca. 500.
        /// </summary>
        private const float idealSpringLength = 1500f;
        
        /// <summary>
        /// CANNOT BE ZERO!
        /// The bigger the number, the faster the force gets smaller with increased iterations, for example:
        /// iterations = 100, coolingsSpeed = 2; 100/2*current_iteration
        /// => from iteration 50 and onwards the coolingSpeed is <= 1
        /// </summary>
        private const float coolingSpeed = 2f;


        //this method calculates the position of the node, it still doesnt check if the node is placed out of bounds
        public static void StartAlgorithm(List<ClassNode> nodes)
        {
            int t = 1;

            DetermineConnectionBetweenNodes(nodes);


            while (t <= iterationsOfClassAlgo)
            {
                foreach (var node in nodes)
                {
                    Vector2 resultRepulsion = Vector2.zero;
                    Vector2 resultSpring = Vector2.zero;

                    foreach (var connectedNode in node.ConnectedNodes)
                    {
                        //the force of attraction
                        resultSpring += ForceSpring(connectedNode, node);
                    }

                    foreach (var notConnectedNode in node.NotConnectedNodes)
                    {
                        //the force of repulsion
                        resultRepulsion += ForceRepulsion(notConnectedNode, node);
                    }

                    node.F = (resultRepulsion + resultSpring);
                }

                float maxForce = 0;

                foreach (var node in
                         nodes) //checking if the forces are small, aka if the graph has stabilized, to end the calculations
                {
                    if (maxForce < node.F.magnitude)
                    {
                        maxForce = node.F.magnitude;
                    }
                }

                if (maxForce < thresholdOfClassAlgo)
                {
                    Debug.Log("we stopped the spring algo at " + t + " iterations");
                    return;
                }


                float cooling = coolingFactor(t, iterationsOfClassAlgo);

                /*
                //this is ignoring the first element, aka the focus class, to force it to stay in the same place
                for (int i = 1; i < nodes.Count; i++)
                {
                    nodes[i].position.x += cooling * nodes[i].F.x;
                    nodes[i].position.y += cooling * nodes[i].F.y;
                }
                */
                foreach (var node in nodes)
                {
                    
                    node.position.x += cooling * node.F.x;
                    node.position.y += cooling * node.F.y;
                    
                    //Debug.Log("Node "+ node.ClassData.GetName() + ": " + node.position.x + "/" + node.position.y);
                }

                t++;
            }

            //set the position for the visual elements
            //as we incorporated the height and width into the Node.position Vector2, we now have to undo this:
            foreach (var node in nodes)
            {
                //node.classGUI.VisualElement.style.marginLeft = node.position.x - node.classGUI.VisualElement.style.width.value.value * 0.5f;
                //node.classGUI.VisualElement.style.marginTop = node.position.y - node.classGUI.VisualElement.style.height.value.value * 0.5f;
                
                node.classGUI.VisualElement.style.marginLeft = node.position.x;
                node.classGUI.VisualElement.style.marginTop = node.position.y;
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

        public static void StartMethodAlgorithm(HashSet<ClassNode> nodes, HashSet<MethodNode> allMethods)
        {
            int t = 1;

            DetermineConnectionsBetweenMethodNodes(allMethods);

            while (t <= iterationsOfMethodAlgo)
            {
                foreach (var node in nodes)
                {
                    Vector2 resultRepulsion = Vector2.zero;
                    Vector2 resultSpring = Vector2.zero;

                    foreach (var methodNode in node.MethodNodes)
                    {
                        foreach (var connectedNode in methodNode.ConnectedNodes)
                        {
                            resultSpring += ForceSpring(connectedNode.MethodData.ContainingClass.ClassNode, node);
                        }
                    }

                    foreach (var methodNode in node.MethodNodes)
                    {
                        foreach (var notConnectedNode in methodNode.NotConnectedNodes)
                        {
                            resultRepulsion += ForceRepulsion(notConnectedNode.MethodData.ContainingClass.ClassNode,node);
                        }
                    }

                    node.F = resultRepulsion + resultSpring;
                }

                float maxForce = 0;

                foreach (var node in
                         nodes) //checking if the forces are small, aka if the graph has stabilized, to end the calculations
                {
                    if (maxForce < node.F.magnitude)
                    {
                        maxForce = node.F.magnitude;
                    }
                }

                if (maxForce < thresholdOfMethodAlgo)
                {
                    Debug.Log("we stopped the spring algo at " + t + " iterations");
                    return;
                }


                float cooling = coolingFactor(t, iterationsOfMethodAlgo);

                foreach (var node in nodes)
                {
                    node.position.x += cooling * node.F.x;
                    node.position.y += cooling * node.F.y;

                    //Debug.Log("Node " + node.ClassData.GetName() + ": " + node.position.x + "/" +
                    //          node.position.y);
                }

                t++;
            }

            //as we incorporated the height and width into the Node.position Vector2, we now have to undo this
            foreach (var node in nodes)
            {
                node.classGUI.VisualElement.style.marginLeft = node.position.x - node.classGUI.VisualElement.style.width.value.value * 0.5f;
                node.classGUI.VisualElement.style.marginTop = node.position.y - node.classGUI.VisualElement.style.height.value.value * 0.5f;
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
        private static Vector2 ForceRepulsion(ClassNode u, ClassNode v)
        {
            Vector2 direction = (v.position - u.position);

            //factor = repulsionContant / (direction.magnitude�)
            float factor = direction.magnitude * direction.magnitude;
            factor = (factor == 0) ? float.Epsilon : factor;

            //if (float.IsInfinity(factor)) factor = float.IsPositiveInfinity(factor) ? float.MaxValue : float.MinValue; //unnecessary
            factor = repulsionConstant / factor;
            //if (float.IsInfinity(factor)) factor = float.IsPositiveInfinity(factor) ? float.MaxValue : float.MinValue; //probably unnecessary

            direction.Normalize();
            Vector2 result = factor * direction;
            //if (float.IsInfinity(result.x)) result.x = float.IsPositiveInfinity(result.x) ? float.MaxValue : float.MinValue; //probably unnecessary
            //if (float.IsInfinity(result.y)) result.y = float.IsPositiveInfinity(result.y) ? float.MaxValue : float.MinValue; //probably unnecessary

            return result;
        }

        private static float coolingFactor(int t, int iterations)
        {
            //if half the iterationnumber is reached, the force becomes weaker

            if (t == 0) return 1;
            //if (coolingSpeed == 0) return 1;
            
            //return iterations / (coolingSpeed * t);

            float factor = coolingSpeed * 5;
                
            if (t >= iterations - iterations / coolingSpeed)
            {
                factor = 1;
            }

            return iterations * factor / (coolingSpeed * t);
        }
        
        

        /// <summary>
        /// Returns the attraction of v that is generated by u
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <param name="attractionConstant"></param>
        /// <param name="idealSpringLength"></param>
        /// <returns></returns>
        private static Vector2 ForceSpring(ClassNode u, ClassNode v) //500 is ca. the height of the standard class
        {
            //if (idealSpringLength == 0) idealSpringLength = float.Epsilon;

            Vector2 direction = u.position - v.position;

            //factor = attractionConstant * log10(direction.magnitude / idealSpringLength)
            float factor = direction.magnitude / idealSpringLength;
            factor = (factor == 0) ? float.Epsilon : factor;
            //if (float.IsInfinity(factor)) factor = float.IsPositiveInfinity(factor) ? float.MaxValue : float.MinValue; //probably uneccesary

            factor = Mathf.Log10(factor);

            if (factor == 0) factor = float.Epsilon; //probably uneccesary 
            //if (float.IsInfinity(factor)) factor = float.IsPositiveInfinity(factor) ? float.MaxValue : float.MinValue; //probably uneccesary

            factor = attractionConstant * factor;
            //if (float.IsInfinity(factor)) factor = float.IsPositiveInfinity(factor) ? float.MaxValue : float.MinValue; //probably uneccesary

            direction.Normalize();
            Vector2 result = factor * direction;
            //if (float.IsInfinity(result.x)) result.x = float.IsPositiveInfinity(result.x) ? float.MaxValue : float.MinValue; //probably uneccesary
            //if (float.IsInfinity(result.y)) result.y = float.IsPositiveInfinity(result.y) ? float.MaxValue : float.MinValue; //probably uneccesary

            return result;
        }
    }
}