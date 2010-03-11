//Created: 28.01.2010

using System;
using System.Drawing;
using System.Xml.Serialization;
using System.IO;
using Rectangle=Microsoft.Xna.Framework.Rectangle;


namespace Hiale.GTA2NET.Helper
{
    /// <summary>
    /// Holds information where certail tiles are put on the image.
    /// </summary>
    [Serializable()]
    public class TextureAtlas : IDisposable
    {
        /// <summary>
        /// Image with all the tiles on it.
        /// </summary>
        [XmlIgnore]
        public Image Image { get; private set; }

        /// <summary>
        /// Path to image file, used by serialization
        /// </summary>
        public string ImagePath { get; set; }

        //public SerializableDictionary<SpriteItem, Rectangle> Dictionary { get; set; }

        public TextureAtlas(Image image, string imagePath)
        {
            Image = image;
            ImagePath = imagePath;
        }

        protected TextureAtlas()
        {
            //needed by xml serializer
        }

        public void Serialize(string path)
        {
            TextWriter textWriter = new StreamWriter(path);
            XmlSerializer serializer = new XmlSerializer(GetType());
            serializer.Serialize(textWriter, this);
            textWriter.Close();
        }

        public static TextureAtlas Deserialize(string path, Type type)
        {
            TextReader textReader = new StreamReader(path);
            XmlSerializer deserializer = new XmlSerializer(type);
            TextureAtlas atlas = (TextureAtlas)deserializer.Deserialize(textReader);
            textReader.Close();
            return atlas;
        }

        /// <summary>
        /// Disposes the image when not needed anymore.
        /// </summary>
        public void Dispose()
        {
            Image.Dispose();
        }

    }

    public class TextureAtlasTiles : TextureAtlas
    {
        public SerializableDictionary<int, Rectangle> Dictionary { get; set; }

        public TextureAtlasTiles(Image image, string imagePath, SerializableDictionary<int, Rectangle> dictionary) : base(image, imagePath)
        {
            Dictionary = dictionary;
        }

        private TextureAtlasTiles()
        {
            //needed by xml serializer
        }
    }

    public class TextureAtlasSprites : TextureAtlas
    {
        public SerializableDictionary<SpriteItem, Rectangle> Dictionary { get; set; }

        public TextureAtlasSprites(Image image, string imagePath, SerializableDictionary<SpriteItem, Rectangle> dictionary) : base(image, imagePath)
        {
            Dictionary = dictionary;
        }

        private TextureAtlasSprites()
        {
            //needed by xml serializer
        }

    }
}
