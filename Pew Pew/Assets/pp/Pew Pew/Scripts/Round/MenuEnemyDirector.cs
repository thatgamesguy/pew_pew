using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Directs enemies as part of the main menu scene.
    /// </summary>
    public class MenuEnemyDirector : MonoBehaviour, RoundManagement
    {
        /// <summary>
        /// Possible enemies that can be spawned during the main menu scene. One is selected at random.
        /// </summary>
        public GameObject[] menuRounds;

        private int m_RoundIndex = 0;

        void Start()
        {
            StartCoroutine(_StartNewRound(1f));
        }

        /// <summary>
        /// Included to meet the contract outlined in RoundManagement. However it is not used during the menu scene.
        /// </summary>
        /// <param name="enemiesKilled">Ignored.</param>
        /// <param name="maxEnemies">Ignored.</param>
        public void OnChallengeRoundOver(int enemiesKilled, int maxEnemies)
        {
            // Empty.
        }

        /// <summary>
        /// Starts a new round.
        /// </summary>
        public void OnRoundOver()
        {
            StartNextRound();
        }

        private void StartNextRound()
        {
            m_RoundIndex = Random.Range(0, menuRounds.Length);
            var nextRound = GetNextRound();
            nextRound.StartRound();
        }

        private IEnumerator _StartNewRound(float seconds)
        {
            m_RoundIndex = Random.Range(0, menuRounds.Length);
            var nextRound = GetNextRound();

            yield return new WaitForSeconds(seconds);

            nextRound.StartRound();
        }

        private Round GetNextRound()
        {
            var roundObj = (GameObject)Instantiate(menuRounds[m_RoundIndex]);
            roundObj.transform.SetParent(transform);

            return roundObj.GetComponent<Round>();
        }
    }
}