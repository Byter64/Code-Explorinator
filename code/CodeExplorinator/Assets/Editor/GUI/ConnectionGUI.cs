

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace CodeExplorinator
{
    public class ConnectionGUI
    {
        public VisualElement VisualElement { get; private set; }

        private Texture2D lineTexture;
        private Texture2D arrowTexture;
        private ClassNode footNode;
        private ClassNode tipNode;

        public ConnectionGUI(ClassNode footNode, ClassNode tipNode)
        {
            this.footNode = footNode;
            this.tipNode = tipNode;
            lineTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Graphics/Linetexture.png");
            arrowTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Graphics/pfeil_centeredt.png");
        }

        //██ This method is not updated. A similar method that replaces this one is in Graphmanager
        //public void DrawConnections()
        //{
        //    foreach (var analysedNode in Nodes)
        //    {
        //        //if there are many variables referencing the same class, the line is drawn multiple times, which is BAD!!!!
                
        //        if (!analysedNode.IsLeaf)
        //        {
        //            foreach (var fieldReference in analysedNode.ClassData.IsReferencingExternalClassAsField)
        //            {
        //                CreateVisualElementArrow(analysedNode,fieldReference.ReferencedClass.ClassNode);
        //            }

        //            foreach (var propertyReference in analysedNode.ClassData.IsReferencingExternalClassAsProperty)
        //            {
        //                CreateVisualElementArrow(analysedNode,propertyReference.ReferencedClass.ClassNode);
        //            }
        //        }
        //        else
        //        {
        //            //draw arrows to classes which exist in the window and draw them to a random location if they dont 
                    
        //            foreach (var fieldReference in analysedNode.ClassData.IsReferencingExternalClassAsField)
        //            {
        //                bool isClassFound = false;
                        
        //                foreach (var node in Nodes)
        //                {
        //                    if (fieldReference.ReferencedClass == node.ClassData)
        //                    {
        //                        CreateVisualElementArrow(analysedNode,node);
        //                        isClassFound = true;
        //                        break;
        //                    }
        //                }
        //                //if the class is not shown, we still want to draw an arrow, just to nowhere in particular
        //                if (!isClassFound)
        //                {
        //                    CreateVisualElementArrowToRandom(analysedNode,false);
        //                }
        //            }

        //            foreach (var propertyReference in analysedNode.ClassData.IsReferencingExternalClassAsProperty)
        //            {
        //                bool isClassFound = false;
                        
        //                foreach (var node in Nodes)
        //                {
        //                    if (propertyReference.ReferencedClass == node.ClassData)
        //                    {
        //                        CreateVisualElementArrow(analysedNode,node);
        //                        isClassFound = true;
        //                        break;
        //                    }
        //                }
                        
        //                //if the class is not shown, we still want to draw an arrow, just to nowhere in particular
        //                if (!isClassFound)
        //                {
        //                    CreateVisualElementArrowToRandom(analysedNode,false);
        //                }
        //            }

        //            foreach (var fieldReference in analysedNode.ClassData.ReferencedByExternalClassField)
        //            {
        //                bool classExsists = false;
        //                foreach (var node in Nodes)
        //                {
        //                    if (fieldReference.FieldContainingReference.ContainingClass == node.ClassData)
        //                    {
        //                        classExsists = true;
        //                        break;
        //                    }
        //                }

        //                if (!classExsists)
        //                {
        //                    CreateVisualElementArrowToRandom(analysedNode,true);
        //                }
        //            }

        //            foreach (var propertyReference in analysedNode.ClassData.ReferencedByExternalClassProperty)
        //            {
        //                bool classExsists = false;
        //                foreach (var node in Nodes)
        //                {
        //                    if (propertyReference.PropertyContainingReference.ContainingClass == node.ClassData)
        //                    {
        //                        classExsists = true;
        //                        break;
        //                    }
        //                }
                        
        //                if (!classExsists)
        //                {
        //                    CreateVisualElementArrowToRandom(analysedNode,true);
        //                }
        //            }
                    
        //        }
                
        //    }
        //}

        public void GenerateVisualElement()
        {
            if(footNode == null)
            {
                VisualElement = CreateVisualElementArrowToRandom(tipNode, true);
            }
            else if (tipNode == null)
            {
                VisualElement = CreateVisualElementArrowToRandom(footNode, false);
            }
            else
            {
                VisualElement = CreateVisualElementArrow(footNode, tipNode);
            }
        }

        //██I Didn't update this method because it is unused.
        //If this method wants to be used again one should refactor it into a form like the other methods
        //private void DrawLine(ClassNode node, ClassNode connectedNode)
        //{
        //    //create vectors u and v that hold the position of the node anchored centered on top
        //    Vector2 u = new Vector2(node.classGUI.style.marginLeft.value.value + node.classGUI.style.width.value.value * 0.5f,
        //        node.classGUI.style.marginTop.value.value);
        //    Vector2 v = new Vector2(connectedNode.classGUI.style.marginLeft.value.value + connectedNode.classGUI.style.width.value.value * 0.5f,
        //        connectedNode.classGUI.style.marginTop.value.value);

        //    //creates the vector from node to node
        //    Vector2 connection = new Vector2(u.x - v.x, u.y - v.y);

        //    //instantiates the line
        //    VisualElement line = new VisualElement();
        //    line.style.backgroundImage = new StyleBackground(lineTexture);
        //    //sets the rotationpoint of the line to the position u (the rotationpoint of the line lies in the middle) the line is not rotated yet, thats why we use the magnitude
        //    line.style.marginLeft = new StyleLength(u.x - connection.magnitude * 0.5f);
        //    line.style.marginTop = new StyleLength(u.y);
        //    line.style.position = new StyleEnum<Position>(UnityEngine.UIElements.Position.Absolute); 
        //    line.style.height = lineTexture.height;
        //    //the width of the line is the magnitude of the vector, as the line is not rotated yet
        //    line.style.width = connection.magnitude;

        //    //rotates the line to be parallel to the vector
        //    line.style.rotate = new StyleRotate( new Rotate(new Angle(Mathf.Atan2(connection.y, connection.x)*Mathf.Rad2Deg)));

        //    //repositions the line half of the vector further to complete the process
        //    line.style.marginLeft = new StyleLength(line.style.marginLeft.value.value - connection.x * 0.5f);
        //    line.style.marginTop = new StyleLength(line.style.marginTop.value.value - connection.y * 0.5f);

        //    Graph.Add(line);
        //}

        private VisualElement CreateVisualElementArrow(ClassNode footNode, ClassNode tipNode)
        {
            //creates parent visual element
            VisualElement parent = new VisualElement();

            //create vectors u and v that hold the position of the node anchored centered on top
            Vector2 u = new Vector2(footNode.classGUI.VisualElement.style.marginLeft.value.value + footNode.classGUI.VisualElement.style.width.value.value * 0.5f,
                footNode.classGUI.VisualElement.style.marginTop.value.value);
            Vector2 v = new Vector2(tipNode.classGUI.VisualElement.style.marginLeft.value.value + tipNode.classGUI.VisualElement.style.width.value.value * 0.5f,
                tipNode.classGUI.VisualElement.style.marginTop.value.value);
            
            //creates the vector from node to node
            Vector2 connection = new Vector2(v.x - u.x, v.y - u.y);
            
            //instantiates the line
            VisualElement line = new VisualElement();
            line.style.backgroundImage = new StyleBackground(lineTexture);
            //sets the rotationpoint of the line to the position u (the rotationpoint of the line lies in the middle) the line is not rotated yet, thats why we use the magnitude
            line.style.marginLeft = new StyleLength(u.x - connection.magnitude * 0.5f);
            line.style.marginTop = new StyleLength(u.y);
            line.style.position = new StyleEnum<Position>(UnityEngine.UIElements.Position.Absolute); 
            line.style.height = lineTexture.height;
            //the width of the line is the magnitude of the vector, as the line is not rotated yet
            line.style.width = connection.magnitude;

            //rotates the line to be parallel to the vector
            line.style.rotate = new StyleRotate( new Rotate(new Angle(Mathf.Atan2(connection.y, connection.x)*Mathf.Rad2Deg)));
            
            //repositions the line half of the vector further to complete the process
            line.style.marginLeft = new StyleLength(line.style.marginLeft.value.value + connection.x * 0.5f);
            line.style.marginTop = new StyleLength(line.style.marginTop.value.value + connection.y * 0.5f);
            
            //draws the arrow
            VisualElement arrow = new VisualElement();
            arrow.style.backgroundImage = new StyleBackground(arrowTexture);
            arrow.style.marginLeft = new StyleLength(v.x - arrowTexture.width * 0.5f);
            arrow.style.marginTop = new StyleLength(v.y - arrowTexture.height * 0.5f);
            arrow.style.position = new StyleEnum<Position>(UnityEngine.UIElements.Position.Absolute); 
            arrow.style.height = arrowTexture.height;
            arrow.style.width = arrowTexture.height;
            
            arrow.style.rotate = new StyleRotate( new Rotate(new Angle(Mathf.Atan2(connection.y, connection.x)*Mathf.Rad2Deg)));

            parent.Add(line);
            parent.Add(arrow);

            return parent;
        }
        
        private VisualElement CreateVisualElementArrowToRandom(ClassNode node, bool isIncomingArrow)
        {
            //creates parent visual element
            VisualElement parent = new VisualElement();

            //create vectors u and v that hold the position of the node anchored centered on top
            Vector2 v = new Vector2(node.classGUI.VisualElement.style.marginLeft.value.value + node.classGUI.VisualElement.style.width.value.value * 0.5f,
                node.classGUI.VisualElement.style.marginTop.value.value);;
            

            //creates a random vector from node to node
            Vector2 connection = new Vector2(Random.Range(-1000,1000), Random.Range(-1000,1000));
            
            //instantiates the line
            VisualElement line = new VisualElement();
            line.style.backgroundImage = new StyleBackground(lineTexture);
            //sets the rotationpoint of the line to the position u (the rotationpoint of the line lies in the middle) the line is not rotated yet, thats why we use the magnitude
            line.style.marginLeft = new StyleLength(v.x - connection.magnitude * 0.5f);
            line.style.marginTop = new StyleLength(v.y);
            line.style.position = new StyleEnum<Position>(UnityEngine.UIElements.Position.Absolute); 
            line.style.height = lineTexture.height;
            //the width of the line is the magnitude of the vector, as the line is not rotated yet
            line.style.width = connection.magnitude;

            //rotates the line to be parallel to the vector
            line.style.rotate = new StyleRotate( new Rotate(new Angle(Mathf.Atan2(connection.y, connection.x)*Mathf.Rad2Deg)));
            
            //repositions the line half of the vector further to complete the process
            line.style.marginLeft = new StyleLength(line.style.marginLeft.value.value + connection.x * 0.5f);
            line.style.marginTop = new StyleLength(line.style.marginTop.value.value + connection.y * 0.5f);

            parent.Add(line);
            
            if (isIncomingArrow)
            {
                //draws the arrow
                VisualElement arrow = new VisualElement();
                arrow.style.backgroundImage = new StyleBackground(arrowTexture);
                //centering v in the arrow image
                arrow.style.marginLeft = new StyleLength(v.x - arrowTexture.width * 0.5f);
                arrow.style.marginTop = new StyleLength(v.y - arrowTexture.height * 0.5f);
                arrow.style.position = new StyleEnum<Position>(UnityEngine.UIElements.Position.Absolute); 
                arrow.style.height = arrowTexture.height;
                arrow.style.width = arrowTexture.height;
            
                arrow.style.rotate = new StyleRotate( new Rotate(new Angle(Mathf.Atan2(-connection.y, -connection.x)*Mathf.Rad2Deg)));

                parent.Add(arrow);
            }

            return parent;
        }
        
    }
}

