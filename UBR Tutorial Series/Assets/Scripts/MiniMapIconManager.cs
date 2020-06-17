using UnityEngine;

namespace PolygonPilgrimage.BattleRoyaleKit
{
    public class MiniMapIconManager : RichMonoBehaviour
    {
        private MeshRenderer meshRenderer;

        // Use this for initialization
        void Start()
        {
            if (meshRenderer)
            {
                ShowIconOnMap(true); //turn on minimap icons
            }
            else
            {
                Debug.LogError("ERROR! No MeshRenderer on MiniMap Icon: " + this.gameObject.name);
            }
        }

        protected override void GatherReferences()
        {
            base.GatherReferences();
            meshRenderer = GetComponent<MeshRenderer>();
        }

        public void ShowIconOnMap(bool active)
        {
            meshRenderer.enabled = active;
        }

        // Update is called once per frame
        void Update()
        {
            //determine if this icon should be shown on minimap and set visibility

        }
    }

}
