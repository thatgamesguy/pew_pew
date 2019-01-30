using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Persistently stores and retrieves the highest round the player has reached. Data is stored in PlayerPrefs.
    /// </summary>
    public class RoundPersistentScore : MonoBehaviour
    {
        /// <summary>
        /// Gets the highest round achieved by the player.
        /// </summary>
        /// <value>The highest round.</value>
        public int highestRound { get; private set; }

        private static readonly string ROUND_KEY = "rounds";

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            highestRound = PlayerPrefs.GetInt(ROUND_KEY, 0);
        }

        /// <summary>
        /// Sets round player has reached.
        /// </summary>
        /// <param name="round">Round player has reached.</param>
        public void SetRound(int round)
        {
            if (round > highestRound)
            {
                highestRound = round;
                PlayerPrefs.SetInt(ROUND_KEY, highestRound);
            }
        }
    }
}