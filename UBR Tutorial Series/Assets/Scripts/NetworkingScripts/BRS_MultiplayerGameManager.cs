using UnityEngine;
using Bolt;

namespace PolygonPilgrimage.BattleRoyaleKit.Networking
{
    public class BRS_MultiplayerGameManager : GlobalEventListener
    {
        /// <summary>
        /// Where the Players are when the game starts.
        /// </summary>
        [Header("GameSettings")]
        [Tooltip("Where should the Players be when the game starts?")]
        [SerializeField] private DeployPlayersMode deployPlayersMode = DeployPlayersMode.OnGround;

        [Tooltip("Players in the game. If not a single player is found, a search will be done for all objects tagged \"Player\" in scene.")]
        [SerializeField] private GameObject[] players;

        [Header("SkyDiving")]
        [Tooltip("This is the height the Player will start at if \"startSkyDiving\" is true.")]
        [SerializeField] private int skyDiveTestHeight = 500;

        /// <summary>
        /// Bolt's version of Awake()
        /// </summary>
        public override void SceneLoadLocalDone(string sceneName,
            IProtocolToken token)
        {
            Debug.Log("Attached", this);
            //spawn game objects:
            //spawn the Zone Wall
            BoltNetwork.Instantiate(BoltPrefabs.UBR_Zone_Manager_Network, 
                Vector3.zero, Quaternion.identity); // center of the world
        }

    }

}
