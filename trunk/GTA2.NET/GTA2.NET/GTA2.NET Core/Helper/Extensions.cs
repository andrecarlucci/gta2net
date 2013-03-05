using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
