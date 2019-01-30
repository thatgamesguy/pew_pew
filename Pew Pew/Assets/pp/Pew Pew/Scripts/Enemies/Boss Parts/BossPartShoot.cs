using UnityEngine;
using System.Collections;
using System;

namespace GameCore
{
    /// <summary>
    /// Controls the boss part shoot.
    /// </summary>
    public class BossPartShoot : BossPartImpl
    {
        private EnemyShoot[] m_EnemyShoots;

        protected override void Awake()
        {
            base.Awake();
            m_EnemyShoots = GetComponentsInChildren<EnemyShoot>();
        }

        protected override void Start()
        {
            foreach (var shoot in m_EnemyShoots)
            {
                shoot.StopActivation();
            }

            base.Start();
        }

        /// <summary>
        /// Pause this instance.
        /// </summary>
        public override void Pause()
        {
            foreach (var shoot in m_EnemyShoots)
            {
                shoot.Pause();
            }
        }

        /// <summary>
        /// Resume this instance.
        /// </summary>
        public override void Resume()
        {
            if (m_HasBeenActivated)
            {
                foreach (var shoot in m_EnemyShoots)
                {
                    shoot.Resume();
                }
            }
        }

        protected override void DoActivation()
        {
            foreach (var shoot in m_EnemyShoots)
            {
                shoot.Begin();
            }
        }
    }
}