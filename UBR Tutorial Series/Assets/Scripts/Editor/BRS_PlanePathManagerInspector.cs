using UnityEngine;
using UnityEditor;

namespace PolygonPilgrimage.BattleRoyaleKit
{
    [CustomEditor(typeof(BRS_PlanePathManager))]
    public class BRS_PlanePathManagerInspector : Editor
    {
        public Texture2D splashTexture;

        private BRS_PlanePathManager inspectedTarget;

        private void OnEnable()
        {
            inspectedTarget = target as BRS_PlanePathManager;
        }

        public override void OnInspectorGUI()
        {
            //draw cool image
            Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(100f));
            GUI.DrawTexture(rect, splashTexture, ScaleMode.ScaleToFit, true, 0f);

            base.OnInspectorGUI(); // normal stuff

            //buttons
            GUILayout.Space(10);

            if (GUILayout.Button("Show Drop Zones"))
            {
                inspectedTarget.ShowDropZoneRenderers();
            }

            if (GUILayout.Button("Hide Drop Zones"))
            {
                inspectedTarget.HideDropZoneRenderers();
            }
        }
    }

}
