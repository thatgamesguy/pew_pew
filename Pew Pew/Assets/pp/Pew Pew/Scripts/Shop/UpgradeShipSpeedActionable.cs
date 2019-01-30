using UnityEngine;
using System.Collections;

namespace GameCore
{
    /// <summary>
    /// Increases the players movement speed when purchased. 
    /// </summary>
    public class UpgradeShipSpeedActionable : ShopPurchaseActionableImpl
    {
        /// <summary>
        /// The amount to increase players movement speed.
        /// </summary>
        public float speedIncrement;

        private PlayerController m_PlayerController;

        protected override void Awake()
        {
            base.Awake();
            m_PlayerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        }

        /// <summary>
        /// Invokes PlayerController::IncrementSpeed.
        /// </summary>
        public override void DoAction()
        {
            base.DoAction();
            m_PlayerController.IncrementSpeed(speedIncrement);
        }
    }
}