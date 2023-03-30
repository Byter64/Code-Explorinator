using UnityEditor;
using UnityEngine;

namespace BII.GraphViewers {
    /// An element that can be drawn by the <see cref="DynamicViewportEditorWindow"/>
    public interface ViewportElement {
        /// Responsible for drawing the element using GUI or EditorGUI methods
        void Draw(Vector2 offset, float scale, bool isSelected = false);

        /// Elements of lower layer are drawn below those of a higher layer
        int Layer { get; }

        /// The bounds the element is drawn in.
        /// Does not need to account for the
        /// offset or scale of the viewport.
        Rect Bounds { get; }
    }

    public sealed class DrawableNode : ViewportElement {
        // Note DG: The editor styles used only support the following limited colors
        public enum Color : short {
            Grey = 0,
            Blue = 1,
            Teal = 2,
            Green = 3,
            Yellow = 4,
            Orange = 5,
            Red = 6
        }

        public readonly Rect Rect;
        public readonly string Text;
        public readonly Color NodeColor;

        private const int defaultTextSize = 12;
        private readonly GUIStyle boxStyle;
        private readonly GUIStyle boxStyleSelected;
        private readonly GUIStyle textStyle;

        public int Layer => 0;
        public Rect Bounds => Rect;

        public DrawableNode(Rect rect, string text, Color nodeColor) {
            Rect = rect;
            Text = text;
            NodeColor = nodeColor;

            boxStyle = $"flow node {(short) NodeColor}";
            boxStyleSelected = $"flow node {(short) NodeColor} on";
            textStyle = new GUIStyle {
                font = Font.CreateDynamicFontFromOSFont("Arial", defaultTextSize), alignment = TextAnchor.MiddleCenter,
            };
            textStyle.normal.textColor = UnityEngine.Color.white;
            textStyle.clipping = TextClipping.Clip;
        }

        public void Draw(Vector2 offset, float scale, bool isSelected) {
            var scaledTextStyle = new GUIStyle(textStyle) {fontSize = Mathf.RoundToInt(defaultTextSize * scale)};
            var rect = new Rect(
                Rect.x * scale + offset.x,
                Rect.y * scale + offset.y,
                Rect.width * scale,
                Rect.height * scale
            );
            GUI.Box(
                rect,
                text: "",
                style: isSelected ? boxStyleSelected : boxStyle
            );
            GUI.Label(rect, new GUIContent(text: Text, tooltip: Text), scaledTextStyle);
        }
    }

    public sealed class DrawableEdge : ViewportElement {
        public readonly Vector2 Begin;
        public readonly Vector2 BeginTangent;
        public readonly Vector2 End;
        public readonly Vector2 EndTangent;
        public readonly Color Color;
        public readonly float Width;

        public int Layer => -1;

        public Rect Bounds =>
            Rect.MinMaxRect(Mathf.Min(Begin.x, End.x), Mathf.Min(Begin.y, End.y), Mathf.Min(Begin.x, End.x), Mathf.Min(Begin.y, End.y));

        public DrawableEdge(
            Vector2 begin, Vector2 beginTangent, Vector2 end, Vector2 endTangent, Color color, float width
        ) {
            Begin = begin;
            BeginTangent = beginTangent;
            End = end;
            EndTangent = endTangent;
            Color = color;
            Width = width;
        }

        public void Draw(Vector2 offset, float scale, bool selected) => Handles.DrawBezier(
            (Begin * scale + offset),
            (End * scale + offset),
            (Begin + BeginTangent) * scale + offset,
            (End + EndTangent) * scale + offset,
            Color,
            texture: null,
            Width * scale * (selected ? 2 : 1)
        );
    }
}