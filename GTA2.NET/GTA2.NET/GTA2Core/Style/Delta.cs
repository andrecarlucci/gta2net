using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hiale.GTA2NET.Core.Style
{
    public class Delta
    {
        private int _sprite;
        /// <summary>
        /// 
        /// </summary>
        public int Sprite
        {
            get { return _sprite; }
            set { _sprite = value; }
        }

        List<int> _deltaSize;
        /// <summary>
        /// 
        /// </summary>
        public List<int> DeltaSize
        {
            get { return _deltaSize; }
            set { _deltaSize = value; }
        }

        public Delta()
        {
            _deltaSize = new List<int>();
        }

        public override string ToString()
        {
            return "Sprite: " + _sprite + " (" + _deltaSize.Count + ")";
        }

    }
}
