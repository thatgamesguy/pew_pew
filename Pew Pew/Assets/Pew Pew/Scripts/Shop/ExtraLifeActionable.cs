using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Provides player with an extra life when purchased.
    /// </summary>
    public class ExtraLifeActionable : ShopPurchaseActionableImpl
    {
        private PlayerHealth m_PlayerHealth;

        protected override void Awake()
        {
            base.Awake();
            m_PlayerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>();
        }

        /// <summary>
        /// Increments players life.
        /// </summary>
        public override void DoAction()
        {
            base.DoAction();
            m_PlayerHealth.IncrementLives();
        }
    }
}