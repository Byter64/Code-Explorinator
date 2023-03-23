using System;
using System.Collections.Generic;
using CodeExplorinator;
using UnityEngine;

namespace CodeExplorinator
{
    public class SpringEmbedderAlgorithm
    {
        public void Init()
        {
            BreadthSearch breadthSearch = new BreadthSearch();
            breadthSearch.Start();
            Calculate(breadthSearch.AnalysedClasses, GenerateNodes(breadthSearch.AnalysedClasses), 3, 3);
        }

        private List<Node> GenerateNodes(List<ClassData> classDatas)
        {
            List<Node> nodes = new List<Node>();
            foreach (var classData in classDatas)
            {
                nodes.Add(new Node(classData));
                //also generate & insert ui elements here i guess
            }

            return nodes;
        }

        private void Calculate(List<ClassData> classDatas, List<Node> nodes, double threshold, int iterations)
        {
            int t = 1;

            while (t < iterations && F(t).sqrMagnitude > threshold) //f MAX!!!
            {
                
                foreach (var node in nodes)
                {
                    Vector2 resultRepulsion = Vector2.zero;
                    Vector2 resultSpring = Vector2.zero;
                    DetermineConnectionBetweenNodes(node,nodes);
                    
                    foreach (var connectedNode in node.ConnectedNodes)
                    {
                        resultRepulsion += ForceRepulsion();
                    }

                    foreach (var notConnectedNode in node.NotConnectedNodes)
                    {
                        resultSpring += ForceSpring();
                    }

                    node.F = resultRepulsion + resultSpring;
                }

                foreach (var node in nodes)
                {
                    
                    node.position += new Vector2(coolingFactior(t) * node.F.x,coolingFactior(t) * node.F.y);
                }

                t++;
            }
        }

        private void DetermineConnectionBetweenNodes(Node analysedNode, List<Node> allNodes)
        {
            List<Node> notConnectedNodes = new List<Node>();


            foreach (var analysedNodeClassData in analysedNode.ClassData.AllConnectedClasses)
            {
                foreach (var node in allNodes)
                {
                    if (node.ClassData == analysedNodeClassData)
                    {
                        analysedNode.ConnectedNodes.Add(node);
                    }
                    else
                    {
                        analysedNode.NotConnectedNodes.Add(node);
                    }
                }
            }
            
        }


        private Vector2 F(int t)
        {
            return Vector2.zero;
        }

        private Vector2 ForceRepulsion()
        {
            return Vector2.zero;
        }

        private float coolingFactior(int t)
        {
            float someEstimatedInterationNumber = 200;
            return 200%t +1;
        }

        private Vector2 ForceSpring()
        {
            return Vector2.zero;
        }
    }
}