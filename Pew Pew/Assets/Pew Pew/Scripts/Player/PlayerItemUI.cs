using UnityEngine;
using UnityEngine.UI;

namespace GameCore
{
    /// <summary>
    /// Shows number of lives and bombs on the in-game UI the player currently has.
    /// </summary>
    public class PlayerItemUI : MonoBehaviour
    {
        /// <summary>
        /// The text object used to display the number of lives/bombs.
        /// </summary>
        public Text livesText;

        private static readonly string PRE_TEXT = "x";

        /// <summary>
        /// Updates the count in the UI.
        /// </summary>
        /// <param name="count">New count.</param>
        public void SetItemCount(int count)
        {
            livesText.text = PRE_TEXT + count.ToString();
        }
    }
}