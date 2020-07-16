using System.Text;
using UnityEngine;

namespace PolygonPilgrimage.BattleRoyaleKit
{
    [RequireComponent(typeof(Collider))]
    //[RequireComponent(typeof(BRS_Trackable))] // optional
    public class BRS_Interactable : RichMonoBehaviour
    {
        [Header("---Interactable---")]
        
        /// <summary>
        /// Trackable behavior that may be attached to this gameObject.
        /// </summary>
        private BRS_Trackable trackable;

        /// <summary>
        /// Used to display ToolTip.
        /// </summary>
        protected bool playerIsLookingAtObject = false;

        protected override void GatherReferences()
        {
            base.GatherReferences();
            trackable = gameObject.GetComponent<BRS_Trackable>() as BRS_Trackable; //may or may not exist
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            //Handle Tooltips
            HandleTooltip();
            //set to false to verfiy next frame
            playerIsLookingAtObject = false;
        }

        /// <summary>
        /// Turns Tooltip Object on or off depending on it being looked at by the Player.
        /// </summary>
        protected virtual void HandleTooltip()
        {
            //toggle tooltip
            ToolTipManager.ShowToolTip(ToolTipENUM.INTERACT, playerIsLookingAtObject);
        }

        /// <summary>
        /// Tells Compass to stop tracking this object
        /// </summary>
        protected void RemoveTrackableFromCompass()
        {
            trackable?.RemoveTrackable();
        }

        /// <summary>
        /// Base interact method. Sends log to Console if not overridden by derived class.
        /// </summary>
        /// <param name="actor">Object, probably player or AI, that is the actor.</param>
        public virtual void Interact(BRS_InteractionManager actor)
        {
            //this method should probably be overridden by derived class, ie a vehicle should do something that an item does not
            var stringBuilder = new StringBuilder();

            stringBuilder.Append(actor.gameObject.name);
            stringBuilder.Append(" is interacting with ");
            stringBuilder.Append(this.gameObject.name);

            Debug.Log(stringBuilder.ToString());
        }

        /// <summary>
        /// Tells this object whether or not the Player is pointing at it.
        /// </summary>
        /// <param name="b"></param>
        public virtual void PlayerIsLookingAtObject(bool b)
        {
            playerIsLookingAtObject = b;
        }

        /// <summary>
        /// Whether or not the Player is looking at this Object.
        /// </summary>
        /// <returns></returns>
        public virtual bool GetPlayerIsLookingAtObject()
        {
            return playerIsLookingAtObject;
        }
        
    }//end class declaration
}//end namespace
