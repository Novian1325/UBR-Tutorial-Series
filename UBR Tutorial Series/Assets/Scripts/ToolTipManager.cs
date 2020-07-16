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

        [SerializeField] private GameObject interactToolTip;

        private static GameObject InteractPrompt { get => Instance.interactToolTip; }

        //containers for keys and objects
        private static Dictionary<ToolTipENUM, GameObject> toolTips = new Dictionary<ToolTipENUM, GameObject>();
        private static List<ToolTipENUM> toolTipKeys = new List<ToolTipENUM>();

        private void InitToolTips()
        {
            //verify that each prompt exists. If it does, add it to the list for tracking
            if (interactToolTip)
            {
                toolTips.Add(ToolTipENUM.INTERACT, skydivePrompt); // track object
                toolTipKeys.Add(ToolTipENUM.INTERACT); // track key
            }

            if (skydivePrompt)
            {
                toolTips.Add(ToolTipENUM.SKYDIVE, skydivePrompt); // track object
                toolTipKeys.Add(ToolTipENUM.SKYDIVE); // track key
            }

            if (deployParachutePrompt)
            {
                toolTips.Add(ToolTipENUM.DEPLOYPARACHUTE, deployParachutePrompt); // track object
                toolTipKeys.Add(ToolTipENUM.DEPLOYPARACHUTE); // track key
            }
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
        
        private static void SingletonPattern(ToolTipManager instance)
        {
            if (!Instance)
            {
                Instance = instance;
            }
            else
            {
                Destroy(instance.gameObject);
            }
        }

        public static void DisableAllToolTips()
        {
            //iterating through a dictionary sucks, so iterate through a list of keys
            for (var i = 0; i < toolTips.Count; ++i)
            {
                var key = toolTipKeys[i]; // get key to get from dictionary

                var tip = toolTips[key]; // get tip object out of dictionary

                tip.SetActive(false); // hide it
            }
        }

        public static void ShowToolTip(ToolTipENUM toolTipKey, bool active)
        {
            GameObject tip;

            if(toolTips.TryGetValue(toolTipKey, out tip))
            {
                DisableAllToolTips();
                tip.SetActive(active);
            }
            else
            {
                Debug.LogError("[ToolTipManager] Key not in dictionary: " 
                    + toolTipKey.ToString());
            }
        }
    }
}
