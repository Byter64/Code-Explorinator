using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEngine.UIElements;
using System.Linq;

namespace CodeExplorinator
{
    public class MethodGUI : BaseGUI
    {
        public MethodData data { get; private set; }

        private const string methodHighlightPath = "Assets/Editor/TiledTextures/HighlightedMethod "; //This needs a number between [0, 10]  + ".asset" inserted
        private static TiledTextureData backgroundData;
        private static TiledTextureBuilder backgroundBuilder;

        private bool isFocused = false;
        private bool isHighlighted = false;
        private bool isVisible = false;
        private int distanceToClosestFocusMethod = -1;
        private GUIStyle style;
        private Texture2D backgroundTexture;
        private ClickBehaviour clickBehaviour;

        public MethodGUI(MethodData data, GUIStyle style, GraphManager graphManager) : base(graphManager)
        {
            this.data = data;
            this.style = style;

            backgroundTexture = Create9SlicedTexture(methodHighlightPath + 0 + ".asset", CalculateBackgroundSize());
            GenerateVisualElement();
        }

        /// <summary>
        /// Generates a Visualelement and saves it in the property "VisualElement". 
        /// </summary>
        public override void GenerateVisualElement()
        {
            VisualElement background = new VisualElement();
            //IMPORTANT: Don't set the height here because it may give ugly result because of the auto layout
            background.style.backgroundImage = Background.FromTexture2D(backgroundTexture);
            background.style.backgroundSize = new StyleBackgroundSize(new BackgroundSize(backgroundTexture.width, backgroundTexture.height));
            background.style.width = backgroundTexture.width;
            background.style.flexShrink = 1;
            background.style.visibility = Visibility.Hidden;

            //TODO: add bold text here to improve visibility for example: new Label("<b>" + data.ToRichString() + "</b>");
            Label method = new Label(data.ToRichString());
            method.style.unityFont = new StyleFont(style.font);
            method.style.unityFontDefinition = new StyleFontDefinition(style.font);
            method.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleLeft);
            method.style.fontSize = style.fontSize;
            method.style.color = UnityEngine.Color.black;
            method.style.flexShrink = background.style.flexShrink;
            method.style.visibility = new StyleEnum<Visibility>(Visibility.Visible);
            background.Add(method);
            VisualElement = background;
            TryAssignClickBehaviours();
        }

        public void ShowHighlight(bool isShowingHighlight)
        {
            isHighlighted = isShowingHighlight;

            Visibility visiBackground = isShowingHighlight ? Visibility.Visible : Visibility.Hidden;
            VisualElement.visible = isShowingHighlight;
            VisualElement.Children().First().visible = isVisible;
        }

        public void SetFocused(bool isFocused, int distanceToClosestFocusMethod = -1)
        {
            this.isFocused = isFocused;
            this.distanceToClosestFocusMethod = distanceToClosestFocusMethod;

            if (distanceToClosestFocusMethod >= 0)
            {
                backgroundTexture = Create9SlicedTexture(methodHighlightPath + distanceToClosestFocusMethod + ".asset", CalculateBackgroundSize());
                VisualElement.style.backgroundImage = Background.FromTexture2D(backgroundTexture);
            }
        }

        private void TryAssignClickBehaviours()
        {
            clickBehaviour ??= new ClickBehaviour(VisualElement, SetFocusMethod, null);
            clickBehaviour.RegisterOnControlMonoClick(AddSelectedMethod);
        }

        private void AddSelectedMethod()
        {
            graphManager.AddSelectedMethod(data.MethodNode);
        }

        private void SetFocusMethod()
        {
            graphManager.AddSelectedMethod(data.MethodNode);
            graphManager.AdjustGraphToSelectedMethods();
            graphManager.ChangeToMethodLayer();
        }

        public override void SetVisible(bool isVisible)
        {
            this.isVisible = isVisible;

            ShowHighlight(isVisible && isHighlighted);
            VisualElement.Children().First().visible = isVisible;
        }

        private Vector2Int CalculateBackgroundSize()
        {
            Vector2 result = Vector2.zero;
            result.y = 0; //because y depends on the texture, we don't know it yet.

            Label tempLabel = new Label();
            tempLabel.style.unityFont = style.font;
            tempLabel.style.fontSize = style.fontSize;

            Vector2 textSize = tempLabel.MeasureTextSize(data.ToString(), 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
            result.x = textSize.x;

            return new Vector2Int((int)result.x, (int)result.y);
        }

    }
}