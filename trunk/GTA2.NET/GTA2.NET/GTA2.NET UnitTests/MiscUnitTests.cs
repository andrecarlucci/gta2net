using System;
using Hiale.GTA2NET.Core.Collision;
using Hiale.GTA2NET.Core.Helper;
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
        public void TestSubMatrix()
        {
            int[,] one =
                    {
                        {0, 1, 1, 1, 0},
                        {0, 1, 1, 1, 0},
                        {0, 1, 0, 0, 0},
                        {0, 0, 0, 0, -1},
                        {0, 0, 0, 1, 0}
                    };
            SubMatrix.MaxSubmatrix(one, 0);
        }

    }
}
