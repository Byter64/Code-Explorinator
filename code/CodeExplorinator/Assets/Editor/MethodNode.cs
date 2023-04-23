using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CodeExplorinator
{
    public class MethodNode
    {
        public MethodData MethodData;
        
        //these are for determining the connections between methods for the spring algo
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
        }
        
    }
}