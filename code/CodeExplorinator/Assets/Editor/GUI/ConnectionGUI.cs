using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace CodeExplorinator
{
    public class ConnectionGUI : IPositionBackup
    {
        public VisualElement VisualElement { get; private set; }
        private Vector2 positionBackup;
        private Texture2D lineTexture;
        private Texture2D arrowTexture;
        private Texture2D inheritanceArrowTexture;
        private VisualElement footNode;
        private VisualElement tipNode;

        public ConnectionGUI(VisualElement footNode, VisualElement tipNode)
        {
            this.footNode = footNode;
            this.tipNode = tipNode;
            lineTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Graphics/Linetexture.png");
            arrowTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Graphics/pfeil_centered_new.png");
            inheritanceArrowTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Graphics/Arrow_inheritance.png");
        }
        
        public void GenerateVisualElement(bool isInheritanceArrow = false, bool isMethod = false)
        {
            if (footNode == null)
            {
                VisualElement = CreateRandomConnection(isMethod? CenteredLeftVectorForMethod(tipNode) :CenteredTopVector(tipNode), true, isInheritanceArrow);
            }
            else if (tipNode == null)
            {
                VisualElement = CreateRandomConnection(isMethod? CenteredLeftVectorForMethod(footNode) :CenteredTopVector(footNode), false, isInheritanceArrow);
            }
            else
            {
                VisualElement = CreateConnection(isMethod? CenteredLeftVectorForMethod(footNode) :CenteredTopVector(footNode), isMethod? CenteredLeftVectorForMethod(tipNode) :CenteredTopVector(tipNode), isInheritanceArrow);
            }
        }

        /// <summary>
        /// Saves the current position internally. Call RestorePositionBackup to restore the position.
        /// </summary>
        public void MakePositionBackup()
        {
            positionBackup = new Vector2(VisualElement.style.marginLeft.value.value, VisualElement.style.marginTop.value.value);
        }

        /// <summary>
        /// Restores the position which was last saved by MakePositionBackup
        /// </summary>
        public void RestorePositionBackup()
        {
            VisualElement.style.marginLeft = positionBackup.x;
            VisualElement.style.marginTop = positionBackup.y;
        }

        private VisualElement CreateConnection(Vector2 footPos, Vector2 tipPos, bool isInheritanceArrow)
        {
            //creates parent visual element
            VisualElement parent = new VisualElement();
            
            //creates the vector from node to node
            Vector2 connection = new Vector2(tipPos.x - footPos.x, tipPos.y - footPos.y);
            
            parent.Add(InstantiateLine(footPos,connection));
            
            parent.Add(InstantiateArrow(tipPos,connection, isInheritanceArrow ? inheritanceArrowTexture : arrowTexture));

            return parent;
            
        }
        
        private VisualElement CreateRandomConnection(Vector2 position, bool isIncomingArrow ,bool isInheritanceArrow)
        {
            //creates parent visual element
            VisualElement parent = new VisualElement();
            
            Vector2 connection = new Vector2(Random.Range(-1000, 1000), Random.Range(-1000, 1000)).normalized * 500; //normalise it to the same length

            parent.Add(InstantiateLine(position,connection));

            if (isIncomingArrow)
            {
                VisualElement arrow = InstantiateIncomingArrow(position, connection, isInheritanceArrow ? inheritanceArrowTexture : arrowTexture);
                parent.Add(arrow);
            }

            return parent;
        }

        private VisualElement InstantiateArrow(Vector2 v, Vector2 connection, Texture2D arrowTexture2D)
        {
            //draws the arrow
            VisualElement arrow = new VisualElement();
            arrow.style.backgroundImage = new StyleBackground(arrowTexture2D);
            arrow.style.marginLeft = new StyleLength(v.x - arrowTexture2D.width * 0.5f);
            arrow.style.marginTop = new StyleLength(v.y - arrowTexture2D.height * 0.5f);
            arrow.style.position = new StyleEnum<Position>(UnityEngine.UIElements.Position.Absolute);
            arrow.style.height = arrowTexture2D.height;
            arrow.style.width = arrowTexture2D.height;

            arrow.style.rotate =
                new StyleRotate(new Rotate(new Angle(Mathf.Atan2(connection.y, connection.x) * Mathf.Rad2Deg)));

            return arrow;
        }

        private VisualElement InstantiateIncomingArrow(Vector2 u, Vector2 connection,Texture2D arrowTexture2D)
        {
            //draws the arrow
            VisualElement arrow = new VisualElement();
            arrow.style.backgroundImage = new StyleBackground(arrowTexture2D);
            //centering v in the arrow image
            arrow.style.marginLeft = new StyleLength(u.x - arrowTexture2D.width * 0.5f);
            arrow.style.marginTop = new StyleLength(u.y - arrowTexture2D.height * 0.5f);
            arrow.style.position = new StyleEnum<Position>(UnityEngine.UIElements.Position.Absolute);
            arrow.style.height = arrowTexture2D.height;
            arrow.style.width = arrowTexture2D.height;

            arrow.style.rotate =
                new StyleRotate(new Rotate(new Angle(Mathf.Atan2(-connection.y, -connection.x) * Mathf.Rad2Deg)));

            return arrow;
        }

        private VisualElement InstantiateLine(Vector2 u, Vector2 connection)
        {
            //instantiates the line
            VisualElement line = new VisualElement();
            line.style.backgroundImage = new StyleBackground(lineTexture);
            //sets the rotationpoint of the line to the position u (the rotationpoint of the line lies in the middle) the line is not rotated yet, thats why we use the magnitude
            line.style.marginLeft = new StyleLength(u.x - connection.magnitude * 0.5f);
            line.style.marginTop = new StyleLength(u.y);
            line.style.position = new StyleEnum<Position>(Position.Absolute);
            line.style.height = lineTexture.height;
            //the width of the line is the magnitude of the vector, as the line is not rotated yet
            line.style.width = connection.magnitude;

            //rotates the line to be parallel to the vector
            line.style.rotate =
                new StyleRotate(new Rotate(new Angle(Mathf.Atan2(connection.y, connection.x) * Mathf.Rad2Deg)));

            //repositions the line half of the vector further to complete the process
            line.style.marginLeft = new StyleLength(line.style.marginLeft.value.value + connection.x * 0.5f);
            line.style.marginTop = new StyleLength(line.style.marginTop.value.value + connection.y * 0.5f);

            return line;
        }

        /// <summary>
        /// creates vector v that holds the position of the node anchored centered on top
        /// </summary>
        /// <param name="node">the node of which the position shall be calculated</param>
        /// <returns></returns>
        private Vector2 CenteredTopVector(VisualElement node)
        {
            return new Vector2(
                node.style.marginLeft.value.value +
                node.style.width.value.value * 0.5f,
                node.style.marginTop.value.value);
        }
        
        private Vector2 CenteredLeftVectorForMethod(VisualElement node)
        {
            return new Vector2(node.parent.style.marginLeft.value.value + (
                node.style.marginLeft.value.value),
                node.parent.style.marginTop.value.value + (
                    node.style.marginTop.value.value + node.style.height.value.value * 0.5f));
        }
    }
}