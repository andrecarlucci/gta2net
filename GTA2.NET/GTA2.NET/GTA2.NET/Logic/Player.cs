//Created: 24.02.2010

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hiale.GTA2NET.Logic
{
    public class Player
    {
        private Car _currentCar;

        private Pedestrian _currentPed;

        private bool _inCar;

        private long _points;

        private byte _wantedLevel; //Police level

        private bool _invisible;
    }
}
