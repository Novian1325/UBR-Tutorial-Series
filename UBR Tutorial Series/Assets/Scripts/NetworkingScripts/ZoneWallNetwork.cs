using UnityEngine;
using Bolt;

namespace PolygonPilgrimage.BattleRoyaleKit.Networking
{
    public class ZoneWallNetwork : EntityBehaviour<IZoneWallState>
    {
        private BRS_ZoneWallManager zoneWall;

        public override void Attached()
        {
            // Awake();
        }

        public override void SimulateOwner()
        {
            //propagate scale
        }
    }

}