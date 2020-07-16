using UnityEngine;

namespace PolygonPilgrimage.BattleRoyaleKit
{
    public class BRS_Trackable : RichMonoBehaviour
    {
        [SerializeField] private Texture compassIcon;
        [SerializeField] private float revealDistance;
        [SerializeField] private Color iconColor;

        /// <summary>
        /// Stop tracking and remove related objects from Compass Instance.
        /// </summary>
        public void RemoveTrackable()
        {
            Compass.UnregisterTrackable(this);
        }

        private void OnEnable()
        {
            if (compassIcon) {
                Compass.RegisterTrackable(this);
            }
        }

        private void OnDisable()
        {
            Compass.UnregisterTrackable(this);
        }

        private void OnDestroy()
        {
            Compass.UnregisterTrackable(this);
        }

        public Texture GetCompassIcon()
        {
            return compassIcon; // readonly
        }

        public float GetRevealDistance()
        {
            return revealDistance; // readonly
        }

        public Color GetIconColor()
        {
            return iconColor; // readonly
        }

        /// <summary>
        /// Changes color on Trackable, and also updates the compass trackable icon color
        /// </summary>
        /// <param name="newColor"></param>
        public void SetPlayerColor(Color newColor)
        {
            this.iconColor = newColor;
            //update color of trackable
            Compass.UnregisterTrackable(this);
            Compass.RegisterTrackable(this);
        }

        public Transform GetTrackableTransform()
        {
            return transform;
        }
    }
}
