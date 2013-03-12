using System;
#if WINRT
using System.Runtime.Serialization;
#endif

namespace Hiale.GTA2NET.Core.Helper
{
    #if WINRT
    [DataContract]
    #else
    [Serializable]
    #endif
    public struct IntVector2 : IEquatable<IntVector2> //ToDo: Replace Vector2 by IntVector2
    {
        #region Private Fields

        private static readonly IntVector2 zero = new IntVector2(0, 0);
        private static readonly IntVector2 unit = new IntVector2(1, 1);
        private static readonly IntVector2 unitX = new IntVector2(1, 0);
        private static readonly IntVector2 unitY = new IntVector2(0, 1);

        #endregion Private Fields


        #region Public Fields
        #if WINRT
        [DataMember]
        #endif
        public int X;
        #if WINRT
        [DataMember]
        #endif
        public int Y;

        #endregion Public Fields


        #region Properties

        public static IntVector2 Zero
        {
            get { return zero; }
        }

        public static IntVector2 One
        {
            get { return unit; }
        }

        public static IntVector2 UnitX
        {
            get { return unitX; }
        }

        public static IntVector2 UnitY
        {
            get { return unitY; }
        }

        #endregion Properties


        #region Constructors

        public IntVector2(int x, int y)
        {
            X = x;
            Y = y;
        }

        public IntVector2(int value)
        {
            X = value;
            Y = value;
        }

        #endregion Constructors

        //ToDo: add more methods
        #region Public Methods

        public override bool Equals(object obj)
        {
            if (obj is IntVector2)
                return Equals((IntVector2) obj);
            return false;
        }

        public bool Equals(IntVector2 other)
        {
            return (X == other.X) && (Y == other.Y);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode();
        }

        #endregion

        #region Operators

        public static IntVector2 operator -(IntVector2 value)
        {
            value.X = -value.X;
            value.Y = -value.Y;
            return value;
        }


        public static bool operator ==(IntVector2 value1, IntVector2 value2)
        {
            return value1.X == value2.X && value1.Y == value2.Y;
        }


        public static bool operator !=(IntVector2 value1, IntVector2 value2)
        {
            return value1.X != value2.X || value1.Y != value2.Y;
        }


        public static IntVector2 operator +(IntVector2 value1, IntVector2 value2)
        {
            value1.X += value2.X;
            value1.Y += value2.Y;
            return value1;
        }


        public static IntVector2 operator -(IntVector2 value1, IntVector2 value2)
        {
            value1.X -= value2.X;
            value1.Y -= value2.Y;
            return value1;
        }


        public static IntVector2 operator *(IntVector2 value1, IntVector2 value2)
        {
            value1.X *= value2.X;
            value1.Y *= value2.Y;
            return value1;
        }


        public static IntVector2 operator *(IntVector2 value, int scaleFactor)
        {
            value.X *= scaleFactor;
            value.Y *= scaleFactor;
            return value;
        }


        public static IntVector2 operator *(int scaleFactor, IntVector2 value)
        {
            value.X *= scaleFactor;
            value.Y *= scaleFactor;
            return value;
        }

        #endregion Operators
    }
}
