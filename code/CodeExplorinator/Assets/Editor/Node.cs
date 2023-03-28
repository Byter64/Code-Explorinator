using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CodeExplorinator
{
    public class Node
    {
        public ClassData ClassData;
        public Vector2 F;
        public List<Node> ConnectedNodes;
        public List<Node> NotConnectedNodes;
        public VisualElement VisualElement;
        public Vector2 position;

        public Node(ClassData classData, VisualElement visualElement)
        {
            ClassData = classData;
            VisualElement = visualElement;
            ConnectedNodes = new List<Node>();
            NotConnectedNodes = new List<Node>();
            F = new Vector2();
        }
        
    }
}

