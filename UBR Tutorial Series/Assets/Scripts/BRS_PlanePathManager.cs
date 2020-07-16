using UnityEngine;
using System.Collections.Generic;

namespace PolygonPilgrimage.BattleRoyaleKit
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="BRS_PlanePathManagerInspector"/>
    public class BRS_PlanePathManager : RichMonoBehaviour
    {
        private const string DROPZONE_LAYER_NAME = "DropZoneTest";

        //visible to Inspector
        [Header("Map Settings")]
        [Tooltip("This is the spawn boundary for the airplane. Airplane will spawn at the very edge of this cylinder/sphere")]
        [SerializeField] private Transform planeSpawnBounds;//where can the plane start and stop?

        [Tooltip("This is the list of volumes where the players are allowed to jump from the plane.")]
        [SerializeField] private GameObject[] playerDropZones;//how big should the zone be that the plane flies through

        [Tooltip("These are the volumes for supply drops. Supplies drop as soon as plane enters this volume.")]
        [SerializeField] private GameObject[] supplyDropZones;//list containing all areas the player can drop into

        [Header("Plane Settings")]
        [Tooltip("This is the plane prefab. Must include a PlaneManager script.")]
        [SerializeField] private GameObject BRS_PlaneSpawn;//plane object (model) to spawn

        [Tooltip("The plane's airspeed when carrying Players")]
        [SerializeField] private int planeSpeed_PlayerDrop = 150;

        [Tooltip("The plane's airspeed when on a supply drop sortie")]
        [SerializeField] private int planeSpeed_SupplyDrop = 300;

        [Tooltip("Helps prevent infinite loops.")]
        [SerializeField]
        private int flightPathChecksUntilFailure = 50;

        [Tooltip("Enables Debug.Log statements and persistence of objects for debugging purposes.")]
        [SerializeField] private bool DEBUG = true;//if true, prints debug statements

        private GameObject[] acceptableDropZones;

        //how high does the plane fly?
        private float planeFlightAltitude = 800.0f;
        private float startingFlightAltitude;
        private bool planeContainsPlayers = false;

        private readonly int failedPathAltitudeIncrementAmount = 25;//if the flight path fails, raise the altitude by this much before trying again

        //radius of spawn zone
        private float spawnBoundsCircleRadius = 100.0f;
        private readonly int minimumDropZoneSize = 1000;//drop zones should be really tall so they can be tested against

        //start and end points for plane to fly through
        private Vector3 planeStartPoint;
        private Vector3 planeEndPoint;
        /// <summary>
        /// A primitive sphere that gets used as a target for raycasting against to determine if a path is clear.
        /// </summary>
        private GameObject endpointMarker;

        //to prevent infinite loops
        private int unsuccessfulPasses = 0;

        //stuff to pass on to plane when deployed
        private GameObject targetDropZone;
        private List<GameObject> cargo_Players = new List<GameObject>();
        private GameObject cargo_Supplies;//can only carry one supply drop per sortie
        private int planeFlightSpeed = 200;

        /// <summary>
        /// Verify all references exist and complain to Console if any are null.
        /// </summary>
        /// <returns></returns>
        private bool VerifyReferences()
        {
            var allReferencesOkay = true;

            if (planeSpawnBounds == null)
            {
                Debug.LogError("ERROR: plane spawn bounds not set!", this);
                allReferencesOkay = false;
            }

            if (playerDropZones.Length < 1)
            {
                Debug.LogError("ERROR: No Player Drop Zones in list!", this);
                allReferencesOkay = false;
            }

            if (supplyDropZones.Length < 1)
            {
                Debug.LogError("ERROR: No Supply Drop Zones in list!", this);
                allReferencesOkay = false;
            }

            return allReferencesOkay;
        }

        void Start()
        {
            //set and check altitude
            planeFlightAltitude = planeSpawnBounds.position.y > 0 
                ? planeSpawnBounds.position.y : 200f;//verifies that altitude is above 0
            startingFlightAltitude = planeFlightAltitude;//set starting value

            //set radius of spawnBoundsCircleRadius
            //leave at default value if local scale is too small
            spawnBoundsCircleRadius = planeSpawnBounds.localScale.x / 2 > spawnBoundsCircleRadius ? planeSpawnBounds.localScale.x / 2 : spawnBoundsCircleRadius;

            //error checking
            if (VerifyReferences())
            {
                if (DEBUG) Debug.Log("Everything looks good here.", this);
            }
            else
            {
                Debug.Log("Failed to set up references.", this);
            }

            //make sure drop zones are the proper height
            ConfigureDropZones(playerDropZones, minimumDropZoneSize);
            ConfigureDropZones(supplyDropZones, minimumDropZoneSize);

            endpointMarker = ConfigureEndpointMarker();
            endpointMarker.transform.SetParent(this.transform);//make this object the parent
            endpointMarker.SetActive(false);
        }

        /// <summary>
        /// Enforces minimum drop zone height.
        /// </summary>
        /// <param name="dropZones"></param>
        /// <param name="minimumHeight"></param>
        private static void ConfigureDropZones(GameObject[] dropZones, int minimumHeight = 1000)
        {
            Transform zoneXform;

            //MAKE SURE Y SCALE IS LARGE ENOUGH!
            foreach (var zone in dropZones)
            {
                zoneXform = zone.transform;//cache
                if (zoneXform.localScale.y < minimumHeight)
                    zoneXform.localScale = new Vector3(zoneXform.localScale.x, minimumHeight, zoneXform.localScale.z);
            }
        }

        /// <summary>
        /// Create the object that raycasts get tested against at runtime.
        /// </summary>
        /// <returns></returns>
        private static GameObject ConfigureEndpointMarker()
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere) as GameObject;//create obj
            marker.GetComponent<MeshRenderer>().enabled = false;//don't need to see this object
            marker.name = "Plane Path Raycast Target."; // name it so you don't say WTF in the hierarchy

            return marker;
        }

        /// <summary>
        /// Sets which drop zones and flight speed to use.
        /// </summary>
        private void ConfigureFlightType()
        {
            acceptableDropZones = planeContainsPlayers ? playerDropZones : supplyDropZones;
            planeFlightSpeed = planeContainsPlayers ? planeSpeed_PlayerDrop : planeSpeed_SupplyDrop;
        }

        /// <summary>
        /// Get a random point on the edge of the plane spawn bounds.
        /// </summary>
        /// <returns></returns>
        private Vector3 GetRandomPointOnCircle()
        {
            //get terminal point on unit circle, then multiply by radius
            var randomArc = Random.Range(0, 2 * Mathf.PI);
            var randomPoint = new Vector3(//create new vector3
                (Mathf.Cos(randomArc) * spawnBoundsCircleRadius) + planeSpawnBounds.position.x, //get x coordiantes on unit circle, multiply by radius, offset relative to bounds
                planeFlightAltitude, // set the height
                (Mathf.Sin(randomArc) * spawnBoundsCircleRadius) + planeSpawnBounds.position.z);//get y coordinate on unity circle, multiply by radius, offset relative to bounds
                //Debug.Log("random point: " + randomPoint);
                //could also use sqrt(1 - x^2), but I like using the trig functions
            return randomPoint;
        }

        /// <summary>
        /// Add cargo to list and flag flight if Player is on board.
        /// </summary>
        /// <param name="cargo"></param>
        private void LoadCargo(GameObject cargo)
        {
            planeContainsPlayers = false; // assume false
            if (cargo.CompareTag("Player"))
            {
                cargo_Players.Add(cargo);
                planeContainsPlayers = true;
            }
            else
            {
                cargo_Supplies = cargo;//set supplies
            }
        }

        /// <summary>
        /// Configure flight, get a flight path, and then spawn the plane.
        /// </summary>
        /// <returns>Whether or not a plane was spawned.</returns>
        public bool InitPlaneDrop()
        {
            ConfigureFlightType();
            var success = SetupFlightPath();
            if (success)
            {
                SpawnPlane();//catch the plane Manager to keep track of the plane further
            }

            return success;
        }

        /// <summary>
        /// Configure a flight path after adding Cargo.
        /// </summary>
        /// <param name="cargo">Supply Drop Prefab or Player Game Object to be dropped upon arriving at drop zone.</param>
        /// <returns></returns>
        public bool InitPlaneDrop(GameObject cargo)
        {
            LoadCargo(cargo);

            return InitPlaneDrop();
        }

        /// <summary>
        /// If multiple cargo are added, load each and Init the plane drop.
        /// </summary>
        /// <param name="incomingCargo">Supply Drop Prefab or Player Game Object to be dropped upon arriving at drop zone.</param>
        /// <returns></returns>
        public bool InitPlaneDrop(GameObject[] incomingCargo)
        {
            if (DEBUG)
                Debug.Log("Trying to be in the plane.", this);

            foreach (var cargo in incomingCargo)
            {
                LoadCargo(cargo);
            }

            return InitPlaneDrop();
        }

        /// <summary>
        /// Decides where the plane starts and stops
        /// </summary>
        /// <returns></returns>
        private bool SetupFlightPath()
        {
            //flags to inform developer why flight path failed.
            bool endpointHit = false;
            bool flightPathThroughLZ = false;

            //find a start point
            planeStartPoint = GetRandomPointOnCircle();

            //look for an endpoint
            for (var endPointsFound = 1; endPointsFound <= flightPathChecksUntilFailure; ++endPointsFound)//while you don't have a valid flight path...
            {
                //Debug.Log("Attempt No: " + endPointsFound);
                //get end point on circle
                planeEndPoint = GetRandomPointOnCircle();
                //move marker to target endpoint to be raycasted against
                endpointMarker.transform.position = planeEndPoint;
                //enable marker
                endpointMarker.SetActive(true);

                //test if flight path goes through LZ
                if (TestRaycastThroughDropZone(planeStartPoint, endpointMarker.transform.position, acceptableDropZones))
                {
                    flightPathThroughLZ = true;
                }
                else
                {
                    if (DEBUG) Debug.Log("INVALID: Flight path not through LZ.", this);
                }

                //test if flight path is clear all the way to endpoint
                if (TestRaycastHitEndPoint(planeStartPoint, endpointMarker))
                {
                    endpointHit = true;
                }
                else
                {
                    if (DEBUG) Debug.Log("INVALID: Endpoint Marker Not Hit.", this);
                }

                //disable marker
                endpointMarker.SetActive(false);

                //is the flight unobstructed and through a drop zone
                if (endpointHit && flightPathThroughLZ)
                {
                    //SUCCESS!!!!!!! 
                    //reset variables for next path
                    planeFlightAltitude = startingFlightAltitude;//reset altitude for next try
                    unsuccessfulPasses = 0; // track to prevent infinite loops
                    return true;
                }
                else
                {
                    if (DEBUG)
                    {
                        if (!endpointHit)
                            Debug.Log("Flight path failed because ENDPOINT NOT HIT.", this);

                        if (!flightPathThroughLZ)
                            Debug.Log("Flight path failed because NOT THROUGH LZ.", this);
                        Debug.Log(".................New test.......................", this);
                    }
                    endpointHit = false;
                    flightPathThroughLZ = false;
                }

            }//end for

            //this altitude is not working. keep raising
            if (++unsuccessfulPasses > flightPathChecksUntilFailure)//we've been here before
            {
                Debug.LogError("[PlanePathManager] Flight pathfinding failed after " 
                    + unsuccessfulPasses * flightPathChecksUntilFailure
                    + " attempts. Tips: Widen planeSpawnBounds, " + 
                    "increase size and/or number of Drop Zones Skipping Plane Deployment", this);
                unsuccessfulPasses = 0;
                planeFlightAltitude = startingFlightAltitude;//reset altitude for next try
                return false;
            }
            //raise altitude and try again
            planeFlightAltitude += failedPathAltitudeIncrementAmount;
            //try again
            return SetupFlightPath();

        }//end func

        /// <summary>
        /// Instantiate a plane GO from prefab.
        /// </summary>
        /// <returns>Plane GameObject created.</returns>
        public PlaneManager SpawnPlane()
        {
            //create this plane in the world at this position, with no rotation
            var plane = Instantiate(BRS_PlaneSpawn, planeStartPoint, 
                Quaternion.identity) as GameObject;//do not set plane to be child of this object!
            plane.transform.LookAt(planeEndPoint);//point plane towards endpoint

            //get plane manager
            var planeManager = plane.GetComponent<PlaneManager>() as PlaneManager
                ?? plane.AddComponent<PlaneManager>() as PlaneManager;//create it if it doesn't exist

            //init plane
            planeManager.InitPlane(targetDropZone, cargo_Players.ToArray(), 
                cargo_Supplies, planeFlightSpeed);
            cargo_Players.Clear();
            return planeManager;

        }

        /// <summary>
        /// Test whether a ray can be cast from startPoint to targetObject and hit a dropZone in list
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="targetObject"></param>
        /// <param name="acceptableDropZones"></param>
        /// <returns></returns>
        private bool TestRaycastThroughDropZone(Vector3 startPoint, 
            Vector3 targetObject, GameObject[] acceptableDropZones)
        {
            if (DEBUG)
            {
                Debug.Log("[PlanePathManger] dropZone Count: " + acceptableDropZones.Length);
            }

            //did the raycast go through a drop zone?
            var raycastThroughDropZone = false;

            var oldLayer = acceptableDropZones[0].layer;
            var testLayer = LayerMask.NameToLayer(DROPZONE_LAYER_NAME);
            var layerMask = LayerMask.GetMask(DROPZONE_LAYER_NAME);
            foreach (var zone in acceptableDropZones)
            {
                zone.layer = testLayer; // set on individual layer for performance and to avoid overlap
            }

            //RaycastHit will store information about anything hit by the raycast
            RaycastHit raycastHitInfo;
            //raycast
            if (Physics.Raycast(startPoint, targetObject - startPoint, out raycastHitInfo, 
                (spawnBoundsCircleRadius + 10) * 2,
                layerMask, QueryTriggerInteraction.Collide))
            {
                if (DEBUG)
                {
                    Debug.Log("Testing Raycast Through DropZone. Hit: "
                    + raycastHitInfo.collider.gameObject.name, // name
                    raycastHitInfo.collider.gameObject); // reference

                    //Debug.Log("Possible Zone Count: " + acceptableDropZones.Length);
                }

                for (var i = 0; i < acceptableDropZones.Length; ++i)//look through each drop zone in list
                {
                    if (raycastHitInfo.collider.gameObject == acceptableDropZones[i])//if the game object that was hit is inside this list of good zones
                    {
                        targetDropZone = acceptableDropZones[i];//this zone will be passed to the plane, so it knows when it hits said zone
                        raycastThroughDropZone = true;//booyah!
                        break;//break out of for loop looking through gameObjects in list
                    }//end if
                }//end for

            }//end if
            else
            {
                //Debug.LogError("ERROR! Raycast missed target LZ");
            }

            foreach (var zone in acceptableDropZones)
            {
                zone.layer = oldLayer; // set on individual layer for performance and to avoid overlap
            }

            return raycastThroughDropZone;
        }

        /// <summary>
        /// Test whether or not a ray can be cast from startPoint towards targetObject without being obstructed by a physical object (ignoring trigger colliders).
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="targetObject"></param>
        /// <returns></returns>
        private bool TestRaycastHitEndPoint(Vector3 startPoint, GameObject targetObject)
        {
            //did the raycast hit the endpoint unobstructed by terrain or obstacles?
            var raycastHitEndpoint = false;
            //RaycastHit holds info about raycast
            RaycastHit raycastHitInfo;
            //if something was hit...
            if (Physics.Raycast(startPoint, targetObject.transform.position - startPoint, out raycastHitInfo, spawnBoundsCircleRadius * 2, 
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                if (DEBUG)
                {
                    Debug.Log("Testing Raycast against Endpoint. Hit: "
                    + raycastHitInfo.collider.gameObject.name, raycastHitInfo.collider.gameObject);

                    Debug.DrawRay(startPoint, targetObject.transform.position - startPoint,
                        Color.green, 10, false);
                }
                //set bool to whether the object ray hit is same as target
                raycastHitEndpoint = raycastHitInfo.collider.gameObject == targetObject;
            }
            else
            {
                //Debug.LogError("ERROR! Raycast missed it's target: " + targetObject);
            }

            return raycastHitEndpoint;
        }

        [ContextMenu("Show Drop Zones")]
        public void ShowDropZoneRenderers()
        {
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                renderer.enabled = true;
            }
        }

        [ContextMenu("Hide Drop Zones")]
        public void HideDropZoneRenderers()
        {
            var renderers = GetComponentsInChildren<Renderer>();
            foreach(var renderer in renderers)
            {
                renderer.enabled = false;
            }
        }
    }
}
