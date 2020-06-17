using System.Collections.Generic;
using UnityEngine;

namespace PolygonPilgrimage.BattleRoyaleKit
{
    /// <summary>
    /// UI Controller that handles prompts to Player
    /// </summary>
    public class ToolTipManager : RichMonoBehaviour
    {
        public static ToolTipManager Instance { get; private set; }//for singleton pattern

        [SerializeField] private GameObject skydivePrompt;

        private static GameObject SkyDivePrompt { get => Instance.skydivePrompt; }

        [SerializeField] private GameObject deployParachutePrompt;

        private static GameObject DeployParachutePrompt { get => Instance.deployParachutePrompt; }

        private List<GameObject> toolTips = new List<GameObject>();

        private void InitToolTips()
        {
            //verify that each prompt exists. If it does, add it to the list for tracking
            if (skydivePrompt) toolTips.Add(skydivePrompt);
            if (deployParachutePrompt) toolTips.Add(deployParachutePrompt);
        }

        protected override void Awake()
        {
            base.Awake();

            //singleton pattern
            SingletonPattern(this); //ensures only one exists, if any

            //verify and init tooltips
            InitToolTips();

            //all tooltips should start disabled
            DisableAllToolTips();
        }
        
        private static void SingletonPattern(ToolTipManager TTM_instance)
        {
            if (!Instance)
            {
                Instance = TTM_instance;
            }
            else
            {
                Destroy(TTM_instance.gameObject);
            }
        }

        public void DisableAllToolTips()
        {
            for (var i = 0; i < toolTips.Count; ++i)
            {
                var tip = toolTips[i];
                tip.SetActive(false);
            }
        }

        public static void ShowToolTip(ToolTipENUM toolTip, bool active)
        {
            switch (toolTip)
            {
                case ToolTipENUM.DEPLOYPARACHUTE:
                    if (DeployParachutePrompt) DeployParachutePrompt.SetActive(active);
                    break;

                case ToolTipENUM.SKYDIVE:
                    if (SkyDivePrompt) SkyDivePrompt.SetActive(active);
                    break;

                default:
                    break;
            }
        }
    }
}
