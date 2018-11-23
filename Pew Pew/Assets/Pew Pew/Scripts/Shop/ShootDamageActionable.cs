using UnityEngine;
using System.Collections;

namespace GameCore
{
    /// <summary>
    /// Increases the damage of players projectiles by one when purchased.
    /// </summary>
    public class ShootDamageActionable : ShopPurchaseActionableImpl
    {
        /// <summary>
        /// The player shoot instance.
        /// </summary>
        public PlayerShoot playerShoot;

        /// <summary>
        /// Increments player damage by calling PlayerShot::IncrementDamage.
        /// </summary>
        public override void DoAction()
        {
            base.DoAction();

            playerShoot.IncrementDamage();
        }
    }
}