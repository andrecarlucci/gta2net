// GTA2.NET
// 
// File: VersionAttribute.cs
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
using System.Reflection;

namespace Hiale.GTA2NET.Core.Helper
{
   [AttributeUsage(AttributeTargets.Class)]
   public class VersionAttribute : Attribute
   {
      private readonly Version _version;

      public Version Version
      {
         get { return _version; }
      }

      public VersionAttribute(int major, int minor, int build, int revision)
      {
         _version = new Version(major, minor, build, revision);
      }

      public VersionAttribute(int major, int minor, int build)
      {
         _version = new Version(major, minor, build);
      }

      public VersionAttribute(int major, int minor)
      {
         _version = new Version(major, minor);
      }

      public VersionAttribute(string version)
      {
         _version = new Version(version);
      }
   }

   [AttributeUsage(AttributeTargets.Class)]
   public class NamedVersionAttribute : VersionAttribute
   {
      private readonly string _name;

      public string Name
      {
         get { return _name; }
      }

      public NamedVersionAttribute(string name, int major, int minor, int build, int revision) : base(major, minor, build, revision)
      {
         _name = name;
      }

      public NamedVersionAttribute(string name, int major, int minor, int build) : base(major, minor, build)
      {
         _name = name;
      }

      public NamedVersionAttribute(string name, int major, int minor) : base(major, minor)
      {
         _name = name;
      }

      public NamedVersionAttribute(string name, string version) : base(version)
      {
         _name = name;
      }

      public static IDictionary<string, Version> GetTypesWithVersionAttribute(Assembly assembly)
      {
         var versionDict = new Dictionary<string, Version>();
         foreach (var type in assembly.GetTypes())
         {
            var attributes = type.GetCustomAttributes(typeof(VersionAttribute), true);
            if (attributes.Length == 0)
               continue;
            var versionAttribute = (VersionAttribute) attributes[0];
            var namedVersionAttribute = versionAttribute as NamedVersionAttribute;
            if (namedVersionAttribute != null)
            {
               versionDict.Add(namedVersionAttribute.Name, namedVersionAttribute.Version);
            }
            else
            {
                if (type.AssemblyQualifiedName != null)
                    versionDict.Add(type.AssemblyQualifiedName, versionAttribute.Version);
            }
         }
         return versionDict;
      }
   }

}
