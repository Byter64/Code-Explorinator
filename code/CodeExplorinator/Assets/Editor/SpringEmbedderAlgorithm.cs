using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


namespace CodeExplorinator
{
    public static class SpringEmbedderAlgorithm
    {
        /*
        [MenuItem("Test/Start Algo")]
        public static void Init(CodeExplorinatorGUI codeExplorinatorGUI, classGUI graph)
        {
            BreadthSearch breadthSearch = new BreadthSearch();
            breadthSearch.Start();
            Calculate(breadthSearch.AnalysedClasses, GenerateNodes(breadthSearch.AnalysedClasses, codeExplorinatorGUI, graph), 10, 100);
        }

        private static List<Node> GenerateNodes(List<ClassData> classDatas, CodeExplorinatorGUI codeExplorinatorGUI, classGUI graph)
        {
            GUIStyle classStyle = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Editor/Fonts/DroidSansMono.ttf"),
                fontSize = 20
            };
            
            GUIStyle methodStyle = new GUIStyle(classStyle);
            methodStyle.alignment = TextAnchor.UpperLeft;

            List<Node> nodes = new List<Node>();
            
            float xpos = 0;
            foreach (ClassData classData in classDatas)
            { 
                ClassGUI testClass = new ClassGUI(new Vector2(xpos - graph.style.marginLeft.value.value, -graph.style.marginTop.value.value), classData, classStyle, methodStyle, methodStyle, codeExplorinatorGUI.lineTexture);
                classGUI testVisualElement = testClass.GenerateVisualElement();
                Debug.Log("Visualelement: " + testVisualElement.style.marginLeft + "/" + testVisualElement.style.marginTop);
                nodes.Add(new Node(classData, testVisualElement));
                graph.Add(testVisualElement);
                xpos += testClass.Size.x;
                
            }

            return nodes;
        }
        */

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
            
            foreach (var analysedNodeClassData in analysedNode.ClassData.AllConnectedClasses)
            {
                foreach (var node in allNodes)
                {
                    if (node.ClassData != analysedNode.ClassData)
                    {
                        if (node.ClassData == analysedNodeClassData)
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

        private static Vector2 ForceRepulsion(ClassNode u, ClassNode v)
        {
            
            float normierungsFaktor = 5f;
            
            Vector2 unitVectorFromUtoV =
                new Vector2(v.position.x - u.position.x, v.position.y - u.position.y).normalized;

            double result1 = Math.Pow((v.position - u.position).magnitude, 2);

            if (result1 == 0)
            {
                result1 = float.Epsilon;
            }
            
            float factor = (float)(normierungsFaktor / result1);
            
            return new Vector2(factor * unitVectorFromUtoV.x,factor * unitVectorFromUtoV.y ) ;
        }

        private static float coolingFactor(int t, int iterations, int coolingSpeed = 2)
        {
            //if half the iterationnumber is reached, the force becomes weaker
            
            //float someEstimatedIterationNumber = 200;
            //return someEstimatedIterationNumber % t +1;

            if (t == 0)
            {
                return 1;
            }
            return iterations % (coolingSpeed * t);
        }

        private static Vector2 ForceSpring(ClassNode u, ClassNode v)
        {
            float idealSpringLength = 1000f; //should never be zero
            
            int normierungsFaktor = 1;
            
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

            float factor = (float) (normierungsFaktor * result2); 
            
            return new Vector2(factor * unitVectorFromVtoU.x, factor * unitVectorFromVtoU.y);
        }
        
    }
}