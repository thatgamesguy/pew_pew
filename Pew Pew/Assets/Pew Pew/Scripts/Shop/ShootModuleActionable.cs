using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Adds new shoot modules, followers, or shields when purchased.
    /// </summary>
    public class ShootModuleActionable : ShopPurchaseActionableImpl
    {
        /// <summary>
        /// Tag name of object that contains the PlayerShootModules to be actioned.
        /// </summary>
        public string objectName;

        private PlayerShootModules m_Modules;

        protected override void Awake()
        {
            base.Awake();

            m_Modules = GameObject.FindGameObjectWithTag(objectName).GetComponent<PlayerShootModules>();
        }

        /// <summary>
        /// Determines whether this instance is actionable.
        /// </summary>
        /// <returns>true if PlayerShootModules::IsActionable</returns>
        /// <c>false</c>
        public override bool IsActionable()
        {
            return base.IsActionable() && m_Modules.IsActionable();
        }

        /// <summary>
        /// Enables new module.
        /// </summary>
        public override void DoAction()
        {
            base.DoAction();

            m_Modules.EnableNewModule();
        }

        /// <summary>
        /// Checks if this instance is actionable/purchasable. Returns true if not all instances have been purchased.
        /// </summary>
        public override void CheckActionable()
        {
            int numOfActionable = m_Modules.GetNumberOfActionableModules();
            int currentlyActive = pointsImages.GetNumberEnabled();
            int total = m_Modules.shootModules.Length;
            int toDisable = currentlyActive - (total - numOfActionable);

            if (toDisable > 0)
            {
                pointsImages.DisableImages(toDisable);
                m_MaxUses += toDisable;
                pointsText.text = cost.ToString();
            }

            base.CheckActionable();
        }
    }
}