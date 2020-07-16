using UnityEngine;
//---------------------------------------------------------------------------------------------------
//This script is provided as a part of The Polygon Pilgrimage
//Subscribe to https://www.youtube.com/user/mentallic3d for more great tutorials and helpful scripts!
//---------------------------------------------------------------------------------------------------
namespace PolygonPilgrimage.BattleRoyaleKit
{
    /// <summary>
    /// This class handles checking if the behavior is outside of the bounds and deals damage accordingly.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(BRS_PlayerHealthManager))]
    public class BRS_ZoneDamage : RichMonoBehaviour
    {
        #region Visible In Inspector
        
        /// <summary>
        /// The Camera Object whose PostProcessing Behavior will be interchanged. If left empty (like for Bots) Console will not complain.
        /// </summary>
        [Tooltip("The Camera Object whose PostProcessing Behavior will be interchanged. If left empty (like for Bots) Console will not complain.")]
        [SerializeField] private GameObject cameraToOverride;

        /// <summary>
        /// When Player is outside of the zone wall, this line will appear and guide the player back to the safe zone.
        /// </summary>
        [Header("---UI References---")]
        [SerializeField]
        private LineRenderer linePointingToCircleCenter;

        /// <summary>
        /// For DEMO purposes ONLY.  Reset our Health to full when we re-enter the Zone!
        /// </summary>
        [Header("---Debugging---")]
        [Tooltip("For DEMO purposes ONLY.  Reset our Health to full when we re-enter the Zone!")]
        [SerializeField]
        private bool debugHealth = false;

        #endregion

        #region Internal Private Variables

        /// <summary>
        /// The object that should handle the dealing of the damage.
        /// </summary>
        private BRS_PlayerHealthManager healthManager;

        /// <summary>
        /// Is this Behavior inside the bounds of the Zone Wall?
        /// </summary>
        private bool inZone;

        /// <summary>
        /// What Time the next damage tick will occur.
        /// </summary>
        private float nextDamageTickTime;

        #endregion

        void Start()
        {
            //handle instance references
            VerifyReferences();
        }
    
        //called once per frame
        private void Update()
        {
            if (!inZone)//if outside the ZoneWall
            {   //ouch!
                HandleZoneDamage();
                DrawLineToCircle();
            }
            else if (debugHealth)//if inside zone and debugging health....
            {
                //increase health (for debugging purposes)
                healthManager.SetToMaxHealth(); // reset to max health
                //note: won't prevent death.
            }
        }

        void OnTriggerExit(Collider col)
        {
            //If we leave the zone, we will be damaged!
            if (col.gameObject == BRS_ZoneWallManager.GameObject)
            {
                inZone = false;
                //set the next Time the healthManager should be dealt a damage tick
                nextDamageTickTime += 1 / BRS_ZoneWallManager.GetTicksPerSecond();

                // TODO: change Post Processing            
            }
        }

        void OnTriggerEnter(Collider col)
        {
            //If we are inside the zone, all is good!
            if (col.gameObject == BRS_ZoneWallManager.GameObject)
            {
                inZone = true;

                // TODO: change Post Processing 
            }

            linePointingToCircleCenter.enabled = false;
        }

        private void DrawLineToCircle()
        {
            if (!linePointingToCircleCenter.enabled)
            {
                linePointingToCircleCenter.enabled = true;
            }
                        
            //starting point is player's current position, drawn at appropriate height
            var pointPosition = transform.position;
            pointPosition.y = BRS_ZoneWallManager.GetDrawHeight();//set height
            linePointingToCircleCenter.SetPosition(0, pointPosition);//set starting point

            //set ending point, a point on the edge of the circle
            var playerPositionRelativeToWall = BRS_ZoneWallManager.Instance.transform.
                InverseTransformPoint(transform.position);//convert world space point of player into local space of zoneWall circle
            var angle = Mathf.Atan2(playerPositionRelativeToWall.z, 
                playerPositionRelativeToWall.x) * Mathf.Rad2Deg;//get the angle between Player and centerpoint of zone wall circle

            //on the MiniMap, point a line from the Player towards the edge of the Zone
            var radius = BRS_ZoneWallManager.GetCurrentRadius();//get radius of circle

            var zoneWallPosition = BRS_ZoneWallManager.Instance
                .transform.position;//get x,z coordinates of circle
            //yay trigonometry!
            pointPosition.x = zoneWallPosition.x 
                + radius * Mathf.Cos(angle * (Mathf.PI / 180));//get x coordinate of point on edge of circle
            pointPosition.z = zoneWallPosition.z 
                + radius * Mathf.Sin(angle * (Mathf.PI / 180));//get y coordinate of point on edge of circle

            linePointingToCircleCenter.SetPosition(1, pointPosition);//set endpoint on edge of circle
        }

        protected override void GatherReferences()
        {
            base.GatherReferences();

            healthManager = GetComponent<BRS_PlayerHealthManager>()
                as BRS_PlayerHealthManager;

            //TODO: MATTT!!!!!! INIT post processing
        }

        /// <summary>
        /// Gets references that were not set and complains if they cannot be found.
        /// </summary>
        private void VerifyReferences()
        {
            if (!linePointingToCircleCenter)
            {
                Debug.LogError("[ZoneDamage] Line Renderer not assigned on Player.", this);
                return;
            }
            linePointingToCircleCenter.enabled = false; //start with object disabled;
            linePointingToCircleCenter.positionCount = 2; // straight line has 2 points
            linePointingToCircleCenter.useWorldSpace = true;
        }
        
        /// <summary>
        /// Handle the sending of damage.
        /// </summary>
        private void HandleZoneDamage()
        {
            if (Time.time > nextDamageTickTime)//if it's time to deal a damage tick
            {
                //Damage the healthManager depending on the phase of the zone wall
                healthManager.ChangeHealth(-BRS_ZoneWallManager.GetDamagePerTick());

                //set the next Time to deal a tick damage
                nextDamageTickTime += 1 / BRS_ZoneWallManager.GetTicksPerSecond();
            }
        }
    }
}
