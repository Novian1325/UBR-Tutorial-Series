using UnityEngine;
using UnityEditor;

namespace PolygonPilgrimage.BattleRoyaleKit
{
    [CustomEditor(typeof(SupplyDropManager))]
    public class BRS_AirDropManagerInspector : Editor
    {
        private SupplyDropManager inspectedInstance;

        private void OnEnable()
        {
            inspectedInstance = target as SupplyDropManager;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            //buttons 
            GUILayout.Space(10);
            
            if(GUILayout.Button("Trigger Supply Drop"))
            {
                inspectedInstance.DeploySupplyDrop();
            }
        }

    }

}
