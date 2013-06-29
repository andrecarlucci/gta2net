// GTA2.NET
// 
// File: MapUnitTests.cs
// Created: 09.03.2013
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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Hiale.GTA2NET.Core;
using Hiale.GTA2NET.Core.Collision;
using Hiale.GTA2NET.Core.Helper;
using Hiale.GTA2NET.Core.Map;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Color = Microsoft.Xna.Framework.Color;

namespace Hiale.GTA2NET.Test
{
    [TestClass]
    public class MapUnitTests
    {
        [TestInitialize]
        public void InitializeTests()
        {
            System.Environment.CurrentDirectory = "..\\..\\..\\GTA2.NET\\bin\\Debug\\";
        }

        //[TestMethod]
        //public void LoadMapTinyTown()
        //{
        //    var map = new Map(Globals.MapsSubDir + "\\MP1-comp.gmp");
        //    var collision = new MapCollision(map);
        //    for (var i = 0; i < 7; i++)
        //    {
        //        var obstacles = collision.GetObstacles(i);
        //        //DisplayCollision(obstacles);
        //    }
        //    Assert.AreEqual(true, true);
        //}

        //[TestMethod]
        //public void LoadMapBil()
        //{
        //    var map = new Map(Globals.MapsSubDir + "\\bil.gmp");
        //    //var collision = new MapCollisionOld(map);
        //    //var obstacles = collision.CollisionMap(new Vector2(239, 192));
        //    //DisplayCollision(obstacles);
        //    Assert.AreEqual(true, true);
        //}

        //[TestMethod]
        //public void SaveMapTinyTown()
        //{
        //    var map = new Map(Globals.MapsSubDir + "\\MP1-comp.gmp");
        //    map.Save("data\\TinyTown.gta2map");
        //    Assert.AreEqual(true, true);
        //}

        //[TestMethod]
        //public void LoadMapTinyTownInternalFormat()
        //{
        //    SaveMapTinyTown();
        //    var map = new Map();
        //    map.Load(Globals.MapsSubDir + "\\TinyTown.gta2map");
        //    Assert.AreEqual(true, true);
        //}


    }
}
