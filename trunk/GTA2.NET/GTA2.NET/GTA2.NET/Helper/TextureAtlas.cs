//Created: 28.01.2010

using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using System.Xml.Serialization;
using System.IO;


namespace Hiale.GTA2NET.Helper
{
    /// <summary>
    /// Holds information where certail tiles are put on the image.
    /// </summary>
    [Serializable()]
    public struct TextureAtlas : IDisposable
    {
        private System.Drawing.Image image;
        /// <summary>
        /// Image with all the tiles on it.
        /// </summary>
        [XmlIgnore()]
        public System.Drawing.Image Image
        {
            get { return image; }
        }

        private string imagePath;
        /// <summary>
        /// Path to image file, used by serialization
        /// </summary>
        public string ImagePath
        {
            get { return imagePath; }
            set { imagePath = value; }
        }

        private SerializableDictionary<int, Rectangle> dictionary;

        public SerializableDictionary<int, Rectangle> Dictionary
        {
            get { return dictionary; }
            set { dictionary = value; }
        }

        public TextureAtlas(System.Drawing.Image Image, string ImagePath, SerializableDictionary<int, Rectangle> Dictionary)
        {
            image = Image;
            imagePath = ImagePath;
            dictionary = Dictionary;
        }

        public void Serialize(string path)
        {
            TextWriter textWriter = new StreamWriter(path);
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(this.GetType());
            serializer.Serialize(textWriter, this);
            textWriter.Close();
        }

        public static TextureAtlas Deserialize(string path)
        {
            TextReader textReader = new StreamReader(path);
            XmlSerializer deserializer = new XmlSerializer(typeof(TextureAtlas));            
            TextureAtlas atlas = (TextureAtlas)deserializer.Deserialize(textReader);
            textReader.Close();
            return atlas;
        }

        /// <summary>
        /// Disposes the image when not needed anymore.
        /// </summary>
        public void  Dispose()
        {
            ((System.Drawing.Bitmap)Image).Dispose();
        }

}
}
