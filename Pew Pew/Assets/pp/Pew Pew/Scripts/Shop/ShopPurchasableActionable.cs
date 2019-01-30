using UnityEngine;
using UnityEngine.UI;

namespace GameCore
{
    /// <summary>
    /// Contract for any item that can be purchased in the shop. Provides methods for performing action, checking if all actions have been performed, and checking if action can be performed.
    /// </summary>
    public interface ShopPurchaseActionable
    {
        /// <summary>
        /// Determines whether this instance is actionable.
        /// </summary>
        /// <returns><c>true</c> if this instance is actionable; otherwise, <c>false</c>.</returns>
        bool IsActionable();

        /// <summary>
        /// Performs the action.
        /// </summary>
        void DoAction();

        /// <summary>
        /// Checks if this instance is actionable/purchasable.
        /// </summary>
        void CheckActionable();

        /// <summary>
        /// Checks if all items have been purchased.
        /// </summary>
        void CheckComplete();
    }

    /// <summary>
    /// Base class for any shop purchase items. Provides access to and manipulation of all common shop purchase features, including:
    /// PointsImages, foreground overlay (enabled when item not currently purchasable), the text that displays the item cost, and the cost value.
    /// </summary>
    public class ShopPurchaseActionableImpl : MonoBehaviour, ShopPurchaseActionable
    {
        /// <summary>
        /// Image used to overlay on a shop item when it can't be purchased.
        /// </summary>
        public GameObject foreground;

        /// <summary>
        /// The images used to show how many instances of this item have been purchased.
        /// </summary>
        public PointsImages pointsImages;

        /// <summary>
        /// The text showing the cost of the item.
        /// </summary>
        public Text pointsText;

        /// <summary>
        /// The cost of purchasing the first instance of this item. Each subsequent purchase doubles in cost.
        /// </summary>
        public int cost = 10;

        protected int m_MaxUses;

        private Score m_Score;

        protected virtual void Awake()
        {
            m_Score = GameObject.FindGameObjectWithTag("UI").GetComponentInChildren<Score>();
            m_MaxUses = pointsImages.images.Length;
            pointsText.text = cost.ToString();
        }

        /// <summary>
        /// Checks if this instance is actionable/purchasable. If not actionable, an image is overlayed.
        /// </summary>
        public virtual void CheckActionable()
        {
            foreground.SetActive(!IsActionable());
        }

        /// <summary>
        /// Checks if all items have been purchased. If true, the cost text is set to '-'
        /// </summary>
        public void CheckComplete()
        {
            if (m_MaxUses == 0)
            {
                pointsText.text = "-";
            }
        }

        /// <summary>
        /// Determines whether this instance is actionable. True if the player can afford to purchase and it is still has instances remaining.
        /// </summary>
        /// <returns>true</returns>
        /// <c>false</c>
        public virtual bool IsActionable()
        {
            return m_Score.score >= cost && m_MaxUses > 0;
        }

        public virtual void DoAction()
        {
            m_MaxUses--;
            pointsImages.EnableNextPointImage();
            m_Score.RemoveScore(cost);
            cost *= 2;
            pointsText.text = cost.ToString();
        }
    }
}