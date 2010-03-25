//Created: 15.03.2010

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hiale.GTA2NET.Helper
{
    public static class RandomHelper
    {
        private static Random _trueRandom;

        public static Random TrueRandom
        {
            get
            {
                if (_trueRandom == null)
                    _trueRandom = new Random();
                return _trueRandom;
            }
            private set { _trueRandom = value; }
        }

        private static int _seed = 924826379;
        public static int Seed
        {
            get { return _seed; }
            private set { _seed = value; }
        }

        private static Random _fakeRandom;
        public static Random FakeRandom
        {
            get
            {
                if (_fakeRandom == null)
                    _fakeRandom = new Random(Seed);
                return _fakeRandom;
            }
            private set { _fakeRandom = value; }
        }

        /// <summary>
        /// Get random int
        /// </summary>
        /// <param name="trueRandom">ToDo</param>
        /// <param name="max">Maximum</param>
        /// <returns>Int</returns>
        public static int GetRandomInt(bool trueRandom, int max)
        {
            return trueRandom ? TrueRandom.Next(max) : FakeRandom.Next(max);
        }

        /// <summary>
        /// Get random float between min and max
        /// </summary>
        /// <param name="trueRandom">ToDo</param>
        /// <param name="min">Min</param>
        /// <param name="max">Max</param>
        /// <returns>Float</returns>
        public static float GetRandomFloat(bool trueRandom, float min, float max)
        {
            float formula = (max - min) + min;
            return trueRandom ? (float) TrueRandom.NextDouble() * formula : (float) FakeRandom.NextDouble() * formula;
        }
    }
}