// GTA2.NET
// 
// File: Extensions.cs
// Created: 04.03.2013
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
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Hiale.GTA2NET.Core.Helper
{
    public static class Extensions
    {
//if(reallyLongIntegerVariableName == 1 || 
//  reallyLongIntegerVariableName == 6 || 
//  reallyLongIntegerVariableName == 9 || 
//  reallyLongIntegerVariableName == 11)
//{
//  // do something....
//}
// -->
        //if(reallyLongIntegerVariableName.EqualsAnyOf(1,6,9,11))
//      {
//      // do something....
//      }

        public static bool EqualsAnyOf<T>(this T source, params T[] list)
        {
            if (null == source)
                throw new ArgumentNullException("source");
            return list.Contains(source);
        }

        /// <summary>
        /// Checks whether a path contains a separator "\" at the end. If not, it got added.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string CheckDirectorySeparator(string path)
        {
            return !path.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)) ? path.Insert(path.Length, Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)) : path;
        }

        public static string GetDescription(this Enum value)
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);
            if (name != null)
            {
                var field = type.GetField(name);
                if (field != null)
                {
                    var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attribute != null)
                        return attribute.Description;
                }
            }
            return value.ToString();
        }

        public static bool FilesAreEqual(string path1, string path2)
        {
            if (!File.Exists(path1) || !File.Exists(path2))
                throw new FileNotFoundException();
            var info1 = new FileInfo(path1);
            var info2 = new FileInfo(path2);
            if (info1.Length != info2.Length)
                return false;
            var hashAlgorithm = MD5.Create();
            var hash1 = hashAlgorithm.ComputeHash(info1.OpenRead());
            var hash2 = hashAlgorithm.ComputeHash(info2.OpenRead());
            if (hash1.Where((t, i) => t != hash2[i]).Any())
                return false;
            return true;
        }
    }
}
