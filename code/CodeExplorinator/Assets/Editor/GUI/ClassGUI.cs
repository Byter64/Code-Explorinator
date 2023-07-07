using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static CodeExplorinator.Color;
using System.Collections.Generic;

namespace CodeExplorinator
{
    [System.Serializable]
    public class ClassGUI : BaseGUI
    {
        public string ClassName { get; private set; }
        public string ClassModifiers { get; private set; }
        public Vector2 Position { get; set; }
        public List<MethodGUI> methodGUIs { get; private set; }

        private const string classTexturePath = "Assets/Editor/TiledTextures/Class.asset";
        private const string headerTexturePath = "Assets/Editor/TiledTextures/Header.asset";
        private const string focusedClassTexturePath = "Assets/Editor/TiledTextures/FocusedClass.asset";
        private const string focusedHeaderTexturePath = "Assets/Editor/TiledTextures/FocusedHeader.asset";

        #region Defines for the graphics
        /// <summary>
        /// Height in pixels of the header for the class
        /// </summary>
        private const int headerHeight = 88;
        /// <summary>
        /// the amount of pixels which everything within the box is moved to the right
        /// </summary>
        private const int intendation = 20;
        /// <summary>
        /// How much empty space will be between the last element in the box and the bottom border of the box
        /// </summary>
        private const int emptySpaceBottom = 10;
        #endregion

        private static TiledTextureBuilder backgroundBuilder;
        private static TiledTextureBuilder headerBuilder;

        private bool isExpanded;
        private bool isVisible;
        private int widthInPixels;
        private int heightInPixels;
        private Vector2 posOnStartMoving;
        private Vector2 mousePosOnStartMoving;
        private GUIStyle classStyle;
        private GUIStyle fieldStyle;
        private GUIStyle methodStyle;
        private ClassData data;
        private Texture2D backgroundTexture;
        private Texture2D headerTexture;
        private Texture2D focusedBackgroundTexture;
        private Texture2D focusedHeaderTexture;
        private Texture2D lineTexture;
        private VisualElement header;
        private VisualElement moveCollider;
        private ClickBehaviour bodyClick;
        private ClickBehaviour headerClick;
        private HashSet<ConnectionGUI> connections;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="classStyle">The style in which the class name will be displayed</param>
        /// <param name="fieldStyle">The style in which fields AND properties will be displayed</param>
        /// <param name="methodStyle">The style in which methods will be displayed</param>
        public ClassGUI(Vector2 position, ClassData data, GUIStyle classStyle, GUIStyle fieldStyle, GUIStyle methodStyle, GraphManager graphManager) :
            base(graphManager)
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
            
            lineTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Graphics/Linetexture.png");
            ClassModifiers = "<<" + data.ClassModifiersAsString + ">>";
            ClassName = data.GetName();
            methodGUIs = new List<MethodGUI>();
            connections = new HashSet<ConnectionGUI>();
            foreach (MethodData methodData in data.PublicMethods.Concat(data.PrivateMethods))
            {
                MethodGUI methodGUI = new MethodGUI(methodData, methodStyle, graphManager);
                methodGUIs.Add(methodGUI);
            }

            backgroundTexture = Create9SlicedTexture(classTexturePath, CalculateBackgroundSize());
            focusedBackgroundTexture = Create9SlicedTexture(focusedClassTexturePath, CalculateBackgroundSize());
            widthInPixels = backgroundTexture.width;
            heightInPixels = backgroundTexture.height;
            headerTexture = Create9SlicedTexture(headerTexturePath, new Vector2Int(widthInPixels, 0));
            focusedHeaderTexture = Create9SlicedTexture(focusedHeaderTexturePath, new Vector2Int(widthInPixels, 0));
        }

        public override void GenerateVisualElement()
        {
            VisualElement classElement = new VisualElement();
            classElement.style.backgroundImage = Background.FromTexture2D(backgroundTexture);
            classElement.style.backgroundSize = new StyleBackgroundSize(new BackgroundSize(widthInPixels, heightInPixels));
            classElement.style.height = heightInPixels;
            classElement.style.width = widthInPixels;
            classElement.style.position = new StyleEnum<Position>(UnityEngine.UIElements.Position.Absolute);
            classElement.style.alignContent = new StyleEnum<Align>(Align.Stretch);
            classElement.style.marginLeft = Position.x;
            classElement.style.marginTop = Position.y;

            #region DrawHeader
            string headerText = ColorText(ClassModifiers + "\n" + ClassName, className);
            Label header = new Label(headerText);
            header.style.backgroundImage = Background.FromTexture2D(headerTexture);
            header.style.backgroundSize = new StyleBackgroundSize(new BackgroundSize(headerTexture.width, headerTexture.height));
            header.style.unityFont = new StyleFont(classStyle.font);
            header.style.unityFontDefinition = new StyleFontDefinition(classStyle.font);
            header.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.UpperCenter);
            header.style.height = headerTexture.height;
            header.style.fontSize = classStyle.fontSize;
            header.style.color = UnityEngine.Color.black;
            this.header = header;

            classElement.Add(header);
            #endregion

            #region DrawFieldsAndProperties
            VisualElement publicVariables = new VisualElement();
            publicVariables.style.paddingLeft = intendation;
            classElement.Add(publicVariables);
            
            VisualElement privateVariables = new VisualElement();
            privateVariables.style.paddingLeft = intendation;
            classElement.Add(privateVariables);

            foreach (FieldData fieldData in data.PublicVariables)
            {
                Label field = new Label(fieldData.ToRichString());
                field.style.unityFont = new StyleFont(fieldStyle.font);
                header.style.unityFontDefinition = new StyleFontDefinition(fieldStyle.font);
                field.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.UpperLeft);
                field.style.fontSize = fieldStyle.fontSize;
                field.style.color = UnityEngine.Color.black;

                publicVariables.Add(field);
            }
            
            foreach (PropertyData propertyData in data.PublicProperties)
            {
                Label property = new Label(propertyData.ToRichString());
                property.style.unityFont = new StyleFont(fieldStyle.font);
                property.style.unityFontDefinition = new StyleFontDefinition(fieldStyle.font);
                property.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.UpperLeft);
                property.style.fontSize = fieldStyle.fontSize;
                property.style.color = UnityEngine.Color.black;

                publicVariables.Add(property);
            }
            
            foreach (FieldData fieldData in data.PrivateVariables)
            {
                Label field = new Label(fieldData.ToRichString());
                field.style.unityFont = new StyleFont(fieldStyle.font);
                header.style.unityFontDefinition = new StyleFontDefinition(fieldStyle.font);
                field.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.UpperLeft);
                field.style.fontSize = fieldStyle.fontSize;
                field.style.color = UnityEngine.Color.black;

                privateVariables.Add(field);
            }

            foreach (PropertyData propertyData in data.PrivateProperties)
            {
                Label property = new Label(propertyData.ToRichString());
                property.style.unityFont = new StyleFont(fieldStyle.font);
                property.style.unityFontDefinition = new StyleFontDefinition(fieldStyle.font);
                property.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.UpperLeft);
                property.style.fontSize = fieldStyle.fontSize;
                property.style.color = UnityEngine.Color.black;

                privateVariables.Add(property);
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
            foreach (MethodGUI methodGUI in methodGUIs)
            {
                methods.Add(methodGUI.VisualElement);
            }
            #endregion

            VisualElement = classElement;
            TryAssignClickBehaviours();
        }

        public override void SetVisible(bool isVisible)
        {
            this.isVisible = isVisible;

            bool isBodyVisible = isExpanded && isVisible;

            VisualElement.visible = isBodyVisible;
            header.visible = isVisible;
            foreach (MethodGUI methodGUI in methodGUIs)
            {
                methodGUI.SetVisible(isBodyVisible);
            }
        }

        public void SetFocused(bool isFocused)
        {
            if(isFocused)
            {
                VisualElement.style.backgroundImage = Background.FromTexture2D(focusedBackgroundTexture);
                header.style.backgroundImage = Background.FromTexture2D(focusedHeaderTexture);
            }
            else
            {
                VisualElement.style.backgroundImage = Background.FromTexture2D(backgroundTexture);
                header.style.backgroundImage = Background.FromTexture2D(headerTexture);
            }
        }

        public void SetIsExpanded(bool isExpanded)
        {
            this.isExpanded = isExpanded;
            SetVisible(isVisible);
        }

        private void TryAssignClickBehaviours()
        {
            if (bodyClick == null)
            {
                bodyClick = new ClickBehaviour(VisualElement, null, SetFocusClass);
                bodyClick.RegisterOnControlMonoClick(AddClassToSelected);
                bodyClick.RegisterOnHoldingClick(Move);
            }

            if (headerClick == null)
            {
                headerClick = new ClickBehaviour(header, SwapIsExpanded, SetFocusClass);
            }
        }

        private void SwapIsExpanded()
        {
            SetIsExpanded(!isExpanded);
        }
        private void SetFocusClass()
        {
            graphManager.AddSelectedClass(data.ClassNode);
            graphManager.AdjustGraphToSelectedClasses();
            graphManager.ChangeToClassLayer();
        }

        private void AddClassToSelected()
        {
            graphManager.AddSelectedClass(data.ClassNode);
        }

        private void Move(bool isFirstCall, bool isLastCall, float x, float y)
        {
            //if(isFirstCall)
            //{
            //    VisualElement.BringToFront();
            //    connections = graphManager.FindAllConnections(this);
            //    moveCollider = new VisualElement();
            //    moveCollider.style.height = new StyleLength(float.MaxValue);
            //    moveCollider.style.width = new StyleLength(float.MaxValue);
            //    moveCollider.style.marginLeft = new StyleLength(-0x7FFFF);
            //    moveCollider.style.marginTop = new StyleLength(-0x7FFFF);
            //    moveCollider.style.position = new StyleEnum<Position>(UnityEngine.UIElements.Position.Absolute);
            //    VisualElement.Add(moveCollider);
            //    posOnStartMoving = new Vector2(VisualElement.style.marginLeft.value.value, VisualElement.style.marginTop.value.value);
            //    mousePosOnStartMoving = new Vector2(x, y); 

            //}
            //else if(isLastCall)
            //{
            //    VisualElement.Remove(moveCollider);
            //}
            //else
            //{
            //    Vector2 delta = new Vector2(x, y) - mousePosOnStartMoving;
            //    VisualElement.style.marginLeft = posOnStartMoving.x + delta.x * 1/CodeExplorinatorGUI.Scale.x;
            //    VisualElement.style.marginTop = posOnStartMoving.y + delta.y * 1/CodeExplorinatorGUI.Scale.y;
            //}
        }

        private Vector2Int CalculateBackgroundSize()
        {
            Vector2 result = Vector2.zero;

            result.y += headerHeight;
            const float lineSpace = 1.5f; //Measuered by hand. May need to be adjusted when changing font, or font size.

            foreach(FieldData field in data.PublicVariables.Concat(data.PrivateVariables))
            {
                result.y += Mathf.Ceil(fieldStyle.lineHeight) + lineSpace;
                float min, max;
                fieldStyle.CalcMinMaxWidth(new GUIContent(field.ToString()), out min, out max);

                if (min > result.x)
                {
                    result.x = min;
                }
            }

            foreach (PropertyData property in data.PublicProperties.Concat(data.PrivateProperties))
            {
                result.y += Mathf.Ceil(fieldStyle.lineHeight) + lineSpace;
                float min, max;
                fieldStyle.CalcMinMaxWidth(new GUIContent(property.ToString()), out min, out max);

                //Debug.Log("For " + property.ToString() + " is " + min + " space needed");

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
                result.y += Mathf.Ceil(methodStyle.lineHeight) + lineSpace;
            }

            result.y += emptySpaceBottom;
            result.x += intendation * 2;

            return new Vector2Int((int)result.x, (int)result.y);
        }
    }
}