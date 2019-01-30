using UnityEngine;
using System.Collections;

namespace GameCore
{
    /// <summary>
    /// Decreases time between shots for player when purchased.
    /// </summary>
    public class UpgradeShootSpeedActionable : ShopPurchaseActionableImpl
    {
        /// <summary>
        /// The amount (in seconds) to decrease the time between player shots.
        /// </summary>
        public float secsBetweenShotDecrements = 0.01f;

        private GameObject m_Player;

        protected override void Awake()
        {
            base.Awake();
            m_Player = GameObject.FindGameObjectWithTag("Player");
        }

        /// <summary>
        /// Decrements shoot speed for all PlayerShoot modules attached to the player.
        /// </summary>
        public override void DoAction()
        {
            base.DoAction();

            var shootModules = m_Player.GetComponentsInChildren<PlayerShoot>();

            foreach (var module in shootModules)
            {
                module.DecrementSecBetweenShots(secsBetweenShotDecrements);
            }
        }
    }
}