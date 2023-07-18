using System.Collections.Generic;

namespace CodeExplorinator
{
    public class MethodNode
    {
        public MethodData MethodData;
        
        //these are for determining the connections between methods for the spring algo
        public HashSet<MethodNode> ConnectedNodes;
        public HashSet<MethodNode> NotConnectedNodes;

        public List<MethodNode> ingoingConnections;
        public List<MethodNode> outgoingConnections;

        public MethodGUI MethodGUI { get; set; }

        /// <summary>
        /// represents the distance of the focus method found by the breadth search 
        /// </summary>
        public int distanceFromFocusMethod;

        /// <summary>
        /// shows if this node is considered a leaf node by the breadth search
        /// </summary>
        public bool IsLeaf;

        public MethodNode(MethodData methodData,MethodGUI methodGUI, bool isLeaf = false)
        {
            MethodData = methodData;
            MethodGUI = methodGUI;
            IsLeaf = isLeaf;
            ConnectedNodes = new HashSet<MethodNode>();
            NotConnectedNodes = new HashSet<MethodNode>();
            ingoingConnections = new List<MethodNode>();
            outgoingConnections= new List<MethodNode>();
        }

        public static void CopyRerefencesFromMethodData(IEnumerable<MethodNode> nodes)
        {
            foreach (MethodNode node in nodes)
            {
                node.ingoingConnections.Clear();
                foreach (MethodInvocationData methodInvocation in node.MethodData.InvokedByExternal)
                {
                    node.ingoingConnections.Add(methodInvocation.ReferencedMethod.MethodNode);
                }

                node.outgoingConnections.Clear();
                foreach (MethodInvocationData methodInvocation in node.MethodData.IsInvokingExternalMethods)
                {
                    node.outgoingConnections.Add(methodInvocation.ReferencedMethod.MethodNode);
                }
            }
        }

    }
}