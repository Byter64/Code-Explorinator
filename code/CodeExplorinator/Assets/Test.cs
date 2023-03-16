using CodeExplorinator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField]
    private Vector2Int size;

    [SerializeField]
    private Texture2D original;

    private TiledTextureBuilder tiled;
    private Vector2Int actualsize;
    void Start()
    {
        tiled = new TiledTextureBuilder(original, new RectInt(16, 16, 32, 16));

        GetComponent<SpriteRenderer>().sprite = Sprite.Create(tiled.BuildTexture(), new Rect(0, 0, tiled.Size.x, tiled.Size.y), Vector2.zero);
    }

    // Update is called once per frame
    void Update()
    {
        if(actualsize != size)
        {
            actualsize = size;
            tiled.Size = actualsize;
            GetComponent<SpriteRenderer>().sprite = Sprite.Create(tiled.BuildTexture(), new Rect(0, 0, tiled.Size.x, tiled.Size.y), Vector2.zero);
        }
    }
}
