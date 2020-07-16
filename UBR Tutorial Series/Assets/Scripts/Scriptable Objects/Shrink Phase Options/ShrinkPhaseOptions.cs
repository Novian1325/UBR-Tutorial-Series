using UnityEngine;

namespace PolygonPilgrimage.BattleRoyaleKit
{
    [CreateAssetMenu(fileName = "ShrinkPhaseOption_", menuName = "ScriptableObjects/ShrinkPhaseOption", order = 1)]
    public class ShrinkPhaseOptions : ScriptableObject
    {
        [SerializeField]
        private int startingRadius = 1000;

        public int StartingRadius { get => startingRadius; } // readonly

        [SerializeField]
        private ShrinkPhase[] shrinkPhases = new ShrinkPhase[7];

        public ShrinkPhase[] ShrinkPhases { get => shrinkPhases; } // readonly
    }

}
