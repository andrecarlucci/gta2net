// GTA2.NET
// 
// File: IndexFile.cs
// Created: 18.07.2013
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
using System.Reflection;

namespace Hiale.GTA2NET.Core.Helper
{
    public class IndexFile
    {
        private const string IndexFileVersion = "0.1";

        public static void Save()
        {
            var versionAttributes = NamedVersionAttribute.GetTypesWithVersionAttribute(Assembly.GetExecutingAssembly());
            using (var writer = new BinaryWriter(new FileStream(Globals.IndexFile, FileMode.CreateNew, FileAccess.ReadWrite)))
            {
                writer.Write("GTA2.NET");
                writer.Write(IndexFileVersion); //Index-File version
                writer.Write(versionAttributes.Count);
                foreach (var versionAttribute in versionAttributes)
                {
                    writer.Write(versionAttribute.Key);
                    writer.Write(versionAttribute.Value.ToString());
                }
                
            }
            
        }

        //true = all files are ok, no need to update
        public static bool Vertify()
        {
            try
            {
                using (var reader = new BinaryReader(new FileStream(Globals.IndexFile, FileMode.Open, FileAccess.Read)))
                {
                    reader.ReadString(); //GTA2.NET
                    var fileVersion = new Version(reader.ReadString());
                    var localVersion = new Version(IndexFileVersion);
                    if (fileVersion < localVersion)
                        return false;
                    var localVersionAttributes = NamedVersionAttribute.GetTypesWithVersionAttribute(Assembly.GetExecutingAssembly());
                    var fileVersionAttributes = new Dictionary<string, Version>();
                    var count = reader.ReadInt32();
                    for (var i = 0; i < count; i++)
                    {
                        fileVersionAttributes.Add(reader.ReadString(), new Version(reader.ReadString()));
                    }
                    foreach (var localVersionAttribute in localVersionAttributes)
                    {
                        Version version;
                        if (!fileVersionAttributes.TryGetValue(localVersionAttribute.Key, out version))
                            return false;
                        if (version < localVersionAttribute.Value)
                            return false;
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return false;
            }
        }
    }
}
