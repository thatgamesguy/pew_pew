using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Starts current round and provides functionality to begin next round.
    /// </summary>
    public class RoundManager : MonoBehaviour
    {
        /// <summary>
        /// The text shown when a new round starts.
        /// </summary>
        public RoundText roundText;

        /// <summary>
        /// A list of possible round prefabs.
        /// </summary>
        public GameObject[] roundPrefabs;

        /// <summary>
        /// Gets the current round.
        /// </summary>
        /// <value>The current round.</value>
        public int currentRound { get { return m_RoundIndex + 1; } }

        private int m_RoundIndex;
        private bool m_Complete;

        /// <summary>
        /// Begin this instance.
        /// </summary>
        public void Begin()
        {
            if (roundPrefabs.Length > 0)
            {
                m_RoundIndex = 0;
                BeginBurrentRound();
            }
        }

        /// <summary>
        /// Begins the next round.
        /// </summary>
        public void BeginNextRound()
        {
            if (!m_Complete)
            {
                m_RoundIndex = (m_RoundIndex + 1) % roundPrefabs.Length;

                m_Complete = m_RoundIndex == 0;

                if (!m_Complete)
                {
                    BeginBurrentRound();
                }
            }
        }

        private void BeginBurrentRound()
        {
            var roundObj = (GameObject)Instantiate(roundPrefabs[m_RoundIndex]);
            roundObj.transform.SetParent(transform);
        }
    }
}