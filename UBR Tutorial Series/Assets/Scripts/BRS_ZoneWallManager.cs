﻿using System;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random; // clarify which random we are talking about because System contains a Random class as well

//NOTE! Sizes and lengths are given in Unity Meters unless otherwise noted.

namespace PolygonPilgrimage.BattleRoyaleKit
{
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(LineRenderer))]
    [SelectionBase]
    public class BRS_ZoneWallManager : RichMonoBehaviour
    {
        /// <summary>
        /// Singleton pattern.
        /// </summary>
        public static BRS_ZoneWallManager Instance { get; private set; } // readonly externally
        public static GameObject GameObject { get; private set; } // readonly externally

        [Header("---Zone Wall Manager---")]
        [Range(16, 360)]
        [Tooltip("How many segments should the circle that appears on the mimimap be? More segments means it looks crisper, but at cost of performance.")]
        [SerializeField] private int lineRendererSegments = 64;//64 seems perfect

        /// <summary>
        /// The projecte image on the rim of ZoneWall. Not needed
        /// </summary>
        [Tooltip("The projecte image on the rim of ZoneWall. Not needed")]
        [SerializeField] private Projector safeZone_Circle_Projector;

        /// <summary>
        /// Scriptable Object containing options for shrink time, radius, damage.
        /// </summary>
        [Header("---Zone Phase Options---")]
        [Tooltip("Scriptable Object containing options for shrink time, radius, damage.")]
        [SerializeField] private ShrinkPhaseOptions shrinkPhaseOptions;

        /// <summary>
        /// Tick rate, damage, radius, and other information for each Phase.
        /// </summary>
        private ShrinkPhase CurrentShrinkPhase
        { get => shrinkPhaseOptions.ShrinkPhases[shrinkPhaseIndex]; } // readonly

        [Header("---Events---")]
        [SerializeField]
        private UnityEvent startShrinkEvent = new UnityEvent();

        public static UnityEvent StartShrinkEvent { get => Instance.startShrinkEvent; } // readonly

        [SerializeField]
        private UnityEvent endShrinkEvent = new UnityEvent();

        public static UnityEvent EndShrinkEvent { get => Instance.endShrinkEvent; } // readonly

        /// <summary>
        /// this can be set to PUBLIC in order to troubleshoot.  It will show a checkbox in the Inspector.
        /// </summary>
        private bool shrinking = false;  // 

        /// <summary>
        /// iterates through delays between each phase and speed at which each phase shrinks.
        /// </summary>
        private int shrinkPhaseIndex = 0;

        /// <summary>
        /// Radius in Unity Meters that the Zone Wall should start the game with.
        /// </summary>
        private float startingZoneWallRadius = 1000;

        /// <summary>
        /// Holds the next Time that the next shrink phase will start.
        /// </summary>
        private float nextShrinkTime;//

        /// <summary>
        /// this is the SIZE of the zone wall object (not scale) in Meters. measure it with a primitive shape to be sure. or snag the radius of attached collider.
        /// </summary>
        private int originalZoneWallRadius;//

        /// <summary>
        /// Current size in Meters of the Zone Wall Radius.
        /// </summary>
        private float currentZoneWallRadius;

        /// <summary>
        /// Distance to centerpoint
        /// </summary>
        private float distanceToMoveCenter;

        /// <summary>
        /// The Zone Wall will become smaller to match its radius with this target radius.
        /// </summary>
        private float targetShrunkenRadius;

        /// <summary>
        /// Point in space that the Zone Wall orbits around.
        /// </summary>
        private Vector3 centerPoint;//

        /// <summary>
        /// Reference to the UI circle that shows the future bounds of the Zone Wall.
        /// </summary>
        private GameObject leadingCircle;

        /// <summary>
        /// Line Renderer that draws the bounds of the Zone Wall.
        /// </summary>
        private LineRenderer lineRenderer;

        /// <summary>
        /// Capsule Collider attached to this GameObject. Used as a reference for Scale.
        /// </summary>
        private CapsuleCollider capsuleCollider;

        /// <summary>
        /// See Debug statements about what's going on during runtime.
        /// </summary>
        [Tooltip("Would the developer like to see Debug statements about what's going on during runtime?")]
        [SerializeField] private bool DEBUG = false;

        /// <summary>
        /// Cached Transform of the ZoneWall Object.
        /// </summary>
        public Transform ZoneWallXform { get => transform; }

        public bool IsShrinking { get => shrinking; } // readonly

        protected override void Awake()
        {
            base.Awake(); // do normal RichMonoBehaviour stuff

            //get original radius
            originalZoneWallRadius = (int)capsuleCollider.radius;
            currentZoneWallRadius = startingZoneWallRadius;

            //draw minimap zone cirlce
            lineRenderer.useWorldSpace = false;//display linerenderer in space local to center of zone wall, not world
            ConfigureWorldCircle(lineRenderer, originalZoneWallRadius, 
                originalZoneWallRadius, lineRendererSegments);

            //move projector with circle
            safeZone_Circle_Projector.transform.position =
                new Vector3(0, capsuleCollider.height, 0);//make sure projector is at a good height

            InitSingleton();
        }

        void Start()
        {
            //verfiy input
            if (!VerifyShrinkPhases())
            {
                Debug.LogError("[ZoneWallManager]" +
                    " Shrink Phases in Zone Wall Options are not valid." +
                    " Check input and try again.", this.gameObject);
            }

            //set target bounds
            InitNextShrink();
            
            //apply starting values
            ShrinkEverything();
        }

        void Update()
        {
            HandleShrinkingUpdate();
        }

        protected override void GatherReferences()
        {
            base.GatherReferences();

            //set necessary references to Components
            capsuleCollider = GetComponent<CapsuleCollider>();
            lineRenderer = gameObject.GetComponent<LineRenderer>();
        }

        private void InitSingleton()
        {
            if (!Instance)
            {
                Instance = this; // there can only be ONE!
                GameObject = gameObject; // for easy access
            }
            else
            {
                Debug.LogError("[ZoneWallManager] Singleton failed. " +
                    "Too many Objects in Scene!", this);
            }
        }

        /// <summary>
        /// Alerts Developer of invalid input. Class is robust and will attempt to skip invalid phases.
        /// </summary>
        /// <returns></returns>
        private bool VerifyShrinkPhases()
        {
            var phasesAreOkay = true;//only takes one to become false

            //each subsequent radius must be smaller than the first.
            var radius = startingZoneWallRadius;
            for(var i = 0; i < shrinkPhaseOptions.ShrinkPhases.Length; ++i)
            {
                var phase = shrinkPhaseOptions.ShrinkPhases[i]; // cache

                //check radius
                if(phase.ShrinkToRadius >= radius)
                {
                    Debug.LogError("[ZoneWallManager] ShrinkPhases are not valid. " 
                        + "Check that radii are in descending order.", this);
                    phasesAreOkay = false;
                }
                else
                {
                    radius = phase.ShrinkToRadius;
                }

                //check shrink seconds to avoid divide by zero on bad input
                if(phase.TicksPerSecond <= 0)
                {
                    Debug.LogError("[ZoneWallManager] Ticks per second must" +
                        " be a positive integer! : " + phase.TicksPerSecond);
                }
            }

            return phasesAreOkay;
        }

        /// <summary>
        /// Shrink, wait for cue, or pass the time.
        /// </summary>
        private void HandleShrinkingUpdate()
        {//is the zone currently in a shrinking state
            if (shrinking)
            {
                //shrink all the things
                ShrinkEverything();

                //know when to stop shrinking
                HandleStopShrinking();
            }
            
            //is it time to start shrinking?
            else if (Time.time > nextShrinkTime)
            {
                shrinking = true;
                startShrinkEvent.Invoke();
                if (DEBUG) Debug.Log("[ZoneWallManager] Shrinking....");
            }

            else
            {//waiting for time to pass
                if (DEBUG)
                {
                    //use string builder because concatentation ( + ) is expensive
                    StringBuilder stringBuilder = new StringBuilder();

                    stringBuilder.Append("[ZoneWallManager] Shrinking in ");
                    stringBuilder.Append(nextShrinkTime - Time.time);

                    Debug.Log(stringBuilder.ToString());
                }
            }
        }

        /// <summary>
        /// Configure a new centerpoint for the next shrink phase.
        /// </summary>
        private void ConfigureNewCenterPoint()
        {
            centerPoint = FindNewCenterPoint(ZoneWallXform.position, 
                currentZoneWallRadius, targetShrunkenRadius, DEBUG);
            distanceToMoveCenter = Vector3.Distance(
                ZoneWallXform.position, centerPoint); //this is used in the Lerp (below)

            if (DEBUG)
            {
                //build new center point message
                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.Append("[ZoneWallManager] New Center Point: ");
                stringBuilder.Append(centerPoint);

                Debug.Log(stringBuilder.ToString());
            }
        }

        /// <summary>
        /// Move/Shrink all components.
        /// </summary>
        private void ShrinkEverything()
        {
            var currentPhase = CurrentShrinkPhase; // cache

            // shrink the zone diameter, over time
            currentZoneWallRadius = Mathf.MoveTowards(currentZoneWallRadius, 
                targetShrunkenRadius, 
                (targetShrunkenRadius / currentPhase.SecondsToFullyShrink) //step ratio
                * Time.deltaTime); // shrink rate

            var sizeRatio = currentZoneWallRadius / originalZoneWallRadius;

            //shrink the zoneWall object and all of its children
            ZoneWallXform.localScale = 
                new Vector3(sizeRatio, 1, sizeRatio); //set local scale of zone wall

            //move ZoneWall towards new centerpoint
            ZoneWallXform.position = Vector3.MoveTowards(
                ZoneWallXform.position, //current position
                new Vector3(centerPoint.x, ZoneWallXform.position.y, centerPoint.z), //target position
                (distanceToMoveCenter / currentPhase.SecondsToFullyShrink) // step ratio
                * Time.deltaTime); // shrink rate

            // shrink circle projector
            if (safeZone_Circle_Projector)
                safeZone_Circle_Projector.orthographicSize = currentZoneWallRadius;
        }

        /// <summary>
        /// Set timers, choose new shrink point, draw new leading circle.
        /// </summary>
        private void InitNextShrink()
        {
            var phase = CurrentShrinkPhase; // cache 

            //this shrink phase, shrink from current radius to this smaller radius. 
            targetShrunkenRadius = phase.ShrinkToRadius;  //use the ZoneRadiusFactor as a percentage

            //set next shrink time
            nextShrinkTime = Time.time 
                + phase.SecondsUntilShrinkBegins;
                        
            //get a new centerpoint for the zone wall to shrink around
            ConfigureNewCenterPoint();

            //show on minimap where zone will shrink to
            leadingCircle = CreateLeadingCircleObject(centerPoint, targetShrunkenRadius,
                originalZoneWallRadius, //original height works here because zone wall model resembles a sphere
                lineRendererSegments);
        }

        /// <summary>
        /// Control when the circle should stop shrinking.
        /// </summary>
        private void HandleStopShrinking()
        {
            // MoveTowards will continue ad infinitum, so we must test that we have gotten CLOSE ENOUGH to be DONE
            if (0.5f > (currentZoneWallRadius - targetShrunkenRadius))//shrinking complete
            {
                Destroy(leadingCircle);

                shrinking = false;
                endShrinkEvent.Invoke();
                if (DEBUG) Debug.Log("[ZoneWallManager] Finished shrinking.");

                //is there more shrinking to do?
                if (++shrinkPhaseIndex < shrinkPhaseOptions.ShrinkPhases.Length)
                {
                    //set timers, draw new circle... 
                    InitNextShrink();
                }
                else
                {
                    if (DEBUG) Debug.Log("[ZoneWallManager] Will no longer shrink.");
                    //turn off this behavior
                    this.enabled = false;
                }
            }
        }

        /// <summary>
        /// Randonly generates a new centerpoint in space.
        /// </summary>
        /// <param name="currentCenter"></param>
        /// <param name="currentRadius">New point will be within this radius</param>
        /// <param name="newRadius">Size of new radius</param>
        /// <param name="shrinkFactor"></param>
        /// <param name="DEBUG"></param>
        /// <returns></returns>
        private static Vector3 FindNewCenterPoint(Vector3 currentCenter, 
            float currentRadius, float newRadius, bool DEBUG = false)
        {
            Vector3 newCenterPoint = Vector3.zero;

            int attemptsUntilFailure = 500; //prevent endless loop which will kill Unity
            int attemptCounter = 0;
            bool foundSuitable = false;

            if (DEBUG) Debug.Log("[ZoneWallManager] Finding a new center point....");

            while (!foundSuitable)
            {
                Vector2 randPoint = Random.insideUnitCircle * newRadius;
                newCenterPoint = currentCenter + new Vector3(randPoint.x, currentCenter.y, randPoint.y);
                foundSuitable = (Vector3.Distance(currentCenter, newCenterPoint) < currentRadius);

                //DEBUGS
                if (DEBUG)
                {
                    Debug.LogFormat("RandomPoint: {0}", randPoint);
                    Debug.LogFormat("NewCenterPoint: {0}", newCenterPoint);
                    Debug.LogFormat("Distance: {0}; Current Radius: {1}", 
                        Vector3.Distance(currentCenter, newCenterPoint), 
                        currentRadius);
                }

                //to prevent infinite loop
                if (++attemptCounter > attemptsUntilFailure)
                {
                    //build error message
                    StringBuilder stringBuilder = new StringBuilder();

                    stringBuilder.Append("[ZoneWallManager] " +
                        "New Center point could not be found after ");
                    stringBuilder.Append(attemptsUntilFailure);
                    stringBuilder.Append(" attempts.");

                    Debug.LogError(stringBuilder.ToString());//display error message

                    newCenterPoint = currentCenter;//return same position to keep the game moving

                    break;//break out of while loop
                }
            }
            return newCenterPoint;
        }

        //public void CreateLeadingCircle()
        //{

        //    //show on minimap where zone will shrink to
        //    leadingCircle = CreateLeadingCircle(centerPoint, targetShrunkenRadius,
        //        originalZoneWallRadius, //original height works here because zone wall model resembles a sphere
        //        lineRendererSegments);
        //}

        public void DestroyLeadingCircle()
        {
            if (leadingCircle)
                Destroy(leadingCircle);
        }

        public void SetLeadingCircle(Vector3 circleCenterPoint,
            float radius, int segments = 64)
        {
            if (leadingCircle)
                Destroy(leadingCircle);

            leadingCircle = CreateLeadingCircleObject(circleCenterPoint,
                radius, originalZoneWallRadius, segments);
        }

        /// <summary>
        /// Create and configure a Leading Circle from scratch. One could set up a prefab with this configuration as well.
        /// </summary>
        /// <param name="circleCenterPoint">World Space coordinates of centerpoint.</param>
        /// <param name="radius">The radius of the Zone Wall after shrinking.</param>
        /// <param name="drawHeight">How high in space the points should be drawn</param>
        /// <param name="segments">How many segments should the circle be drawn using? 64 seems good; don't go too crazy.</param>
        /// <returns></returns>
        private static GameObject CreateLeadingCircleObject(Vector3 circleCenterPoint, 
            float radius, float drawHeight, int segments = 64)
        {
            //new empty game object
            var leadingCircle = new GameObject();
            //set position
            leadingCircle.transform.position = circleCenterPoint;
            //set layer to make sure is on top of other objects
            leadingCircle.layer = 10;// Minimap Icon layer
            //name it so developer can identify it
            leadingCircle.name = "Next Zone Wall Boundary Marker";

            //configure line renderer
            //create new
            var lr = leadingCircle.AddComponent<LineRenderer>() as LineRenderer;
            //coordinates are given in World Space, not Local Space (relative to the world, not this object)
            lr.useWorldSpace = false;
            //do not cast shadows
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            //do not receive shadows
            lr.receiveShadows = false;
            //do this because I said so... I had a reason once. what was it? The lesson here is: comment as you go
            lr.allowOcclusionWhenDynamic = false;
            //make first and last point connect to form a loop
            lr.loop = true;

            //create a new array
            ConfigureWorldCircle(lr, radius, drawHeight, segments);

            return leadingCircle;
        }
        
        /// <summary>
        /// Configure given line renderer's points to make a circle in space.
        /// </summary>
        /// <param name="renderer">which line renderer to use</param>
        /// <param name="radius">the radius of the circle</param>
        /// <param name="height">height of the circle</param>
        /// <param name="segments">how many segments should the circle be divided into?</param>
        /// <param name="renderInWorldSpace">Use local or world space?</param>
        private static void ConfigureWorldCircle(LineRenderer renderer, 
            float radius, float height, int segments = 64)
        {
            var arcLength = 0.0f;//used for trig to determine terminal point on unit circle
            var spaceBetweenPoints = 360f / segments;//if a circle has x points, this is the distance between each of those points

            renderer.positionCount = segments;//positions are vertices of circle

            //place each point an equal distance apart on the unit circle, scaled by radius
            for (var i = 0; i < segments; ++i)
            {
                var x = Mathf.Sin(Mathf.Deg2Rad * arcLength) * radius;
                var z = Mathf.Cos(Mathf.Deg2Rad * arcLength) * radius;

                renderer.SetPosition(i, new Vector3(x, height, z));

                arcLength += spaceBetweenPoints;
            }
        }

        public static float GetDamagePerTick()
        {
            return Instance.CurrentShrinkPhase.DamagePerTick;
        }

        /// <summary>
        /// Gets the height of the zone wall at which the line renderers are drawn.
        /// </summary>
        /// <returns></returns>
        public static float GetDrawHeight()
        {
            return Instance.capsuleCollider.height;
        }

        public static float GetCurrentRadius()
        {
            return Instance.currentZoneWallRadius;
        }

        /// <summary>
        /// Damage is dealt every "tick".
        /// </summary>
        /// <returns></returns>
        public static float GetTicksPerSecond()
        {
            return Instance.CurrentShrinkPhase.TicksPerSecond;
        }
    }
}
