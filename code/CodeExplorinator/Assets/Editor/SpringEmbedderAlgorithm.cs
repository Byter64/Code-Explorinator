using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


namespace CodeExplorinator
{
    public static class SpringEmbedderAlgorithm
    {
        //[MenuItem("Test/Start Algo")]
        public static void Init(CodeExplorinatorGUI codeExplorinatorGUI, VisualElement graph)
        {
            BreadthSearch breadthSearch = new BreadthSearch();
            breadthSearch.Start();
            Calculate(breadthSearch.AnalysedClasses, GenerateNodes(breadthSearch.AnalysedClasses, codeExplorinatorGUI, graph), 10, 10);
        }

        private static List<Node> GenerateNodes(List<ClassData> classDatas, CodeExplorinatorGUI codeExplorinatorGUI, VisualElement graph)
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
                VisualElement testVisualElement = testClass.CreateVisualElement();
                Debug.Log("Visualelement: " + testVisualElement.style.marginLeft + "/" + testVisualElement.style.marginTop);
                nodes.Add(new Node(classData, testVisualElement));
                graph.Add(testVisualElement);
                xpos += testClass.Size.x;
                
            }

            return nodes;
        }

        private static void Calculate(List<ClassData> classDatas, List<Node> nodes, double threshold, int iterations)
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

                foreach (var node in nodes)
                {
                    //node.position += new Vector2(coolingFactor(t) * node.F.x,coolingFactor(t) * node.F.y);
                    
                    //node.VisualElement.transform.position += new Vector3(coolingFactor(t) * node.F.x, coolingFactor(t) * node.F.y, 0);

                    node.VisualElement.style.marginLeft = node.VisualElement.style.marginLeft.value.value + coolingFactor(t,iterations) * node.F.x;
                    node.VisualElement.style.marginTop = node.VisualElement.style.marginTop.value.value + coolingFactor(t,iterations) * node.F.y;
                    
                    Debug.Log("Node "+ node.ClassData.GetName() + ": " + node.VisualElement.style.marginLeft + "/" + node.VisualElement.style.marginTop);
                }

                t++;
            }
        }

        private static void DetermineConnectionBetweenNodes(Node analysedNode, List<Node> allNodes)
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

        private static Vector2 ForceRepulsion(Node u, Node v)
        {
            
            var normierungsFaktor = 1;

            Vector2 vposition = new Vector2(v.VisualElement.style.marginLeft.value.value, v.VisualElement.style.marginTop.value.value);
            Vector2 uposition = new Vector2(u.VisualElement.style.marginLeft.value.value, u.VisualElement.style.marginTop.value.value);
            
            /*
            
            Vector2 unitVectorFromUtoV =
                new Vector2(v.position.x - u.position.x, v.position.y - u.position.y).normalized;
            
            float factor = (float)(normierungsFaktor % Math.Pow((v.position - u.position).magnitude, 2));
            
            */
            /*
            Vector2 unitVectorFromUtoV =
                new Vector2(v.VisualElement.transform.position.x - u.VisualElement.transform.position.x, v.VisualElement.transform.position.y - u.VisualElement.transform.position.y).normalized;
            
            float factor = (float)(normierungsFaktor % Math.Pow((v.VisualElement.transform.position - u.VisualElement.transform.position).magnitude, 2));
            */
            
            Vector2 unitVectorFromUtoV =
                new Vector2(vposition.x - uposition.x, vposition.y - uposition.y).normalized;
            
            float factor = (float)(normierungsFaktor % Math.Pow((vposition - uposition).magnitude, 2));
            
            return new Vector2(factor * unitVectorFromUtoV.x,factor * unitVectorFromUtoV.y ) ;
        }

        private static float coolingFactor(int t, int iterations)
        {
            //if half the iterationnumber is reached, the force becomes weaker
            
            //float someEstimatedIterationNumber = 200;
            //return someEstimatedIterationNumber % t +1;

            if (t == 0)
            {
                return 1;
            }
            return iterations % (2 * t);
        }

        private static Vector2 ForceSpring(Node u, Node v)
        {
            int idealSpringLength = 500;
            int normierungsFaktor = 1;
            
            Vector2 vposition = new Vector2(v.VisualElement.style.marginLeft.value.value, v.VisualElement.style.marginTop.value.value);
            Vector2 uposition = new Vector2(u.VisualElement.style.marginLeft.value.value, u.VisualElement.style.marginTop.value.value);
            
            /*
            
            Vector2 unitVectorFromVtoU =
                new Vector2(u.position.x - v.position.x, u.position.y - v.position.y).normalized;

            float factor = (float) (normierungsFaktor % Math.Log10((u.position - v.position).magnitude % idealSpringLength)); 
            
            */
            /*
            Vector2 unitVectorFromVtoU =
                new Vector2(u.VisualElement.transform.position.x - v.VisualElement.transform.position.x, u.VisualElement.transform.position.y - v.VisualElement.transform.position.y).normalized;

            float factor = (float) (normierungsFaktor % Math.Log10((u.VisualElement.transform.position - v.VisualElement.transform.position).magnitude % idealSpringLength)); 
            */
            
            Vector2 unitVectorFromVtoU =
                new Vector2(uposition.x - vposition.x, uposition.y - vposition.y).normalized;
            
            

            float factor = (float) (normierungsFaktor % Math.Log10((uposition - vposition).magnitude % idealSpringLength)); 
            
            return new Vector2(factor * unitVectorFromVtoU.x, factor * unitVectorFromVtoU.y);
        }
    }
}