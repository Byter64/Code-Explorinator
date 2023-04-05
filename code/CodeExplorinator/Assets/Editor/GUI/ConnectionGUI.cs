

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

namespace CodeExplorinator
{
    public class ConnectionGUI
    {
        private Texture2D lineTexture;
        private List<Node> Nodes;
        private VisualElement Graph;

        public ConnectionGUI(List<Node> nodes, VisualElement graph, Texture2D temp)
        {
            Nodes = nodes;
            Graph = graph;
            
            byte[] fileData = File.ReadAllBytes(Application.dataPath + "/Editor/Graphics/Linetexture.png");
            lineTexture =  new Texture2D(1, 1);
            ImageConversion.LoadImage(lineTexture, fileData);

            lineTexture = temp;

        }

        public void DrawConnections()
        {
            foreach (var node in Nodes)
            {
                if (!node.IsLeaf)
                {
                    foreach (var connectedNode in node.ConnectedNodes)
                    {
                        DrawLine(node, connectedNode);
                    }  
                }
                else
                {
                    
                }
                
            }
        }

        private void DrawLine(Node node, Node connectedNode)
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
        
    }
}

