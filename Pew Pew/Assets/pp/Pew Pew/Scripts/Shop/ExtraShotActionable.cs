using UnityEngine;
using System.Collections;

namespace GameCore
{
    /// <summary>
    /// Provides the player with an extra burst shot when purchased.
    /// </summary>
    public class ExtraShotActionable : ShopPurchaseActionableImpl
    {
        public PlayerShoot playerShoot;

        /// <summary>
        /// Increments players burst shot.
        /// </summary>
        public override void DoAction()
        {
            base.DoAction();

            playerShoot.IncrementShotBurst();
        }
    }
}