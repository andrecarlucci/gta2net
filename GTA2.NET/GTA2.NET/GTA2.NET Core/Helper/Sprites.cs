using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Hiale.GTA2NET.Core.Style;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Helper
{
    /// <summary>
    /// Responsible for the access to the Sprites.
    /// </summary>
    public class Sprites
    {
        private Dictionary<int, SpriteItem> tileAtlas;
        private float pixelPerWidth;
        private float pixelPerHeight;

        /// <summary>
        /// Creates a new instance of Sprites.
        /// </summary>
        public Sprites()
        {
            String path = Globals.GraphicsSubDir + Path.DirectorySeparatorChar + Globals.SpritesSuffix;
            TextureAtlasSprites dict = TextureAtlas.Deserialize<TextureAtlasSprites>(path + Globals.XmlFormat);
            tileAtlas = dict.SpriteDictionary;

            Bitmap bmp = (Bitmap)Image.FromFile(path + Globals.TextureImageFormat);
            pixelPerHeight = 1f / bmp.Height;
            pixelPerWidth = 1f / bmp.Width;
        }

        /// <summary>
        /// Gets a sprite from a position
        /// </summary>
        /// <param name="spriteID">The position of the sprite to get</param>
        /// <returns>A array with 4 positions where each position represent one of the vertices's of the sprite</returns>
        public Vector2[] GetSprite(UInt32 spriteID)
        {
            Vector2[] texturePosition = new Vector2[4];
            CompactRectangle tex = tileAtlas[(int)spriteID].Rectangle;

            texturePosition[3] = new Vector2((tex.X + 1) * pixelPerWidth, (tex.Y + 1) * pixelPerHeight);
            texturePosition[2] = new Vector2((tex.X + tex.Width - 1) * pixelPerWidth, (tex.Y + 1) * pixelPerHeight);
            texturePosition[0] = new Vector2((tex.X + 1) * pixelPerWidth, (tex.Y + tex.Height - 1) * pixelPerHeight);
            texturePosition[1] = new Vector2((tex.X + tex.Width - 1) * pixelPerWidth, (tex.Y + tex.Height - 1) * pixelPerHeight);

            return texturePosition;
        }
    }
}
