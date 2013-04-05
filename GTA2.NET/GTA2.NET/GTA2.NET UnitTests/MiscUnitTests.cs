// GTA2.NET
// 
// File: MiscUnitTests.cs
// Created: 01.04.2013
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
using System.IO;
using System.IO.Pipes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using Hiale.GTA2NET.Core.Collision;
using Hiale.GTA2NET.Core.Helper;
using Hiale.GTA2NET.Core.Map;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hiale.GTA2NET.Test
{
    [TestClass]
    public class MiscUnitTests
    {
        [TestInitialize]
        public void InitializeTests()
        {
            System.Environment.CurrentDirectory = "..\\..\\..\\GTA2.NET\\bin\\Debug\\";
        }

        [TestMethod]
        public void TestBitHelper()
        {
            var block = new BlockInfo();
            block.ParseSlope(31);
            block.Lid = new BlockFaceLid(16906);
            var stream = new MemoryStream();
            block.Save(new BinaryWriter(stream));

            //const int testInt = 683795;
            //var r1 = BitHelper.CheckBit(testInt, 0);
            //var r2 = BitHelper.CheckBit(testInt, 30);

            //int r3 = BitHelper.SetBit(testInt, 0, true);
            //r3 = BitHelper.SetBit(r3, 0, false);
            //r3 = BitHelper.SetBit(r3, 30, false);
            //r3 = BitHelper.SetBit(r3, 0, true);
            //Assert.AreEqual(testInt, r3);
        }

        [TestMethod]
        public void TestFarseerJson()
        {
            //World world = JsonWorldSerialization.Deserialize("racetrack.json");

        }

    }
}
