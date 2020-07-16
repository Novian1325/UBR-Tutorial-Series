using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PolygonPilgrimage.BattleRoyaleKit
{
    [RequireComponent(typeof(RawImage))]
    public class BRS_CompassMarker : RichMonoBehaviour
    {
        [Header("---Compass Marker---")]
        [Tooltip("Integrated. Optional.")]
        [SerializeField] private TextMeshProUGUI distanceTMP;

        private RawImage compassMarkerImage;
        private BRS_Trackable trackable;
        private float distanceFromPlayer; // updated through SetDistanceToPlayer()

        //coroutine trackers
        private Coroutine coroutine_updateDistanceText;//track to enable pausing game
        private static readonly int textUpdatesPerSecond = 2;

        protected override void GatherReferences()
        {
            base.GatherReferences();
            compassMarkerImage = gameObject.GetComponent<RawImage>() as RawImage;
        }

        private void Start()
        {
            ResumeUpdatingDistance();
        }

        private void OnEnable()
        {
            ResumeUpdatingDistance();
        }

        private void OnDisable()
        {
            StopUpdatingDistance();
        }

        /// <summary>
        /// Initialize a new compass marker
        /// </summary>
        /// <param name="trackable">Associated trackable object this is paired with.</param>
        public void InitCompassMarker(BRS_Trackable trackable)
        {
            this.trackable = trackable; // cache

            //update visuals
            UpdateColor();
            UpdateIcon();
        }


        /// <summary>
        /// Stops coroutine associated with distance polling. Useful if game is paused.
        /// </summary>
        public void StopUpdatingDistance()
        {
            if(coroutine_updateDistanceText != null) StopCoroutine(coroutine_updateDistanceText);
        }

        /// <summary>
        /// Causes coroutines to resume.
        /// </summary>
        public void ResumeUpdatingDistance()
        {
            //
            if (coroutine_updateDistanceText == null && distanceTMP)
            {
                coroutine_updateDistanceText = StartCoroutine(UpdateDistanceText());
            }
        }
        
        public bool CompareTrackable(BRS_Trackable otherTrackable)
        {
            return otherTrackable == trackable;
        }

        public RawImage GetCompassMarkerImage()
        {
            return compassMarkerImage;
        }

        public float GetRevealDistance()
        {
            return this.trackable.GetRevealDistance();
        }

        public Transform GetTrackableTransform()
        {
            return trackable.GetTrackableTransform();
        }

        public float GetDistanceFromPlayer()
        {
            return this.distanceFromPlayer;
        }

        public void SetDistanceFromPlayer(float distanceToPlayer)
        {
            this.distanceFromPlayer = distanceToPlayer;
        }

        public void UpdateColor()
        {
            compassMarkerImage.color = trackable.GetIconColor();
        }

        public void UpdateIcon()
        {
            compassMarkerImage.texture = trackable.GetCompassIcon();
        }

        /// <summary>
        /// Controls how often updates to the UI text are made.
        /// </summary>
        /// <returns></returns>
        private IEnumerator UpdateDistanceText()
        {
            var updateDelay = new WaitForSeconds(1 / textUpdatesPerSecond);

            while (true)
            {
                distanceTMP.text = distanceFromPlayer.ToString();
                yield return updateDelay;
            }
        }
    }
}
