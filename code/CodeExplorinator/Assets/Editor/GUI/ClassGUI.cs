using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CodeExplorinator
{
    [System.Serializable]
    public class ClassGUI
    {
        public Vector2 Position { get; set; }
        public Vector2 Size
        {
            get { return new Vector2(backgroundTexture.width, backgroundTexture.height); }
        }
        
        public VisualElement VisualElement { get; private set; }

        private static TiledTextureData BackgroundTextureData
        {
            get
            {
                if (backgroundData == null)
                {
                    backgroundData = (TiledTextureData)AssetDatabase.LoadAssetAtPath("Assets/Editor/TiledTextures/Class.asset", typeof(TiledTextureData));
                }
                return backgroundData;
            }
        }

        private static TiledTextureData HeaderTextureData
        {
            get
            {
                if (headerData == null)
                {
                    headerData = (TiledTextureData)AssetDatabase.LoadAssetAtPath("Assets/Editor/TiledTextures/Header.asset", typeof(TiledTextureData));
                }
                return headerData;
            }
        }

        private static TiledTextureBuilder BackgroundBuilder
        {
            get
            {
                if(backgroundBuilder == null)
                {
                    byte[] fileData = File.ReadAllBytes(Application.dataPath + "/Editor/Graphics/" + BackgroundTextureData.graphicsPath);
                    Texture2D texture = new Texture2D(1, 1);
                    ImageConversion.LoadImage(texture, fileData);
                    backgroundBuilder = new TiledTextureBuilder(texture, BackgroundTextureData.middleRectangle);
                }
                return backgroundBuilder;
            }
        }

        private static TiledTextureBuilder HeaderBuilder
        {
            get
            {
                if (headerBuilder == null)
                {
                    byte[] fileData = File.ReadAllBytes(Application.dataPath + "/Editor/Graphics/" + HeaderTextureData.graphicsPath);
                    Texture2D texture = new Texture2D(1, 1);
                    ImageConversion.LoadImage(texture, fileData);
                    headerBuilder = new TiledTextureBuilder(texture, HeaderTextureData.middleRectangle);
                }
                return headerBuilder;
            }
        }

        #region Defines for the graphics
        /// <summary>
        /// Height in pixels of the header for the class
        /// </summary>
        private static int headerHeight = 88;
        /// <summary>
        /// the amount of pixels which everything within the box is moved to the right
        /// </summary>
        private static int intendation = 20;
        /// <summary>
        /// How much empty space will be between the last element in the box and the bottom border of the box
        /// </summary>
        private static int emptySpaceBottom = 10;
        #endregion

        private static TiledTextureBuilder backgroundBuilder;
        private static TiledTextureData backgroundData;
        private static TiledTextureBuilder headerBuilder;
        private static TiledTextureData headerData;

        private bool isExpanded;
        private int widthInPixels;
        private int heightInPixels;

        private GUIStyle classStyle;
        private GUIStyle fieldStyle;
        private GUIStyle methodStyle;
        private ClassData data;
        private Texture2D backgroundTexture;
        private Texture2D headerTexture;
        private Texture2D lineTexture;
        private GraphManager graphManager;
        private VisualElement header;
        private ClickBehaviour bodyClick;
        private ClickBehaviour headerClick;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="classStyle">The style in which the class name will be displayed</param>
        /// <param name="fieldStyle">The style in which fields AND properties will be displayed</param>
        /// <param name="methodStyle">The style in which methods will be displayed</param>
        public ClassGUI(Vector2 position, ClassData data, GUIStyle classStyle, GUIStyle fieldStyle, GUIStyle methodStyle, GraphManager graphManager)
        {
            if (!classStyle.font.dynamic)
            {
                throw new System.ArgumentException("Font for class style is not dynamic and thus cannot be scaled");
            }
            if (!fieldStyle.font.dynamic)
            {
                throw new System.ArgumentException("Font for field style is not dynamic and thus cannot be scaled");
            }
            if (!methodStyle.font.dynamic)
            {
                throw new System.ArgumentException("Font for field style is not dynamic and thus cannot be scaled");
            }

            isExpanded = true;
            Position = position;
            this.data = data;
            this.classStyle = classStyle;
            this.fieldStyle = fieldStyle;
            this.methodStyle = methodStyle;
            this.graphManager = graphManager;
            lineTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Graphics/Linetexture.png");

            BackgroundBuilder.Size = CalculateBackgroundSize();
            backgroundTexture = BackgroundBuilder.BuildTexture();
            widthInPixels = backgroundTexture.width;
            heightInPixels = backgroundTexture.height;
            HeaderBuilder.Size = new Vector2Int(widthInPixels, 0);
            headerTexture = HeaderBuilder.BuildTexture();
        }

        public void GenerateVisualElement()
        {
            //Debug.LogWarning("Default font is used because I do not know how to change the default font");
            VisualElement classElement = new VisualElement();
            classElement.style.backgroundImage = Background.FromTexture2D(backgroundTexture);
            classElement.style.backgroundSize = new StyleBackgroundSize(new BackgroundSize(widthInPixels, heightInPixels));
            classElement.style.height = heightInPixels;
            classElement.style.width = widthInPixels;
            classElement.style.position = new StyleEnum<Position>(UnityEngine.UIElements.Position.Absolute);
            classElement.style.marginLeft = Position.x;
            classElement.style.marginTop = Position.y;

            #region DrawHeader
            string headerText = data.ClassModifiersList.Count == 0 ? "" : "<<" + data.ClassModifiersAsString + ">>";
            headerText += "\n" + data.GetName();
            Label header = new Label(headerText);
            header.style.backgroundImage = Background.FromTexture2D(headerTexture);
            header.style.backgroundSize = new StyleBackgroundSize(new BackgroundSize(headerTexture.width, headerTexture.height));
            header.style.unityFont = new StyleFont(classStyle.font);
            header.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.UpperCenter);
            header.style.height = headerTexture.height;
            header.style.fontSize = classStyle.fontSize;
            header.style.color = Color.black;
            this.header = header;

            classElement.Add(header);
            #endregion

            #region DrawFields
            VisualElement fields = new VisualElement();
            fields.style.paddingLeft = intendation;
            classElement.Add(fields);

            foreach (FieldData fieldData in data.PublicVariables.Concat(data.PrivateVariables))
            {
                Label field = new Label(fieldData.ToString());
                field.style.unityFont = new StyleFont(fieldStyle.font);
                field.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.UpperLeft);
                field.style.fontSize = fieldStyle.fontSize;
                field.style.color = Color.black;

                fields.Add(field);
            }
            #endregion

            if (data.PublicMethods.Concat(data.PrivateMethods).Count() != 0 && data.PublicVariables.Concat(data.PrivateVariables).Count() != 0)
            {
                VisualElement line = new VisualElement();
                line.style.backgroundImage = new StyleBackground(lineTexture);
                line.style.height = lineTexture.height;
                line.style.width = classElement.style.width;

                classElement.Add(line);
            }

            #region DrawMethods
            VisualElement methods = new VisualElement();
            methods.style.paddingLeft = intendation;
            classElement.Add(methods);
            foreach (MethodData methodData in data.PublicMethods.Concat(data.PrivateMethods))
            {
                Label method = new Label(methodData.ToString());
                StyleFont de = new StyleFont(methodStyle.font);
                method.style.unityFont = de;
                method.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.UpperLeft);
                method.style.fontSize = methodStyle.fontSize;
                method.style.color = Color.black;

                methods.Add(method);
            }
            #endregion

            VisualElement = classElement;
            TryAssignClickBehaviours();
        }

        private void TryAssignClickBehaviours()
        {
            bodyClick ??= new ClickBehaviour(VisualElement, null, UpdateFocusClass);
            headerClick ??= new ClickBehaviour(header, SwapExpandedCollapsed, UpdateFocusClass);
        }

        private void SwapExpandedCollapsed()
        {
            isExpanded = !isExpanded;
            if (isExpanded)
            {
                VisualElement.visible = true;
                header.visible = true;
                Debug.Log("Ich bin jetzt ausgeklappt: " + data);
            }
            else
            {
                VisualElement.visible = false;
                header.visible = true;
                Debug.Log("Ich bin jetzt eingeklappt: " + data);
            }
        }

        private void UpdateFocusClass()
        {
            graphManager.UpdateFocusClass(data);
        }

        private Vector2Int CalculateBackgroundSize()
        {
            Vector2 result = Vector2.zero;

            result.y += headerHeight;

            foreach(FieldData field in data.PublicVariables.Concat(data.PrivateVariables))
            {
                result.y += fieldStyle.lineHeight;
                float min, max;
                fieldStyle.CalcMinMaxWidth(new GUIContent(field.ToString()), out min, out max);
                if(min != max) { Debug.LogWarning("min and max width were not the same for: \n" + field.ContainingClass.GetName() + "." + field.GetName()); };

                //Debug.Log("For " + field.ToString() + " is " + min + " space needed");

                if (min > result.x)
                {
                    result.x = min;
                }
            }

            result.y += lineTexture.height;

            Label tempLabel = new Label();
            tempLabel.style.unityFont = methodStyle.font;
            tempLabel.style.fontSize = methodStyle.fontSize;
            foreach (MethodData method in data.PublicMethods.Concat(data.PrivateMethods))
            {
                tempLabel.text = method.ToString();

                Vector2 textSize = tempLabel.MeasureTextSize(method.ToString(), 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);

                if (textSize.x > result.x)
                {
                    result.x = textSize.x;
                }
                result.y += methodStyle.lineHeight;
            }

            result.y += emptySpaceBottom;
            result.x += intendation * 2;

            return new Vector2Int((int)result.x, (int)result.y);
        }
    }
}