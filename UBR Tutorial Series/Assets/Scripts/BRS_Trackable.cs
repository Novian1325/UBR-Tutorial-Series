using UnityEngine;

namespace PolygonPilgrimage.BattleRoyaleKit
{
    public class BRS_Trackable : RichMonoBehaviour
    {
        [SerializeField] private Texture compassImage;
        [SerializeField] private float revealDistance;
        [SerializeField] private Color iconColor;

        private static Compass compassInstance;

        protected override void Awake()
        {
            base.Awake();

            //only the first trackable has to do the hard work
            InitStaticCompassInstance();

            //if there's no compass, there's no work to do
            if (!compassInstance)
            {
                Debug.LogError("ERROR! No BRS_Compass in scene! nothing to register trackable to.");
                this.enabled = false;
            }
        }

        /// <summary>
        /// Stop tracking and remove related objects from Compass Instance.
        /// </summary>
        public void RemoveTrackable()
        {
            compassInstance.UnregisterTrackable(this);
        }

        /// <summary>
        /// Attempt to find the compass instance.
        /// </summary>
        private static void InitStaticCompassInstance()
        {
            if (!compassInstance)
            {
                var compassObject = GameObject.FindGameObjectWithTag("Compass");
                if(compassObject) compassInstance = compassObject.GetComponent<Compass>();
            }
        }

        private void OnEnable()
        {
            if (compassInstance && compassImage) {
                compassInstance.RegisterTrackable(this);
            }
        }

        private void OnDisable()
        {
           if(compassInstance) compassInstance.UnregisterTrackable(this);
        }

        private void OnDestroy()
        {
            if (compassInstance) compassInstance.UnregisterTrackable(this);
        }

        public Texture GetCompassImage()
        {
            return this.compassImage; // readonly
        }

        public float GetRevealDistance()
        {
            return this.revealDistance; // readonly
        }

        public Color GetIconColor()
        {
            return this.iconColor; // readonly
        }

        /// <summary>
        /// Changes color on Trackable, and also updates the compass trackable icon color
        /// </summary>
        /// <param name="newColor"></param>
        public void SetPlayerColor(Color newColor)
        {
            this.iconColor = newColor;
            //update color of trackable
            compassInstance.UnregisterTrackable(this);
            compassInstance.RegisterTrackable(this);
        }

        public Transform GetTrackableTransform()
        {
            return transform;
        }
    }
}
