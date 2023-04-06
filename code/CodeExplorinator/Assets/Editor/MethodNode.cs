using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CodeExplorinator
{
    public class MethodNode
    {
        public MethodData MethodData;
        public Vector2 F;
        /// <summary>
        /// this position is used to calculate the position of the node and taking into consideration the width and height of the box
        /// </summary>
        public Vector2 position;
        public List<MethodNode> ConnectedNodes;
        public List<MethodNode> NotConnectedNodes;
        public VisualElement VisualElement;
        
        /// <summary>
        /// shows if this node is considered a leaf node by the breadth search
        /// </summary>
        public bool IsLeaf;

        public MethodNode(MethodData methodData, VisualElement visualElement, bool isLeaf = false)
        {
            MethodData = methodData;
            VisualElement = visualElement;
            IsLeaf = isLeaf;
            ConnectedNodes = new List<MethodNode>();
            NotConnectedNodes = new List<MethodNode>();
            F = new Vector2();

            
            //i should probably change the position of 
            position = new Vector2(VisualElement.style.marginLeft.value.value + VisualElement.style.width.value.value * 0.5f,VisualElement.style.marginTop.value.value + VisualElement.style.height.value.value * 0.5f);
        }
        
    }
}