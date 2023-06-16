using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CodeExplorinator
{
    public abstract class BaseGUI
    {
        public VisualElement VisualElement { get; protected set; }

        protected GraphManager graphManager;

        protected BaseGUI(GraphManager graphManager)
        {
            this.graphManager = graphManager;
        }

        public abstract void GenerateVisualElement();
        public abstract void SetVisible(bool isVisible);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetPath">The path to the TiledTextureData asset relative to the Assets folder. e.g.: "Assets/Editor/ckass.asset"</param>
        /// <returns></returns>
        protected static Texture2D Create9SlicedTexture(string assetPath, Vector2Int size)
        {
            TiledTextureData backgroundData = (TiledTextureData)AssetDatabase.LoadAssetAtPath(assetPath, typeof(TiledTextureData));
            byte[] fileData = File.ReadAllBytes(Application.dataPath + "/Editor/Graphics/" + backgroundData.graphicsPath);
            Texture2D texture = new Texture2D(1, 1);
            ImageConversion.LoadImage(texture, fileData);
            return TiledTextureBuilder.NineSliceScaleTexture(texture, backgroundData.middleRectangle, size);
        }
    }
}