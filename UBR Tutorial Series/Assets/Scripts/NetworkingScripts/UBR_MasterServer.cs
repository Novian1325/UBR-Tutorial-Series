using System;
using UnityEngine;
using Bolt;
using Bolt.Matchmaking;
using UdpKit;

namespace PolygonPilgrimage.BattleRoyaleKit.Networking
{
    public class UBR_MasterServer : GlobalEventListener
    {
        [SerializeField]
        private string roomName = null;

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, Screen.width - 20, Screen.height - 20));

            if(GUILayout.Button("Init Master Server", 
                GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
            {
                BoltLauncher.StartServer(); // Let it begin!
            }

            GUILayout.EndArea();
        }

        public override void BoltStartDone()
        {
            //Bolt has finished loading. Create a new session.
            var matchName = !String.IsNullOrEmpty(roomName) ? roomName : // use name if it exists
                Guid.NewGuid().ToString(); // otherwise generate a unique ID

            BoltMatchmaking.CreateSession(
                sessionID: matchName, // set room name (can be anything!)
                sceneToLoad: "NetworkTest"); // which scene to load
        }

        public override void SessionListUpdated(Map<Guid, UdpSession> sessionList)
        {
            Debug.LogFormat("Session list updated: {0} total sessions", sessionList.Count);

            foreach (var session in sessionList)
            {
                var photonSession = session.Value as UdpSession;

                if (photonSession.Source == UdpSessionSource.Photon)
                {
                    BoltMatchmaking.JoinSession(photonSession);
                }
            }
        }
    }
}
