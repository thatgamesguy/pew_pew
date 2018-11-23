using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Provides player with an extra bomb when purchased.
    /// </summary>
    public class ExtraBombActionable : ShopPurchaseActionableImpl
    {
        private BombManager m_Bombs;

        protected override void Awake()
        {
            base.Awake();

            m_Bombs = GameObject.FindObjectOfType<BombManager>();
        }

        /// <summary>
        /// Increments bomb count.
        /// </summary>
        public override void DoAction()
        {
            base.DoAction();

            m_Bombs.IncrementBombCount();
        }
    }
}