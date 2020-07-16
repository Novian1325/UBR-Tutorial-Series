using System.Collections.Generic;
using System.Collections;
using System.Linq; //
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PolygonPilgrimage.BattleRoyaleKit
{
    public enum DegreeIncrement
    {
        One = 1,
        Five = 5,
        Ten = 10
    }

    public class Compass : RichMonoBehaviour
    {
        private static readonly float sortsPerSecond = 0.5f;// every other second

        /// <summary>
        /// Singleton pattern.
        /// </summary>
        private static Compass singletonInstance;

        [Header("---Scene Refs---")]
        [SerializeField]
        private RawImage compassImage;
        
        private static RawImage CompassImage { get => singletonInstance.compassImage; }

        private Transform mainCameraXform;

        [SerializeField]
        private TextMeshProUGUI compassDirectionText;

        /// <summary>
        /// Shortcut.
        /// </summary>
        public string CompassDirectionText { set => compassDirectionText.text = value; }

        [Header("---Readout Options---")]
        [SerializeField] private bool ordinalLetters = true;//show N instead of 0 or S instead of 180
        [SerializeField] private DegreeIncrement degreeIncrement = DegreeIncrement.Five;// round to this number

        [Header("---Prefabs---")]
        [SerializeField] private GameObject compassMarkerPrefab;// prefab used  //icons UV rect x value is between -.5 and .5

        private static GameObject CompassMarkerPrefab { get => singletonInstance.compassMarkerPrefab; }

        private static List<BRS_CompassMarker> compassMarkerList = new List<BRS_CompassMarker>();

        //coroutine references
        private Coroutine coroutine_CompassMarkerSort;//track coroutine so can stop/start (like when pausing game)
        private Rect headingRect = new Rect(360, 0, 1, 1);

        private void Start()
        {
            coroutine_CompassMarkerSort = StartCoroutine(SortCompassMarker(sortsPerSecond));
        }

        private void Update()
        {
            UpdateHeading();
        }

        private void OnEnable()
        {
            ResumeSorting();
        }

        private void OnDisable()
        {
            StopSorting();
        }

        protected override void GatherReferences()
        {
            base.GatherReferences();

            if (mainCameraXform == null) mainCameraXform =
                    GameObject.FindGameObjectWithTag("MainCamera").transform;

            InitSingleton();
        }

        /// <summary>
        /// Set the text of Compass Degree Text to the clamped value, but change it to the letter if it is a True direction.
        /// </summary>
        /// <param name="angle"></param>
        private static string ConvertAngleToLetter(int angle)
        {
            string outputText;
            switch (angle)
            {
                case 0:
                case 360:
                    outputText = "N";
                    break;
                case 45:
                    outputText = "NE";
                    break;
                case 90:
                    outputText = "E";
                    break;
                case 135:
                    outputText = "SE";
                    break;
                case 180:
                    outputText = "S";
                    break;
                case 225:
                    outputText = "SW";
                    break;
                case 270:
                    outputText = "W";
                    break;
                case 315:
                    outputText = "NW";
                    break;
                default:
                    outputText = angle.ToString();
                    break;
            }
            return outputText;
        }

        /// <summary>
        /// Only a single instance of Compass should ever exist.
        /// </summary>
        private void InitSingleton()
        {
            if (!singletonInstance)
            {
                singletonInstance = this;
            }
            else
            {
                Debug.LogError("[Compass] Too many instances of Compass in Scene", this);
                Destroy(this.gameObject); // get outta here, you imposter!
            }

        }

        private void UpdateDistances()
        {
            for (int i = 0; i < compassMarkerList.Count; ++i)
            {
                var compassMarker = compassMarkerList[i]; // get cached marker
                var trackablePosition = compassMarker.GetTrackableTransform().position; // get target position
                var distance = Vector3.Distance(mainCameraXform.position, trackablePosition); // get distance

                compassMarker.SetDistanceFromPlayer(distance); // store this value on the marker

                //check if Player is close enough to marker to be revealed
                if (distance <= compassMarker.GetRevealDistance())
                {
                    //update uv rect on compass to reflect angle to player
                    UpdateCompassMarker(compassMarker, trackablePosition, mainCameraXform);

                    //enable it if it is not already so
                    if (!compassMarker.isActiveAndEnabled)
                    {
                        compassMarker.gameObject.SetActive(true);
                    }

                }
                else // too far away
                {
                    compassMarker.gameObject.SetActive(false);
                }
            }
        }

        private void UpdateHeading()
        {
            var headingAngle = mainCameraXform.eulerAngles.y;

            //Get a handle on the Image's uvRect
            headingRect.x = headingAngle / 360;
            compassImage.uvRect = headingRect;

            //round heading
            headingAngle = Mathf.RoundToInt(headingAngle / (int)degreeIncrement) * (int)degreeIncrement;

            //convert the numbers to letters if pointing towards a direction (N/E/S/W)
            if (ordinalLetters)
            {
                compassDirectionText.text = ConvertAngleToLetter((int)headingAngle);
            }
            else
            {
                compassDirectionText.text = headingAngle.ToString();
            }
        }

        /// <summary>
        /// Update the icon on the compass to match angle from player to trackable object
        /// </summary>
        /// <param name="compassMarker">marker with image to manipulate</param>
        /// <param name="trackablePosition">Position of trackable object in World Space</param>
        /// <param name="playerXform">Reference transform (player)</param>
        private void UpdateCompassMarker(BRS_CompassMarker compassMarker,
            Vector3 trackablePosition, Transform playerXform)
        {
            var relative = playerXform.InverseTransformPoint(trackablePosition);
            var angle = Mathf.Atan2(relative.x, relative.z) * Mathf.Rad2Deg; // yay trigonometry!

            headingRect.x = -angle / 360; // convert angle to displacement of UI element
            compassMarker.GetCompassMarkerImage().uvRect = headingRect; //need a value between -.5 an .5 for uvRect
        }

        /// <summary>
        /// Stops coroutine associated with sorting. Useful if game is paused or if there aren't enough to warrant sorting.
        /// </summary>
        public void StopSorting()
        {
            if (coroutine_CompassMarkerSort != null) StopCoroutine(coroutine_CompassMarkerSort);
        }

        /// <summary>
        /// Causes coroutines to resume.
        /// </summary>
        public void ResumeSorting()
        {
            //
            if (coroutine_CompassMarkerSort == null)
            {
                coroutine_CompassMarkerSort = StartCoroutine(SortCompassMarker(sortsPerSecond));
            }
        }

        /// <summary>
        /// Coroutine used to restrict frequency of list sorting. Helps with performance
        /// </summary>
        /// <param name="sortsPerSecond"></param>
        /// <returns></returns>
        private IEnumerator SortCompassMarker(float sortsPerSecond = 1)
        {
            var sortDelay = new WaitForSeconds(1 / sortsPerSecond); // create and re-use

            while (true)
            {
                UpdateDistances(); // update all distances from markers to active player

                if (compassMarkerList.Count > 1)
                {
                    //TODO: LINQ is not very performant, especially in a repeating function. Use SelectionSort instead
                    //order icons so closest object to player is on top of all other icons
                    compassMarkerList = compassMarkerList.OrderBy(o => o.GetDistanceFromPlayer()).ToList();

                    for (var i = 0; i < compassMarkerList.Count; ++i)
                    {
                        compassMarkerList[i].transform.SetSiblingIndex(compassMarkerList.Count - 1 - i);
                    }
                }

                yield return sortDelay;
            }
        }

        /// <summary>
        /// Adds the given trackable to the top compass.
        /// </summary>
        /// <param name="newTrackable">Trackable whose texture and color will be used on the icon.</param>
        public static void RegisterTrackable(BRS_Trackable newTrackable)
        {
            //do nothing if no Compass in Scene
            if (!singletonInstance)
            {
                Debug.Log("[Compass] No Compass in Scene.");
                return;
            }

            //check if already exists
            foreach (var marker in compassMarkerList)
            {
                if (marker.CompareTrackable(newTrackable)) return;
            }

            //create new marker
            var compassMarker = Instantiate(CompassMarkerPrefab, CompassImage.transform)
                .GetComponent<BRS_CompassMarker>() as BRS_CompassMarker;

            //initialize marker with image, color, and distance
            compassMarker.InitCompassMarker(newTrackable);

            //add trackables to list
            compassMarkerList.Add(compassMarker);
        }

        /// <summary>
        /// Removes trackable from the compass.
        /// </summary>
        /// <param name="trackable">trackable to remove</param>
        public static void UnregisterTrackable(BRS_Trackable trackable)
        {
            //do nothing if no Compass in Scene
            if (!singletonInstance)
            {
                return;
            }

            for (var i = 0; i < compassMarkerList.Count; ++i)
            {
                var marker = compassMarkerList[i];//cache

                if (marker.CompareTrackable(trackable))
                {
                    //remove marker icon reference
                    compassMarkerList.Remove(marker);//this is safe as long as there is a 'break' at the end
                    //destroy UI element
                    if (marker) Destroy(marker.gameObject); // TODO: consider pooling 
                    break;
                }
            }
        }
    }

}
