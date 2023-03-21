using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeExplorinator
{
    public class ClassGUI : IDrawable
    {
        public Vector2 Position { get; set; }
        private static TiledTextureData TiledTextureData
        {
            get
            {
                if (tiledTextureData == null)
                {
                    tiledTextureData = (TiledTextureData)AssetDatabase.LoadAssetAtPath("Assets/Editor/TiledTextures/Class.asset", typeof(TiledTextureData));
                }
                return tiledTextureData;
            }
        }

        private static TiledTextureBuilder TextureBuilder
        {
            get
            {
                if(textureBuilder == null)
                {
                    byte[] fileData = File.ReadAllBytes(Application.dataPath + "/Editor/Graphics/" + TiledTextureData.graphicsPath);
                    Texture2D texture = new Texture2D(1, 1);
                    ImageConversion.LoadImage(texture, fileData);
                    textureBuilder = new TiledTextureBuilder(texture, TiledTextureData.middleRectangle);
                }
                return textureBuilder;
            }
        }


        #region Defines for the graphics
        /// <summary>
        /// Height in pixels of the header for the class
        /// </summary>
        private static int headerHeight = 71;
        /// <summary>
        /// For the lines that seperate methods from fields
        /// </summary>
        private static int lineThickness = 5;
        /// <summary>
        /// the amount of pixels which everything within the box is moved to the right
        /// </summary>
        private static int intendation = 20;
        /// <summary>
        /// How much empty space will be between the last element in the box and the bottom border of the box
        /// </summary>
        private static int emptySpaceBottom = 10;
        #endregion

        private static TiledTextureBuilder textureBuilder;
        private static TiledTextureData tiledTextureData;

        private int widthInPixels;
        private int heightInPixels;
        private GUIStyle classStyle;
        private GUIStyle fieldStyle;
        private GUIStyle methodStyle;
        private ClassData data;
        private Texture2D background;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="classStyle">The style in which the class name will be displayed</param>
        /// <param name="fieldStyle">The style in which fields AND properties will be displayed</param>
        /// <param name="methodStyle">The style in which methods will be displayed</param>
        public ClassGUI(ClassData data, GUIStyle classStyle, GUIStyle fieldStyle, GUIStyle methodStyle)
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

            this.data = data;
            this.classStyle = classStyle;
            this.fieldStyle = fieldStyle;
            this.methodStyle = methodStyle;


            TextureBuilder.Size = CalculateBackgroundSize();
            background = TextureBuilder.BuildTexture();
            widthInPixels = background.width;
            heightInPixels = background.height;
            Debug.LogWarning("GUIStyles für ClassGUI sind noch nicht richtig definiert");
        }

        public void Draw()
        {
            ///<summary>always indicates the next free space that has not been drawn to</summary>
            float yPosition = Position.y;
            Rect drawRect = new Rect(Position.x, yPosition, background.width, background.height);

            EditorGUI.DrawTextureTransparent(drawRect, background);
            #region DrawHeader
            if (data.ClassModifiersList.Count == 0)
            {
                Rect headerRect = new Rect(Position.x, yPosition, background.width, headerHeight);
                //Class name
                EditorGUI.LabelField(headerRect, data.GetName(), classStyle);

            }
            else
            {
                //Modifiers
                Rect headerRect = new Rect(Position.x, yPosition, background.width, headerHeight / 2);
                EditorGUI.LabelField(headerRect, "<<" + data.ClassModifiersAsString + ">>", classStyle);

                //Class name
                drawRect.y += headerHeight / 2;
                EditorGUI.LabelField(headerRect, data.GetName(), classStyle);
            }

            yPosition += headerHeight;
            #endregion

            #region DrawFields
            drawRect.height = methodStyle.lineHeight;
            drawRect.x = intendation + Position.x;
            foreach (FieldData fieldData in data.PublicVariables.Concat(data.PrivateVariables))
            {
                drawRect.y = yPosition;
                EditorGUI.LabelField(drawRect, fieldData.ToString(), methodStyle);

                yPosition += methodStyle.lineHeight;
            }
            #endregion

            Rect line = new Rect(Position.x, yPosition, widthInPixels, lineThickness);
            EditorGUI.DrawRect(line, Color.black);
            yPosition += lineThickness;

            #region DrawMethods
            drawRect.height = methodStyle.lineHeight;
            drawRect.x = intendation + Position.x;
            foreach (MethodData methodData in data.PublicMethods.Concat(data.PrivateMethods))
            {
                drawRect.y = yPosition;
                EditorGUI.LabelField(drawRect, methodData.ToString(), methodStyle);

                yPosition += methodStyle.lineHeight;
            }
            #endregion
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
                
                if(min > result.x)
                {
                    result.x = min;
                }
            }

            foreach (MethodData method in data.PublicMethods.Concat(data.PrivateMethods))
            {
                result.y += methodStyle.lineHeight;
                float min, max;
                methodStyle.CalcMinMaxWidth(new GUIContent(method.ToString()), out min, out max);
                if (min != max) { Debug.LogWarning("min and max width were not the same for: \n" + method.ContainingClass.GetName() + "." + method.GetName()); };

                if (min > result.x)
                {
                    result.x = min;
                }
            }

            result.y += emptySpaceBottom;
            result.x += intendation * 2;

            return new Vector2Int((int)result.x, (int)result.y);
        }
    }
}