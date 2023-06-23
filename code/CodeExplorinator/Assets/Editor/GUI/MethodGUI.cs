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

        private const string methodTexturePath = "Assets/Editor/TiledTextures/Method.asset";
        private const string focusedMethodTexturePath = "Assets/Editor/TiledTextures/FocusedMethod.asset";
        private static TiledTextureData backgroundData;
        private static TiledTextureBuilder backgroundBuilder;

        private bool isFocused = false;
        private bool isHighlighted = false;
        private GUIStyle style;
        private Texture2D backgroundTexture;
        private Texture2D focusedBackgroundTexture;
        private ClickBehaviour clickBehaviour;

        public MethodGUI(MethodData data, GUIStyle style, GraphManager graphManager) : base(graphManager)
        {
            this.data = data;
            this.style = style;

            backgroundTexture = Create9SlicedTexture(methodTexturePath, CalculateBackgroundSize());
            focusedBackgroundTexture = Create9SlicedTexture(focusedMethodTexturePath, CalculateBackgroundSize());
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
            Visibility visiBackground = isShowingHighlight ? Visibility.Visible : Visibility.Hidden;

            isHighlighted = isShowingHighlight;
            VisualElement.style.visibility = visiBackground;
            VisualElement.Children().First().style.visibility = Visibility.Visible;
        }

        public void SetFocused(bool isFocused)
        {
            this.isFocused = isFocused;
            if (isFocused)
            {
                VisualElement.style.backgroundImage = Background.FromTexture2D(focusedBackgroundTexture);
            }
            else
            {
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

        public override void SetVisible(bool visible)
        {
            //Hide text
            VisualElement.Children().First().visible = visible;
            
            //Hide highlight
            //If will be hidden, make background hidden, too
            if(visible)
            {
                if(isHighlighted)
                {
                    ShowHighlight(true);
                }
            }
            else
            {
                VisualElement.visible = visible;
            }
        }
    }
}