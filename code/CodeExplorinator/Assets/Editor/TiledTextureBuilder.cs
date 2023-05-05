using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CodeExplorinator
{
    /// <summary>
    /// A wrapper class for a tiled texture. Be aware that resizing it does NOT change the old texture but rather creates a new texture everytime the scale or size is changed.
    /// </summary>
    public class TiledTextureBuilder
    {
        private enum Tile
        {
            TOPLEFT,
            TOP,
            TOPRIGHT,
            LEFT,
            MIDDLE,
            RIGHT,
            BOTTOMLEFT,
            BOTTOM,
            BOTTOMRIGHT
        }

        /// <summary>
        /// The scale of the BackgroundTextureData relative to the original size
        /// </summary>
        public Vector2 Scale
        {
            get { return scale; }
            set
            {
                if(value.x < 1) { value.x = 1; }
                if (value.y < 1) { value.y = 1; }
                Vector2 sizeAsFloat = (Vector2)OriginalSize * value;
                size = new Vector2Int(Mathf.RoundToInt(sizeAsFloat.x), Mathf.RoundToInt(sizeAsFloat.y));
                scale = value;
                mustUpdateTiledTexture = true;
            }
        }

        /// <summary>
        /// The size of the TiledTextureBuilder in pixels
        /// </summary>
        public Vector2Int Size
        {
            get { return size; }
            set
            {
                if (value.x < OriginalSize.x) { value.x = OriginalSize.x; }
                if (value.y < OriginalSize.y) { value.y = OriginalSize.y; }
                scale = new Vector2(value.x / (float)OriginalSize.x, value.y/ (float)OriginalSize.y);
                size = value;
                mustUpdateTiledTexture = true;
            }
        }
        /// <summary>
        /// the size of the underlying texture in pixels
        /// </summary>
        public Vector2Int OriginalSize { get { return new Vector2Int(OriginalTexture.width, OriginalTexture.height); } }
        public Texture2D OriginalTexture { get; private set; }

        private bool mustUpdateTiledTexture = true;
        private Vector2Int size;
        private Vector2 scale;
        private Vector2Int splitPointBottomLeft;
        private Vector2Int splitPointTopRight;
        private Texture2D tiledTexture;
        private Dictionary<Tile, Texture2D> tiles;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="texture2D">the underlying base texture</param>
        /// <param name="middleRectangle">the middle part of the texture in pixels that is going to be repeated if the texture is scaled up. !!CAUTION: (0|0) is in the bottom left corner!! To each side there must be at least one pixel left</param>
        public TiledTextureBuilder(Texture2D texture2D, RectInt middleRectangle)
        {
            OriginalTexture = texture2D;
            splitPointBottomLeft = middleRectangle.position;
            splitPointTopRight = middleRectangle.position + new Vector2Int(middleRectangle.size.x, middleRectangle.size.y);
            try
            {
                tiles = CreateTileDictionary(texture2D, splitPointBottomLeft, splitPointTopRight);
            }
            catch
            {
                throw new Exception($"Error while trying to slice {texture2D.name}. Try checking your middleRectangle if there is one pixel free to every side");
            }
            scale = Vector2.one;
            size = new Vector2Int(texture2D.width, texture2D.height);
        }

        public Texture2D BuildTexture()
        {
            if (mustUpdateTiledTexture)
            {
                UpdateTiledTexture();
            }
            return tiledTexture;
        }

        /// <summary>
        /// Creates a 9-Sliced tile and applies it to the tiledTexture variable. (0|0) is bottom left
        /// </summary>
        private void UpdateTiledTexture()
        {
            Vector2Int splitPointTopRight = Size - (OriginalSize - this.splitPointTopRight);
            mustUpdateTiledTexture = false;
            tiledTexture = new Texture2D(Size.x, Size.y);

            int[] positionX = new int[3]
            {
                0, splitPointBottomLeft.x, splitPointTopRight.x
            };
            int[] positionY = new int[3]
            {
                0, splitPointBottomLeft.y, splitPointTopRight.y
            };
            int[] sizeX = new int[3]
            {
                splitPointBottomLeft.x, -1, Size.x - splitPointTopRight.x
            };
            int[] sizeY = new int[3]
            {
                splitPointBottomLeft.y, -1, Size.y - splitPointTopRight.y
            };
            sizeX[1] = Size.x - sizeX[0] - sizeX[2]; //Size - left - right
            sizeY[1] = Size.y - sizeY[0] - sizeY[2]; //Size - left - right


            //First make the corners because they don't need to scale
            //BottomLeft
            tiledTexture.SetPixels(positionX[0], positionY[0], sizeX[0], sizeY[0], tiles[Tile.BOTTOMLEFT].GetPixels());
            //TopRight
            tiledTexture.SetPixels(positionX[2], positionY[2], sizeX[2], sizeY[2], tiles[Tile.TOPRIGHT].GetPixels());
            //TopLeft
            tiledTexture.SetPixels(positionX[0], positionY[2], sizeX[0], sizeY[2], tiles[Tile.TOPLEFT].GetPixels());
            //BottomRight
            tiledTexture.SetPixels(positionX[2], positionY[0], sizeX[2], sizeY[0], tiles[Tile.BOTTOMRIGHT].GetPixels());

            //Then the edges. they need to scale in one dimension
            //Top
            Texture2D stretchedTop = CreateStretchedTexture(tiles[Tile.TOP], new Vector2Int(sizeX[1], tiles[Tile.TOP].height));
            tiledTexture.SetPixels(positionX[1], positionY[2], sizeX[1], sizeY[2], stretchedTop.GetPixels());

            //Bottom
            Texture2D stretchedBottom = CreateStretchedTexture(tiles[Tile.BOTTOM], new Vector2Int(sizeX[1], tiles[Tile.BOTTOM].height));
            tiledTexture.SetPixels(positionX[1], positionY[0], sizeX[1], sizeY[0], stretchedBottom.GetPixels());

            //Left
            Texture2D stretchedLeft = CreateStretchedTexture(tiles[Tile.LEFT], new Vector2Int(tiles[Tile.LEFT].width, sizeY[1]));
            tiledTexture.SetPixels(positionX[0], positionY[1], sizeX[0], sizeY[1], stretchedLeft.GetPixels());

            //Right
            Texture2D stretchedRight = CreateStretchedTexture(tiles[Tile.RIGHT], new Vector2Int(tiles[Tile.RIGHT].width, sizeY[1]));
            tiledTexture.SetPixels(positionX[2], positionY[1], sizeX[2], sizeY[1], stretchedRight.GetPixels());


            //lastly, the middle which needs to scale in both dimensions
            Texture2D stretchedMiddle = CreateStretchedTexture(tiles[Tile.MIDDLE], new Vector2Int(sizeX[1], sizeY[1]));
            tiledTexture.SetPixels(positionX[1], positionY[1], sizeX[1], sizeY[1], stretchedMiddle.GetPixels());

            tiledTexture.Apply();
        }

        private Texture2D CreateStretchedTexture(Texture2D texture, Vector2Int size)
        {
            RenderTexture renderTexture = new RenderTexture(size.x, size.y, 32);
            Texture2D result = new Texture2D(size.x, size.y);
            RenderTexture oldActive = RenderTexture.active;

            RenderTexture.active = renderTexture;
            Graphics.Blit(texture, renderTexture);
            result.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);
            result.Apply();

            RenderTexture.active = oldActive;
            return result;
        }

        private Dictionary<Tile, Texture2D> CreateTileDictionary(Texture2D texture, Vector2Int splitPointBottomLeft, Vector2Int splitPointTopRight)
        {
            Texture2D tile;
            int width, height;
            Dictionary<Tile, Texture2D> tiles = new Dictionary<Tile, Texture2D>();

            height = splitPointBottomLeft.y;
            width = splitPointBottomLeft.x;
            tile = new Texture2D(width, height);
            tile.SetPixels(texture.GetPixels(0, 0, width, height));
            tile.Apply();
            tiles.Add(Tile.BOTTOMLEFT, tile);

            width = splitPointTopRight.x - splitPointBottomLeft.x;
            tile = new Texture2D(width, height);
            tile.SetPixels(texture.GetPixels(splitPointBottomLeft.x, 0, width, height));
            tile.Apply();
            tiles.Add(Tile.BOTTOM, tile);

            width = texture.width - splitPointTopRight.x;
            tile = new Texture2D(width, height);
            tile.SetPixels(texture.GetPixels(splitPointTopRight.x, 0, width, height));
            tile.Apply();
            tiles.Add(Tile.BOTTOMRIGHT, tile);



            height = splitPointTopRight.y - splitPointBottomLeft.y;
            width = splitPointBottomLeft.x;
            tile = new Texture2D(width, height);
            tile.SetPixels(texture.GetPixels(0, splitPointBottomLeft.y, width, height));
            tile.Apply();
            tiles.Add(Tile.LEFT, tile);

            width = splitPointTopRight.x - splitPointBottomLeft.x;
            tile = new Texture2D(width, height);
            tile.SetPixels(texture.GetPixels(splitPointBottomLeft.x, splitPointBottomLeft.y, width, height));
            tile.Apply();
            tiles.Add(Tile.MIDDLE, tile);

            width = texture.width - splitPointTopRight.x;
            tile = new Texture2D(width, height);
            tile.SetPixels(texture.GetPixels(splitPointTopRight.x, splitPointBottomLeft.y, width, height));
            tile.Apply();
            tiles.Add(Tile.RIGHT, tile);



            height = texture.height - splitPointTopRight.y;
            width = splitPointBottomLeft.x;
            tile = new Texture2D(width, height);
            tile.SetPixels(texture.GetPixels(0, splitPointTopRight.y, width, height));
            tile.Apply();
            tiles.Add(Tile.TOPLEFT, tile);

            width = splitPointTopRight.x - splitPointBottomLeft.x;
            tile = new Texture2D(width, height);
            tile.SetPixels(texture.GetPixels(splitPointBottomLeft.x, splitPointTopRight.y, width, height));
            tile.Apply();
            tiles.Add(Tile.TOP, tile);

            width = texture.width - splitPointTopRight.x;
            tile = new Texture2D(width, height);
            tile.SetPixels(texture.GetPixels(splitPointTopRight.x, splitPointTopRight.y, width, height));
            tile.Apply();
            tiles.Add(Tile.TOPRIGHT, tile);

            return tiles;
        }
    }
}