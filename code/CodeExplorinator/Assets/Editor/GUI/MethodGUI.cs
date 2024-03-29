using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace CodeExplorinator
{
    public class MethodGUI : BaseGUI
    {
        public MethodData data { get; private set; }

        private const string methodHighlightPath = Utilities.pathroot + "Editor/TiledTextures/HighlightedMethod "; //This path needs a number between [0, 10]  + ".asset" appended

        private bool isHighlighted = false;
        private bool isVisible = false;
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

            Label method = new Label(data.ToRichString());
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

        public void ShowHighlight()
        {
            bool showHighlight = isVisible && isHighlighted;
            VisualElement.visible = showHighlight;
            VisualElement.Children().First().visible = isVisible;
        }

        public void SetHighlight(bool isShowingHighlight)
        {
            isHighlighted = isShowingHighlight;
            ShowHighlight();
        }

        public void SetFocused(bool isFocused, int distanceToClosestFocusMethod = -1)
        {
            if (distanceToClosestFocusMethod >= 0)
            {
                backgroundTexture = Create9SlicedTexture(methodHighlightPath + distanceToClosestFocusMethod + ".asset", CalculateBackgroundSize());
                VisualElement.style.backgroundImage = Background.FromTexture2D(backgroundTexture);
            }
        }

        private void TryAssignClickBehaviours()
        {
            clickBehaviour ??= new ClickBehaviour(VisualElement, null, SetFocusMethod);
            clickBehaviour.RegisterOnControlMonoClick(AddSelectedMethod);
        }

        private void AddSelectedMethod()
        {
            graphManager.AddSelectedMethod(data.MethodNode);
        }

        private void SetFocusMethod()
        {
            graphManager.AddSelectedMethod(data.MethodNode);
            graphManager.ApplySelectedMethods();
        }

        public override void SetVisible(bool isVisible)
        {
            this.isVisible = isVisible;

            ShowHighlight();
            VisualElement.Children().First().visible = isVisible;
        }

        private Vector2Int CalculateBackgroundSize()
        {
            Vector2 result = Vector2.zero;
            result.y = 0; //because y depends on the texture, we don't know its width yet.

            Label tempLabel = new Label();
            tempLabel.style.unityFont = style.font;
            tempLabel.style.fontSize = style.fontSize;

            Vector2 textSize = tempLabel.MeasureTextSize(data.ToString(), 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
            result.x = textSize.x;

            return new Vector2Int((int)result.x, (int)result.y);
        }

    }
}