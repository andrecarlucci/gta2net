using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hiale.GTA2NET.Core.Style
{
    public class Palette
    {
        private List<ushort> _Indexes;

        public List<ushort> Indexes
        {
            get { return _Indexes; }
            set { _Indexes = value; }
        } 
    }
}
