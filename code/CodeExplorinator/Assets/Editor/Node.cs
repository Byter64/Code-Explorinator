using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CodeExplorinator
{
    public class Node
    {
        //the node should hold not only all connected and not connected classes, but also in which direction? wait we have the classdata and isLeaf bool for that tho?

        public ClassData ClassData;
        public Vector2 F;
        /// <summary>
        /// this position is used to calculate the position of the node and taking into consideration the width and height of the box
        /// </summary>
        public Vector2 position;
        public List<Node> ConnectedNodes;
        public List<Node> NotConnectedNodes;
        public VisualElement VisualElement;
        
        /// <summary>
        /// shows if this node is considered a leaf node by the breadth search
        /// </summary>
        public bool IsLeaf;

        public Node(ClassData classData, VisualElement visualElement, bool isLeaf = false)
        {
            ClassData = classData;
            VisualElement = visualElement;
            IsLeaf = isLeaf;
            ConnectedNodes = new List<Node>();
            NotConnectedNodes = new List<Node>();
            F = new Vector2();

            position = new Vector2(VisualElement.style.marginLeft.value.value + VisualElement.style.width.value.value * 0.5f,VisualElement.style.marginTop.value.value + VisualElement.style.height.value.value * 0.5f);
        }
        
    }
}

