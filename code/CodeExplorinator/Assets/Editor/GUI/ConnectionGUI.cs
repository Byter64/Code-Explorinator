using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CodeExplorinator
{
    public class ConnectionGUI : BaseGUI
    {
        protected static Texture2D LineTexture
        {
            get
            {
                if (lineTexture == null)
                {
                    lineTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Graphics/Linetexture.png");
                }
                return lineTexture;
            }
        }
        protected static Texture2D ArrowTexture
        {
            get
            {
                if (arrowTexture == null)
                {
                    arrowTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Graphics/pfeil_centered_new.png");
                }
                return arrowTexture;
            }
        }
        protected static Texture2D InheritanceArrowTexture
        {
            get
            {
                if (inheritanceArrowTexture == null)
                {
                    inheritanceArrowTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Graphics/Arrow_inheritance.png");
                }
                return inheritanceArrowTexture;
            }
        }

        public VisualElement FootNode { get; private set; }
        public VisualElement TipNode { get; private set; }

        private const float indicatingArrowLength = 100;
        private const float indicatingArrowsOffset = 50;
        private static Texture2D lineTexture;
        private static Texture2D arrowTexture;
        private static Texture2D inheritanceArrowTexture;

        private bool isInheritanceConnection;
        private bool drawArrow;
        private bool isPointingTowardNode; //Only relevant for indicating arrows

        public ConnectionGUI(GraphManager graphManager, VisualElement footNode, VisualElement tipNode, bool isInheritanceConnection = false, bool drawArrow = true) : base(graphManager)
        {
            this.FootNode = footNode;
            this.TipNode = tipNode;
            this.drawArrow = drawArrow;
            this.isInheritanceConnection = isInheritanceConnection;
            GenerateVisualElement();
        }

        public ConnectionGUI(GraphManager graphManager, VisualElement node, bool pointsTowardNode) : base(graphManager)
        {
            FootNode = node;
            TipNode = null;
            drawArrow = true;
            isInheritanceConnection = false;
            this.isPointingTowardNode = pointsTowardNode;
            GenerateIndicatingVisualElement();
        }

        public override void GenerateVisualElement()
        {
            VisualElement = CreateConnection(CalculateCenterTopPosition(FootNode), CalculateCenterTopPosition(TipNode), isInheritanceConnection);
        }

        private void GenerateIndicatingVisualElement()
        {
            Vector2 ankerpoint = CalculateCenterTopPosition(FootNode);
            ankerpoint += CalculateIndicatingOffset();
            VisualElement = CreateIndicatingConnection(ankerpoint, isPointingTowardNode);
        }

        public void UpdatePosition()
        {
            if (VisualElement != null)
            {
                if (TipNode != null)
                {
                    UpdateConnection(VisualElement, CalculateCenterTopPosition(FootNode),
                                    CalculateCenterTopPosition(TipNode), isInheritanceConnection);
                }
                else //else it is an indicating arrow
                {
                    Vector2 footPos = CalculateCenterTopPosition(FootNode) + CalculateIndicatingOffset();
                    Vector2 tipPos = CalculateCenterTopPosition(FootNode) + CalculateIndicatingOffset() + new Vector2(0, -indicatingArrowLength);
                    if (isPointingTowardNode)
                    {
                        Vector2 temp = footPos;
                        footPos = tipPos;
                        tipPos = temp;
                    }
                    UpdateConnection(VisualElement, footPos, tipPos, isInheritanceConnection);
                }
            }
        }

        public override void SetVisible(bool isVisible)
        {
            VisualElement.visible = isVisible;
        }

        private VisualElement CreateConnection(Vector2 footPos, Vector2 tipPos, bool isInheritanceArrow)
        {
            VisualElement container = new VisualElement();
            Vector2 footToTip = tipPos - footPos;

            VisualElement line = new VisualElement();
            container.Add(SetLinePosition(line, footPos, footToTip));

            if (drawArrow)
            {
                VisualElement arrow = new VisualElement();
                container.Add(SetArrowPosition(arrow, tipPos, footToTip, isInheritanceArrow ? InheritanceArrowTexture : ArrowTexture));
            }

            return container;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ankerpoint">The ankerpoint on the class on the top border of the class</param>
        /// <param name="isIncomingArrow">If the arrow points to the class (or away from the class)</param>
        /// <returns></returns>
        private VisualElement CreateIndicatingConnection(Vector2 ankerpoint, bool isIncomingArrow)
        {
            VisualElement container = new VisualElement();
            VisualElement line = new VisualElement();
            VisualElement arrowhead = new VisualElement();
            Vector2 arrowAsVector = new Vector2(0, -indicatingArrowLength);
            Vector2 arrowHeadPos = isIncomingArrow ? ankerpoint : (ankerpoint + arrowAsVector);

            container.Add(SetLinePosition(line, ankerpoint, arrowAsVector));
            if (!isIncomingArrow)
            {
                arrowAsVector *= -1;
            }
            container.Add(SetIncomingArrowPosition(arrowhead, arrowHeadPos, arrowAsVector, ArrowTexture));
            return container;
        }

        private void UpdateConnection(VisualElement parent, Vector2 footPos, Vector2 tipPos, bool isInheritanceArrow)
        {
            Vector2 footToTip = tipPos - footPos;

            SetLinePosition(parent.Children().First(), footPos, footToTip);

            if (drawArrow)
            {
                SetArrowPosition(parent.Children().Last(), tipPos, footToTip, isInheritanceArrow ? InheritanceArrowTexture : ArrowTexture);
            }
        }

        private VisualElement SetArrowPosition(VisualElement arrow, Vector2 v, Vector2 connection, Texture2D arrowTexture2D)
        {
            arrow.style.backgroundImage = new StyleBackground(arrowTexture2D);
            arrow.style.marginLeft = new StyleLength(v.x - (arrowTexture2D.width * 0.5f));
            arrow.style.marginTop = new StyleLength(v.y - (arrowTexture2D.height * 0.5f));
            arrow.style.position = new StyleEnum<Position>(Position.Absolute);
            arrow.style.height = arrowTexture2D.height;
            arrow.style.width = arrowTexture2D.height;

            arrow.style.rotate =
                new StyleRotate(new Rotate(new Angle(Mathf.Atan2(connection.y, connection.x) * Mathf.Rad2Deg)));

            return arrow;
        }

        private VisualElement SetIncomingArrowPosition(VisualElement arrow, Vector2 arrowHeadPosition, Vector2 connection, Texture2D arrowTexture2D)
        {
            arrow.style.backgroundImage = new StyleBackground(arrowTexture2D);
            arrow.style.marginLeft = new StyleLength(arrowHeadPosition.x - (arrowTexture2D.width * 0.5f));
            arrow.style.marginTop = new StyleLength(arrowHeadPosition.y - (arrowTexture2D.height * 0.5f));
            arrow.style.position = new StyleEnum<Position>(Position.Absolute);
            arrow.style.height = arrowTexture2D.height;
            arrow.style.width = arrowTexture2D.height;

            arrow.style.rotate =
                new StyleRotate(new Rotate(new Angle(Mathf.Atan2(-connection.y, -connection.x) * Mathf.Rad2Deg)));

            return arrow;
        }

        private VisualElement SetLinePosition(VisualElement line, Vector2 ankerpoint, Vector2 connection)
        {
            line.style.backgroundImage = new StyleBackground(LineTexture);
            //sets the rotationpoint of the line to the ankerpoint position (the rotationpoint of the line lies in the middle) the line is not rotated yet, thats why we use the magnitude
            line.style.marginLeft = new StyleLength(ankerpoint.x - (connection.magnitude * 0.5f));
            line.style.marginTop = new StyleLength(ankerpoint.y);
            line.style.position = new StyleEnum<Position>(Position.Absolute);
            line.style.height = LineTexture.height;
            //the width of the line is the magnitude of the vector, as the line is not rotated yet
            line.style.width = connection.magnitude;

            //rotates the line to be parallel to the vector
            line.style.rotate =
                new StyleRotate(new Rotate(new Angle(Mathf.Atan2(connection.y, connection.x) * Mathf.Rad2Deg)));

            //repositions the line half of the vector further to complete the process
            line.style.marginLeft = new StyleLength(line.style.marginLeft.value.value + (connection.x * 0.5f));
            line.style.marginTop = new StyleLength(line.style.marginTop.value.value + (connection.y * 0.5f));

            return line;
        }

        /// <summary>
        /// Calculates the relative center top position of the given node.
        /// </summary>
        /// <param name="node">The node of which the position is to be calculated</param>
        /// <returns>The center top position of the given node</returns>
        private Vector2 CalculateCenterTopPosition(VisualElement node)
        {
            return new Vector2(node.style.marginLeft.value.value + (node.style.width.value.value * 0.5f), node.style.marginTop.value.value);
        }

        private Vector2 CalculateIndicatingOffset()
        {
            return new Vector2(isPointingTowardNode ? -indicatingArrowsOffset : indicatingArrowsOffset, 0);
        }
    }
}