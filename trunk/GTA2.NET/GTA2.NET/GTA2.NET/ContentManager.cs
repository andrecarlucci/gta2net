using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Hiale.GTA2NET.Core.Helper;
using Hiale.GTA2NET.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hiale.GTA2NET
{
    public class ContentManager
    {
        public static Texture2D LoadTexture(string textureAtlasFile)
        {
            Texture2D cityTexture;
            Dictionary<int, Rectangle> tileAtlas;

            const string tilesDictPath = "Textures\\tiles.xml";
            TextureAtlasTiles dict;
            if (!File.Exists(tilesDictPath))
            {
                var zip = ZipStorer.Open("Textures\\bil.zip", FileAccess.Read);
                dict = new TextureAtlasTiles("Textures\\tiles.png", zip);
                dict.BuildTextureAtlas();
                dict.Serialize(tilesDictPath);
                tileAtlas = dict.TileDictionary;
                var stream = new MemoryStream();
                dict.Image.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                stream.Position = 0;
                cityTexture = Texture2D.FromStream(BaseGame.Device, stream);
                stream.Close();
                dict.Dispose();
            }
            else
            {
                dict = (TextureAtlasTiles)TextureAtlas.Deserialize(tilesDictPath, typeof(TextureAtlasTiles));
                tileAtlas = dict.TileDictionary;
                var fs = new FileStream(dict.ImagePath, FileMode.Open);
                cityTexture = Texture2D.FromStream(BaseGame.Device, fs);
                fs.Close();
            }

            return cityTexture;
        }
    }
}
