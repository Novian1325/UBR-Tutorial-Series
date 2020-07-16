using UnityEngine;

namespace PolygonPilgrimage.BattleRoyaleKit
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(MeshRenderer))]
    public class Parachute : RichMonoBehaviour
    {
        private Animator anim;
        private MeshRenderer meshRenderer;

        // Use this for initialization
        private void Start()
        {
            meshRenderer.enabled = false;
        }

        protected override void GatherReferences()
        {
            base.GatherReferences();
            anim = this.gameObject.GetComponent<Animator>();
            meshRenderer = this.gameObject.GetComponent<MeshRenderer>();
        }

        public void DeployParachute()
        {
            meshRenderer.enabled = true;
            anim.SetTrigger("DeployChute");
            //play sound
            //let other players know teammate deployed 'chute  
        }

        public void DestroyParachute()
        {
            //don't destroy right away
            //maybe play a final animation or something
            //meshRenderer.enabled = false;
            Destroy(this.gameObject);
        }

    }
}
