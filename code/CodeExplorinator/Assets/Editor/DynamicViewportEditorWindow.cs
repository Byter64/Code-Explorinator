using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace BII.GraphViewers {
    /// This class is an abstraction for an editor window
    /// that provides an interactable viewport.
    /// It can render any kind of <see cref="ViewportElement"/> in its viewport,
    /// and the user can drag the viewport around or scale it.
    public abstract class DynamicViewportEditorWindow : EditorWindow {
        // All allowed scaling factors. These have been hard-coded
        // based on what values "feel" good to use
        private static readonly float[] scaleSteps = new[] {0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f, 1.2f, 1.4f};
        
        private int _currentScaleIndex = 6;

        private float scale => scaleSteps[_currentScaleIndex];

        private Vector2 dragOffset { get; set; }
        
        protected int _legendWidth = 320;

        private Rect viewportRect => new Rect(
            x: -dragOffset.x / scale,
            y: -dragOffset.y / scale,
            width: position.width / scale,
            height: position.height / scale
        );

        [CanBeNull] protected ViewportElement _selectedElement = null;

        private void OnEnable() {
            wantsMouseMove = true;
            wantsMouseEnterLeaveWindow = true;
            onEnable();
        }

        /// Can be overwritten for initialization logic 
        protected virtual void onEnable() { }

        /// Must be implemented to provide the elements that should be drawn.
        protected abstract IEnumerable<ViewportElement> contentToDraw { get; }

        /// Must be implemented to draw the legend of the viewport.
        /// The legend is a vertical area on the left, where
        /// GUILayout or EditorGUILayout elements can be drawn in.
        /// Usually, it should be used for any elements the user
        /// can use to control the viewport.
        /// Unlike the <see cref="contentToDraw"/> these elements
        /// are not affected by the dragging or scaling of the viewport. 
        protected abstract void drawLegend();

        /// Called after all elements have been drawn.
        /// Can be overwritten for custom drawing or update logic.
        protected virtual void onPostDraw() { }

        private void OnGUI() {
            var gray1 = Color.gray;
            gray1.a = 0.4f;
            var gray2 = Color.gray;
            gray2.a = 0.2f;
            drawGrid(gridSpacing: 100 * scale, gray1);
            if (scale > 0.5f)
                drawGrid(gridSpacing: 20 * scale, gray2);

            foreach (var content in contentToDraw.Where(c => viewportRect.Overlaps(c.Bounds)).OrderBy(c => c.Layer)) {
                content.Draw(dragOffset, scale, _selectedElement.Equals(content));
            }

            GUI.Box(new Rect(0, 0, _legendWidth, position.height), "");
            GUILayout.BeginArea(new Rect(5, 5, _legendWidth - 10, position.height - 10));
            drawLegend();
            GUILayout.EndArea();

            onPostDraw();

            processEvents();

            if (GUI.changed)
                Repaint();
        }

        private void drawGrid(float gridSpacing, Color gridColor) {
            var verticalLineCount = Mathf.CeilToInt(position.width / gridSpacing) + 1;
            var horizontalLineCount = Mathf.CeilToInt(position.height / gridSpacing) + 1;

            Handles.BeginGUI();
            Handles.color = gridColor;

            var offset = new Vector2(dragOffset.x % gridSpacing, dragOffset.y % gridSpacing);

            // Draw vertical lines
            drawLines(
                verticalLineCount,
                lineBegin: i => new Vector2(gridSpacing * i, -gridSpacing) + offset,
                lineEnd: i => new Vector2(gridSpacing * i, position.height + gridSpacing) + offset
            );

            // Draw horizontal lines
            drawLines(
                horizontalLineCount,
                lineBegin: i => new Vector2(-gridSpacing, gridSpacing * i) + offset,
                lineEnd: i => new Vector2(position.width + gridSpacing, gridSpacing * i) + offset
            );

            Handles.color = Color.white;
            Handles.EndGUI();

            void drawLines(int count, Func<int, Vector2> lineBegin, Func<int, Vector2> lineEnd) {
                for (var index = 0; index < count; index++)
                    Handles.DrawLine(lineBegin(index), lineEnd(index));
            }
        }

        private void processEvents() {
            var e = Event.current;

            switch (e.type) {
                case EventType.MouseDrag when (e.button == 0 && e.alt) || e.button == 2:
                    dragScreen(e.delta);
                    break;
                case EventType.MouseDown when (e.button == 0) && e.mousePosition.x > 250:
                    onMouseDown((e.mousePosition - dragOffset) / scale);
                    break;
                case EventType.ScrollWheel when e.delta.y < 0:
                    changeScale(i => i + 1, e.mousePosition);
                    break;
                case EventType.ScrollWheel when e.delta.y > 0:
                    changeScale(i => i - 1, e.mousePosition);
                    break;
            }
        }

        private void changeScale(Func<int, int> scaleIndexChanger, Vector2 mousePosition) {
            var prevFixpointPos = (mousePosition - dragOffset) / scale;
            _currentScaleIndex = scaleIndexChanger(_currentScaleIndex);
            if (_currentScaleIndex < 0) _currentScaleIndex = 0;
            if (_currentScaleIndex > scaleSteps.Length - 1) _currentScaleIndex = scaleSteps.Length - 1;
            var newFixpointPos = prevFixpointPos * scale + dragOffset;
            dragOffset += (mousePosition - newFixpointPos);
            GUI.changed = true;
        }

        private void dragScreen(Vector2 delta) {
            dragOffset += delta;
            GUI.changed = true;
        }

        private void onMouseDown(Vector2 gridPosition) {
            var clickedElement = contentToDraw.FirstOrDefault(e => e.Bounds.Contains(gridPosition));
            OnElementClicked(clickedElement, gridPosition);
        }

        /// Called when the user clicks something, parses the element when an element is clicked.
        protected virtual void OnElementClicked([CanBeNull] ViewportElement element, Vector2 gridPosition) { }
    }
}