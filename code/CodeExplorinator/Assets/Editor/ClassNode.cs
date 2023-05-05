using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Graphs;
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
        
        public List<ClassNode> ingoingConnections;
        public List<ClassNode> outgoingConnections;
        
        //these are for determining the connections between classes for the spring algo
        public HashSet<ClassNode> ConnectedNodes;
        public HashSet<ClassNode> NotConnectedNodes;
        
        public List<MethodNode> MethodNodes;

        public ClassGUI classGUI { get; private set; }
        
        /// <summary>
        /// shows if this node is considered a leaf node by the breadth search
        /// </summary>
        public bool IsLeaf { get; set; }

        public ClassNode(ClassData classData, ClassGUI classGUI, bool isLeaf = false)
        {
            ClassData = classData;
            classGUI.GenerateVisualElement();
            this.classGUI = classGUI;
            IsLeaf = isLeaf;
            ConnectedNodes = new HashSet<ClassNode>();
            NotConnectedNodes = new HashSet<ClassNode>();
            ingoingConnections = new List<ClassNode>();
            outgoingConnections = new List<ClassNode>();
            MethodNodes = new List<MethodNode>();
            F = new Vector2();

            position = new Vector2(this.classGUI.VisualElement.style.marginLeft.value.value + this.classGUI.VisualElement.style.width.value.value * 0.5f, this.classGUI.VisualElement.style.marginTop.value.value + this.classGUI.VisualElement.style.height.value.value * 0.5f);
        }
        
        public static void CopyRerefencesFromClassData(IEnumerable<ClassNode> nodes)
        {
            foreach (ClassNode node in nodes)
            {
                node.ingoingConnections.Clear();
                foreach(ClassFieldReferenceData fieldAccess in node.ClassData.ReferencedByExternalClassField)
                {
                    node.ingoingConnections.Add(fieldAccess.FieldContainingReference.ContainingClass.ClassNode);
                }
                foreach (ClassPropertyReferenceData propertyAccess in node.ClassData.ReferencedByExternalClassProperty)
                {
                    node.ingoingConnections.Add(propertyAccess.PropertyContainingReference.ContainingClass.ClassNode);
                }

                node.outgoingConnections.Clear();
                foreach (ClassFieldReferenceData fieldAccess in node.ClassData.IsReferencingExternalClassAsField)
                {
                    node.outgoingConnections.Add(fieldAccess.ReferencedClass.ClassNode);
                }
                foreach (ClassPropertyReferenceData propertyAccess in node.ClassData.IsReferencingExternalClassAsProperty)
                {
                    node.outgoingConnections.Add(propertyAccess.ReferencedClass.ClassNode);
                }
                
            }
        }

    }
}

