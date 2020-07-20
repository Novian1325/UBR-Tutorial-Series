﻿using UnityEngine;
using Bolt;

namespace PolygonPilgrimage.BattleRoyaleKit.Networking
{
    public class ZoneWallNetwork : EntityBehaviour<IZoneWallState>
    {
        private BRS_ZoneWallManager zoneWall;

        private void Awake()
        {
            zoneWall = GetComponent<BRS_ZoneWallManager>();
        }

        public override void Attached()
        {
            // Awake();
            state.SetTransforms(state.Transform, zoneWall.ZoneWallXform); // assign transform value

            //set networking callbacks
            state.AddCallback("Scale", MatchScale);
        }

        public override void SimulateOwner()
        {
            //transform.position and .rotation are automatic
            //propagate scale
            if (zoneWall.IsShrinking)
            {
                state.Scale = zoneWall.ZoneWallXform.localScale;
            }
        }

        private void MatchScale()
        {
            zoneWall.ZoneWallXform.localScale = state.Scale;
        }
    }

}
