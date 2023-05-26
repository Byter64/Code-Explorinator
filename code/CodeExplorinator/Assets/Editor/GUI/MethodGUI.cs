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
        private static TiledTextureData BackgroundTextureData
        {
            get
            {
                if (backgroundData == null)
                {
                    backgroundData = (TiledTextureData)AssetDatabase.LoadAssetAtPath("Assets/Editor/TiledTextures/Method.asset", typeof(TiledTextureData));
                }
                return backgroundData;
            }
        }

        private static TiledTextureBuilder BackgroundBuilder
        {
            get
            {
                if (backgroundBuilder == null)
                {
                    byte[] fileData = File.ReadAllBytes(Application.dataPath + "/Editor/Graphics/" + BackgroundTextureData.graphicsPath);
                    Texture2D texture = new Texture2D(1, 1);
                    ImageConversion.LoadImage(texture, fileData);
                    backgroundBuilder = new TiledTextureBuilder(texture, BackgroundTextureData.middleRectangle);
                }
                return backgroundBuilder;
            }
        }
        private static TiledTextureData backgroundData;
        private static TiledTextureBuilder backgroundBuilder;

        private GUIStyle style;
        private Texture2D backgroundTexture;
        private ClickBehaviour clickBehaviour;

        public MethodGUI(MethodData data, GUIStyle style, GraphVisualizer graphManager) : base(graphManager)
        {
            this.data = data;
            this.style = style;

            BackgroundBuilder.Size = CalculateBackgroundSize();
            backgroundTexture = BackgroundBuilder.BuildTexture();
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

        public void ShowBackground(bool isShowingBackground)
        {
            Visibility visiBackground = isShowingBackground ? Visibility.Visible : Visibility.Hidden;

            VisualElement.style.visibility = visiBackground;
            VisualElement.Children().First().style.visibility = Visibility.Visible;
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
            graphManager.ChangeToMethodLayer();
            graphManager.AddSelectedMethod(data.MethodNode);
            graphManager.FocusOnSelectedMethods();
        }

        private Vector2Int CalculateBackgroundSize()
        {
            Vector2 result = Vector2.zero;
            result.y = BackgroundBuilder.OriginalSize.y;

            Label tempLabel = new Label();
            tempLabel.style.unityFont = style.font;
            tempLabel.style.fontSize = style.fontSize;

            Vector2 textSize = tempLabel.MeasureTextSize(data.ToString(), 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
            result.x = textSize.x;

            return new Vector2Int((int)result.x, (int)result.y);
        }

        public void SetVisible(bool visible)
        {
            VisualElement.Children().First().visible = visible;
            //If will be hidden, make background hidden, too
            if(!visible)
            {
                VisualElement.visible = visible;
            }
        }
    }
}