using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CodeExplorinator
{
    public class ClassNode
    {
        public ClassData ClassData;
        public Vector2 F;
        /// <summary>
        /// this position is used to calculate the position of the node and taking into consideration the width and height of the box
        /// </summary>
        public Vector2 position;
        public List<ClassNode> ConnectedNodes;
        public List<ClassNode> NotConnectedNodes;
        public VisualElement VisualElement;
        
        /// <summary>
        /// shows if this node is considered a leaf node by the breadth search
        /// </summary>
        public bool IsLeaf;

        public ClassNode(ClassData classData, VisualElement visualElement, bool isLeaf = false)
        {
            ClassData = classData;
            VisualElement = visualElement;
            IsLeaf = isLeaf;
            ConnectedNodes = new List<ClassNode>();
            NotConnectedNodes = new List<ClassNode>();
            F = new Vector2();

            position = new Vector2(VisualElement.style.marginLeft.value.value + VisualElement.style.width.value.value * 0.5f,VisualElement.style.marginTop.value.value + VisualElement.style.height.value.value * 0.5f);
        }
        
    }
}

