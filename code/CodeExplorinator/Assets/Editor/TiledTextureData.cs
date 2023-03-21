using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "CodeExplorinator/TiledTextureData")]
public class TiledTextureData : ScriptableObject
{
    [Tooltip("The path to the graphic within the \"Assets/Editor/Graphics/\" folder. Needs the file extension.")]
    public string graphicsPath;

    [Tooltip("The middle rectangle of the tiled Texture that will be stretched in both directions in order to fit the dimension.")]
    public RectInt middleRectangle;

}
