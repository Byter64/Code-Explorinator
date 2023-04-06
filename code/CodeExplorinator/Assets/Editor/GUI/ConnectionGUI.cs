

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace CodeExplorinator
{
    public class ConnectionGUI
    {
        private Texture2D lineTexture;
        private Texture2D arrowTexture;
        private List<ClassNode> Nodes;
        private VisualElement Graph;

        public ConnectionGUI(List<ClassNode> nodes, VisualElement graph, Texture2D line)
        {
            Nodes = nodes;
            Graph = graph;
            lineTexture = line;
            
            //i probably need to import the texture another way as this causes graphic glitches
            byte[] bytes = File.ReadAllBytes(Application.dataPath + "/Editor/Graphics/pfeil_centeredt.png");
 
            arrowTexture = new Texture2D(1, 1);
            arrowTexture.LoadImage(bytes);

        }

        public void DrawConnections()
        {
            foreach (var analysedNode in Nodes)
            {
                //if there are many variables referencing the same class, the line is drawn multiple times, which is BAD!!!!
                
                if (!analysedNode.IsLeaf)
                {
                    foreach (var fieldReference in analysedNode.ClassData.IsReferencingExternalClassAsField)
                    {
                        DrawArrow(analysedNode,fieldReference.ReferencedClass.ClassNode);
                    }

                    foreach (var propertyReference in analysedNode.ClassData.IsReferencingExternalClassAsProperty)
                    {
                        DrawArrow(analysedNode,propertyReference.ReferencedClass.ClassNode);
                    }
                }
                else
                {
                    //draw arrows to classes which exist in the window and draw them to a random location if they dont 
                    
                    foreach (var fieldReference in analysedNode.ClassData.IsReferencingExternalClassAsField)
                    {
                        bool isClassFound = false;
                        
                        foreach (var node in Nodes)
                        {
                            if (fieldReference.ReferencedClass == node.ClassData)
                            {
                                DrawArrow(analysedNode,node);
                                isClassFound = true;
                                break;
                            }
                        }
                        //if the class is not shown, we still want to draw an arrow, just to nowhere in particular
                        if (!isClassFound)
                        {
                            DrawArrowToSomewhere(analysedNode,false);
                        }
                    }

                    foreach (var propertyReference in analysedNode.ClassData.IsReferencingExternalClassAsProperty)
                    {
                        bool isClassFound = false;
                        
                        foreach (var node in Nodes)
                        {
                            if (propertyReference.ReferencedClass == node.ClassData)
                            {
                                DrawArrow(analysedNode,node);
                                isClassFound = true;
                                break;
                            }
                        }
                        
                        //if the class is not shown, we still want to draw an arrow, just to nowhere in particular
                        if (!isClassFound)
                        {
                            DrawArrowToSomewhere(analysedNode,false);
                        }
                    }

                    foreach (var fieldReference in analysedNode.ClassData.ReferencedByExternalClassField)
                    {
                        bool classExsists = false;
                        foreach (var node in Nodes)
                        {
                            if (fieldReference.FieldContainingReference.ContainingClass == node.ClassData)
                            {
                                classExsists = true;
                                break;
                            }
                        }

                        if (!classExsists)
                        {
                            DrawArrowToSomewhere(analysedNode,true);
                        }
                    }

                    foreach (var propertyReference in analysedNode.ClassData.ReferencedByExternalClassProperty)
                    {
                        bool classExsists = false;
                        foreach (var node in Nodes)
                        {
                            if (propertyReference.PropertyContainingReference.ContainingClass == node.ClassData)
                            {
                                classExsists = true;
                                break;
                            }
                        }
                        
                        if (!classExsists)
                        {
                            DrawArrowToSomewhere(analysedNode,true);
                        }
                    }
                    
                }
                
            }
        }

        private void DrawLine(ClassNode node, ClassNode connectedNode)
        {
            //create vectors u and v that hold the position of the node anchored centered on top
            Vector2 u = new Vector2(node.VisualElement.style.marginLeft.value.value + node.VisualElement.style.width.value.value * 0.5f,
                node.VisualElement.style.marginTop.value.value);
            Vector2 v = new Vector2(connectedNode.VisualElement.style.marginLeft.value.value + connectedNode.VisualElement.style.width.value.value * 0.5f,
                connectedNode.VisualElement.style.marginTop.value.value);
            
            //creates the vector from node to node
            Vector2 connection = new Vector2(u.x - v.x, u.y - v.y);
            
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
            line.style.marginLeft = new StyleLength(line.style.marginLeft.value.value - connection.x * 0.5f);
            line.style.marginTop = new StyleLength(line.style.marginTop.value.value - connection.y * 0.5f);

            Graph.Add(line);
        }
        
        private void DrawArrow(ClassNode footNode, ClassNode tipNode)
        {
            //create vectors u and v that hold the position of the node anchored centered on top
            Vector2 u = new Vector2(footNode.VisualElement.style.marginLeft.value.value + footNode.VisualElement.style.width.value.value * 0.5f,
                footNode.VisualElement.style.marginTop.value.value);
            Vector2 v = new Vector2(tipNode.VisualElement.style.marginLeft.value.value + tipNode.VisualElement.style.width.value.value * 0.5f,
                tipNode.VisualElement.style.marginTop.value.value);
            
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

            Graph.Add(line);
            
            //draws the arrow
            VisualElement arrow = new VisualElement();
            arrow.style.backgroundImage = new StyleBackground(arrowTexture);
            arrow.style.marginLeft = new StyleLength(v.x - arrowTexture.width * 0.5f);
            arrow.style.marginTop = new StyleLength(v.y - arrowTexture.height * 0.5f);
            arrow.style.position = new StyleEnum<Position>(UnityEngine.UIElements.Position.Absolute); 
            arrow.style.height = arrowTexture.height;
            arrow.style.width = arrowTexture.height;
            
            arrow.style.rotate = new StyleRotate( new Rotate(new Angle(Mathf.Atan2(connection.y, connection.x)*Mathf.Rad2Deg)));
            
            Graph.Add(arrow);
        }
        
        private void DrawArrowToSomewhere(ClassNode node, bool isIncomingArrow)
        {
            //create vectors u and v that hold the position of the node anchored centered on top

            Vector2 v = new Vector2(node.VisualElement.style.marginLeft.value.value + node.VisualElement.style.width.value.value * 0.5f,
                node.VisualElement.style.marginTop.value.value);;
            

            //creates the vector from node to node
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

            Graph.Add(line);

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
            
                Graph.Add(arrow);
            }
            
        }
        
    }
}

