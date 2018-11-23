using UnityEngine;
using System.Collections;

namespace GameCore
{
    /// <summary>
    /// Provides a player with a shield (or additional shield) when purchased. A player can have up to four active shields.
    /// When a shield is destroyed the player can purchase the item again (at an increased cost).
    /// </summary>
    public class ShieldActionable : ShopPurchaseActionableImpl
    {
        /// <summary>
        /// The shield to enable.
        /// </summary>
        public GameObject shield;

        /// <summary>
        /// Determines whether this instance is actionable.
        /// </summary>
        /// <returns>true if shield not already active.</returns>
        /// <c>false</c>
        public override bool IsActionable()
        {
            return base.IsActionable() && !shield.activeSelf;
        }

        public override void DoAction()
        {
            base.DoAction();

            shield.SetActive(true);
        }
    }
}