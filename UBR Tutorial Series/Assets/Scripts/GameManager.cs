using UnityEngine;

namespace PolygonPilgrimage.BattleRoyaleKit
{
    public class GameManager : RichMonoBehaviour
    {
        /// <summary>
        /// Where the Players are when the game starts.
        /// </summary>
        [Header("GameSettings")]
        [Tooltip("Where should the Players be when the game starts?")]
        [SerializeField] private DeployPlayersMode deployPlayersMode = DeployPlayersMode.OnGround;

        [Tooltip("Players in the game. If not a single player is found, a search will be done for all objects tagged \"Player\" in scene.")]
        [SerializeField] private GameObject[] players;

        [Tooltip("The object that will calculate the randomized flight path.")]
        [SerializeField] private BRS_PlanePathManager planePathManager;

        [Header("SkyDiving")]
        [SerializeField] private SkyDiveHandler skyDiveController;

        [Tooltip("This is the height the Player will start at if \"startSkyDiving\" is true.")]
        [SerializeField] private int skyDiveTestHeight = 500;

        // Use this for initialization
        void Start()
        {
            VerifyReferences();

            //populate loot
            //determine mission
            //spawn certain enemies and specific locations

            InitPlayerStartMode();
        }

        private void InitPlayerStartMode()
        {
            switch (deployPlayersMode)
            {
                case DeployPlayersMode.InPlane:
                    DeployPlayersInPlane();
                    break;
                case DeployPlayersMode.OnGround:
                    break;
                case DeployPlayersMode.Skydiving:
                    if (skyDiveController.transform.position.y < skyDiveTestHeight)
                        skyDiveController.transform.position = new Vector3(skyDiveController.transform.position.x,
                            skyDiveTestHeight, skyDiveController.transform.position.z);

                    skyDiveController.BeginSkyDive();
                    break;
            }
        }

        private void DeployPlayersInPlane()
        {
            planePathManager.InitPlaneDrop(players);
        }

        private bool VerifyReferences()
        {
            bool allReferencesOkay = true;
            //verify players
            if (players.Length < 1)//if no players loaded
            {
                players = GameObject.FindGameObjectsWithTag("Player");
                if (players.Length < 1)//if no players tagged
                {
                    Debug.LogError("ERROR! No Players found in scene. Tag one or double check BRS TPC.");
                    allReferencesOkay = false;
                }
                else
                {
                    Debug.Log("Players Found!");
                }
            }

            //verify skyDiveController
            if (skyDiveController == null)
            {
                if (players.Length < 1)
                {
                    Debug.LogError("ERROR! cannot auto set skyDiveController without any players.");
                    allReferencesOkay = false;
                }
                else
                {
                    skyDiveController = players[0].GetComponent<SkyDiveHandler>();
                }
            }
            return allReferencesOkay;
        }
    }
}
