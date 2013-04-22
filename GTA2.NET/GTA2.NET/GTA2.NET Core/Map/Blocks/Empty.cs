using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Map.Blocks
{
    class Empty : BlockInfo
    {
        public Empty(blockInfo blockInfo, Vector3 pos):base(blockInfo, pos)
        {
            this.SlopeType = Core.Map.SlopeType.Empty;
        }

        public Empty()
            : base()
        {
            this.SlopeType = Core.Map.SlopeType.Empty;
        }
        public Empty(Vector3 pos)
            : base()
        {
            this.SlopeType = Core.Map.SlopeType.Empty;
            this.Position = pos;
        }

        public override BlockInfo DeepCopy()
        {
            return new Empty(this.blockInfo, this.Position);
        }

        public override BlockInfo DeepCopy(blockInfo blockInfo, Vector3 pos)
        {
            return new Empty(blockInfo, pos);
        }

        protected override void SetUpCube()
        {
            return;
        }

        public override bool IsMe(SlopeType slopeType)
        {
            return (slopeType == SlopeType.Empty);
        }

        public override bool IsEmpty
        {
            get
            {
                return true;
            }
        }
    }
}
