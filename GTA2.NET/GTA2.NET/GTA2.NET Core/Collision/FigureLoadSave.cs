// GTA2.NET
// 
// File: FigureLoadSave.cs
// Created: 12.06.2013
// 
// 
// Copyright (C) 2010-2013 Hiale
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software
// and associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
// is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies
// or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR
// IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// Grand Theft Auto (GTA) is a registred trademark of Rockstar Games.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Collision
{
    public class FigureLoadSave : IXmlSerializable
    {
        public static Figure Load(string fileName)
        {
            var deserializer = new XmlSerializer(typeof (Figure));
            TextReader reader = new StreamReader(fileName);
            var figure = (Figure) deserializer.Deserialize(reader);
            reader.Close();
            return figure;
        }

        public void Save(string fileName)
        {
            var settings = new XmlWriterSettings {Indent = true};
            var xmlWriter = XmlWriter.Create(fileName, settings);
            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);
            var serializer = new XmlSerializer(typeof (Figure));
            serializer.Serialize(xmlWriter, this, ns);
            xmlWriter.Close();
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        public void WriteXml(XmlWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    public static class FigureLoadSaveExtensions
    {
        public static void ReadXml(this Figure figure, XmlReader reader)
        {
            var doc = new XmlDocument {PreserveWhitespace = false};
            doc.Load(reader);

            figure.Lines.Clear();
            var lineSegments = doc.GetElementsByTagName("LineSegments");
            if (lineSegments.Count > 0)
            {
                foreach (XmlNode lineSegment in lineSegments[0])
                    figure.Lines.Add((LineSegment) new XmlSerializer(typeof (LineSegment)).Deserialize(XmlReader.Create(new StringReader(lineSegment.OuterXml))));
            }

            //figure.Nodes.Clear();
            //var nodes = doc.GetElementsByTagName("Nodes");
            //if (nodes.Count > 0)
            //{
            //    var xmlReader = XmlReader.Create(new StringReader(nodes[0].OuterXml));
            //    xmlReader.MoveToContent();
            //    Nodes.ReadXml(xmlReader);
            //}

            figure.SwitchPoints.Clear();
            var switchPoints = doc.GetElementsByTagName("SwitchPoints");
            if (switchPoints.Count > 0)
            {
                var xmlReader = XmlReader.Create(new StringReader(switchPoints[0].OuterXml));
                xmlReader.MoveToContent();
                figure.SwitchPoints.ReadXml(xmlReader);
            }

            figure.ForlornStartNodes.Clear();
            var forlornStartNodes = doc.GetElementsByTagName("ForlornStartNodes");
            if (forlornStartNodes.Count > 0)
            {
                foreach (XmlNode forlornStartNode in forlornStartNodes[0])
                    figure.ForlornStartNodes.Add((Vector2) new XmlSerializer(typeof (Vector2)).Deserialize(XmlReader.Create(new StringReader(forlornStartNode.OuterXml))));
            }
        }

        public static void WriteXml(this Figure figure, XmlWriter writer)
        {
            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);

            if (figure.Lines.Count > 0)
            {
                writer.WriteStartElement("LineSegments");
                foreach (var lineSegment in figure.Lines)
                    new XmlSerializer(typeof (LineSegment)).Serialize(writer, lineSegment, ns);
                writer.WriteEndElement();
            }

            //if (Nodes.Count > 0)
            //{
            //    writer.WriteStartElement("Nodes");
            //    Nodes.WriteXml(writer);
            //    writer.WriteEndElement();
            //}

            if (figure.SwitchPoints.Count > 0)
            {
                writer.WriteStartElement("SwitchPoints");
                figure.SwitchPoints.WriteXml(writer);
                writer.WriteEndElement();
            }

            if (figure.ForlornStartNodes.Count > 0)
            {
                writer.WriteStartElement("ForlornStartNodes");
                foreach (var forlornStartNode in figure.ForlornStartNodes)
                    new XmlSerializer(typeof (Vector2)).Serialize(writer, forlornStartNode, ns);
                writer.WriteEndElement();
            }
        }
    }
}
