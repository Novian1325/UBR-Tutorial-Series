﻿using System;
using UnityEngine;

namespace PolygonPilgrimage.BattleRoyaleKit
{
    [Serializable]//makes visible in Inspector!
    public struct ShrinkPhase
    {
        /// <summary>
        /// The amount of seconds that must pass before the Zone Wall begins shrinking.
        /// </summary>
        [Tooltip("How many seconds will pass until the Zone Wall begins shrinking.")]
        [SerializeField]
        private int secondsUntilShrinkBegins;

        public int SecondsUntilShrinkBegins { get => secondsUntilShrinkBegins; } // readonly

        /// <summary>
        /// The amount of seconds it takes for the Zone Wall to completely shrink through this shrink phase.
        /// </summary>
        [Tooltip("How many seconds it takes for the Zone Wall to completely shrink through this shrink phase.")]
        [SerializeField]
        private int secondsToFullyShrink;

        public int SecondsToFullyShrink { get => secondsToFullyShrink; } // readonly

        /// <summary>
        /// The radius at which the Zone Wall will stop shrinking this phase.
        /// </summary>
        [Tooltip("What radius should the Zone Wall stop shrinking at this phase?")]
        [SerializeField]
        private int shrinkToRadius;

        public int ShrinkToRadius { get => shrinkToRadius; } // readonly

        /// <summary>
        /// The frequency of the damage ticks per second.
        /// </summary>
        [Tooltip("The frequency of the damage ticks per second.")]
        [SerializeField]
        private float ticksPerSecond;

        public float TicksPerSecond { get => ticksPerSecond; } // readonly

        /// <summary>
        /// The amount of damage that is given every one time damage is dealt.
        /// </summary>
        [Tooltip("The amount of damage that is given every one time damage is dealt.")]
        [SerializeField]
        private float damagePerTick;

        public float DamagePerTick { get => damagePerTick; } // readonly

        /// <summary>
        /// Creates a new ShrinkPhase with the given options.
        /// </summary>
        /// <param name="secondsUntilShrinkBegins">The amount of seconds that must pass before the Zone Wall begins shrinking.</param>
        /// <param name="secondsToFullyShrink">The amount of seconds it takes for the Zone Wall to completely shrink through this shrink phase.</param>
        /// <param name="shrinkToRadius">The radius at which the Zone Wall will stop shrinking this phase.</param>
        /// <param name="ticksPerSecond">The frequency of the damage ticks per second.</param>
        /// <param name="damagePerTick">The amount of damage that is given every one time damage is dealt.</param>
        public ShrinkPhase(int secondsUntilShrinkBegins, 
            int secondsToFullyShrink, int shrinkToRadius, 
            int ticksPerSecond, float damagePerTick)
        {
            this.secondsUntilShrinkBegins = secondsUntilShrinkBegins;
            this.secondsToFullyShrink = secondsToFullyShrink;
            this.shrinkToRadius = shrinkToRadius;
            this.ticksPerSecond = ticksPerSecond;
            this.damagePerTick = damagePerTick;
        }
    }
}
