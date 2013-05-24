// GTA2.NET
// 
// File: Remap.cs
// Created: 01.05.2013
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
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Hiale.GTA2NET.Core.Style
{
    [Serializable]
    [XmlInclude(typeof(PedestrianRemap))]
    public class Remap
    {
        public int Key { get; set; }
        public int Palette { get; set; }

        public Remap()
        {
            //XML Serializer
        }

        public Remap(int key, int palette)
        {
            Key = key;
            Palette = palette;
        }

        public override string ToString()
        {
            return Key.ToString(CultureInfo.InvariantCulture);
        }
    }

    public class PedestrianRemap : Remap
    {
        public string Description { get; set; }

        public PedestrianRemap() :base()
        {
            //XML Serializer
        }

        public PedestrianRemap(int key, int palette) : base(key, palette)
        {
            Description = GetDescription(key);
        }

        private static string GetDescription(int key)
        {
            //Values from http://projectcerbera.com/gta/2/tutorials/characters
            switch (key)
            {
                case 0:
                    return "Blue Police";
                case 1:
                    return "Green Police";
                case 2:
                    return "Red Police";
                case 3:
                    return "Yellow Police";
                case 4:
                    return "Army";
                case 5:
                    return "Red hair Redneck";
                case 6:
                    return "Blonde hair Redneck";
                case 7:
                    return "Scientist";
                case 8:
                    return "Zaibatsu";
                case 9:
                    return "Krishna";
                case 10:
                    return "Russian";
                case 11:
                    return "Loony";
                case 12:
                    return "Elvis";
                case 13:
                    return "Yakuza";
                case 14:
                    return "Fireman";
                case 15:
                    return "Green Shorts Ped";
                case 16:
                    return "Medic";
                case 17:
                    return "Mugger Remap";
                case 18:
                    return "Blue Dummy Remap";
                case 19:
                    return "Light Blue Dummy Remap";
                case 20:
                    return "T-Shirt & Shorts Dummy Remap";
                case 21:
                    return "Short Sleeved Shirt & Trousers Dummy Remap";
                case 22:
                    return "Prison Uniform";
                case 23:
                    return "Hulk (Normal)";
                case 24:
                    return "Hulk (Green)";
                case 25:
                    return "Default Player Ped";
                case 26:
                    return "Naked Dummy";
                case 27:
                    return "Blue shirt and jeans";
                case 28:
                    return "White shirt and jeans";
                case 29:
                    return "Lilac shirt and jeans";
                case 30:
                    return "Light red shirt and jeans";
                case 31:
                    return "Light red shirt and khaki shorts";
                case 32:
                    return "Blue shirt and blue shorts";
                case 33:
                    return "Yellow shirt and khaki shorts";
                case 34:
                    return "Light purple shirt and jeans";
                case 35:
                    return "Dark red shirt and turquoise trousers";
                case 36:
                    return "Dark green shirt and dark green trousers";
                case 37:
                    return "Brown shirt and jeans";
                case 38:
                    return "Brown shirt and brown trousers";
                case 39:
                    return "Dark purple shirt and brown trousers";
                case 40:
                    return "Dark purple shirt and light brown trousers";
                case 41:
                    return "Burgundy shirt and light brown trousers";
                case 42:
                    return "Short sleeve burgundy shirt and light brown trousers";
                case 43:
                    return "Short sleeve light burgundy shirt and light brown trousers";
                case 44:
                    return "Aqua shirt and jeans";
                case 45:
                    return "Yellow shirt and jeans";
                case 46:
                    return "Short sleeve dark green shirt and lilac trousers";
                case 47:
                    return "Light grey shirt and dark grey jeans";
                case 48:
                    return "Short sleeve grey/blue shirt and drak grey jeans";
                case 49:
                    return "Short sleeve light purple shirt and light grey trousers";
                case 50:
                    return "Purple shirt with light purple sleeves and light grey trousers";
                case 51:
                    return "Grey/green shirt and grey trousers";
                case 52:
                    return "Pink shirt and jeans";
            }
            return string.Empty;
        }

        public override string ToString()
        {
            return Description;
        }
    }
}
