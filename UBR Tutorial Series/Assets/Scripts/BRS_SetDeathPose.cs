using UnityEngine;

namespace PolygonPilgrimage.BattleRoyaleKit
{
    public class BRS_SetDeathPose : RichMonoBehaviour
    {
        private Animator anim;
        public int DeathPose;

        protected override void GatherReferences()
        {
            base.GatherReferences();
            anim = gameObject.GetComponent<Animator>();
        }

        // Use this for initialization
        void Start()
        {
            anim.SetInteger("DeathPose", DeathPose);
        }
    }
}
