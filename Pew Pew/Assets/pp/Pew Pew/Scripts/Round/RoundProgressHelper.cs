using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Ensures that if an enemy is offscreen for too long a period it is removed from the round.
    /// </summary>
    public class RoundProgressHelper : MonoBehaviour
    {
        /// <summary>
        /// The game manager. Used to retrieve current round enemies.
        /// </summary>
        public GameManager gameManager;

        private static readonly float ENEMY_PERCENT = 0.3f;
        private static readonly float WAIT_TIME = 4f;
        private static readonly float CHECK_WAIT_TIME = 3f;

        private List<EnemyHealth> m_RoundEnemies = new List<EnemyHealth>();
        private int m_MaxRoundEnemies;
        private List<EnemyHealth> m_EnemyChecks = new List<EnemyHealth>();
        private bool m_NewRound = false;

        void Start()
        {
            StartCoroutine(ProgessRounds());
        }

        void OnEnable()
        {
            gameManager.onRoundStart += GetRoundEnemies;
        }

        void OnDisable()
        {
            gameManager.onRoundStart -= GetRoundEnemies;
        }

        private void GetRoundEnemies()
        {
            if (gameManager.currentRound.roundType == Round.RoundType.Wave)
            {
                m_RoundEnemies = gameManager.currentRound.enemies;
                m_MaxRoundEnemies = m_RoundEnemies.Count;
            }
            else
            {
                m_RoundEnemies.Clear();
                m_MaxRoundEnemies = 0;
            }

            m_NewRound = true;
        }

        private IEnumerator ProgessRounds()
        {
            while (true)
            {
                if (m_RoundEnemies == null || m_RoundEnemies.Count == 0)
                {
                    yield return new WaitUntil(() => m_RoundEnemies != null
                                                && m_RoundEnemies.Count > 0);
                }

                // If enemies have already been killed remove them from collection.
                for (int i = m_RoundEnemies.Count - 1; i >= 0; --i)
                {
                    if (m_RoundEnemies[i] == null || m_RoundEnemies[i].gameObject == null)
                    {
                        m_RoundEnemies.RemoveAt(i);
                    }
                }

                if (m_RoundEnemies.Count <= (int)(m_MaxRoundEnemies * ENEMY_PERCENT))
                {
                    for (int i = m_RoundEnemies.Count - 1; i >= 0; --i)
                    {
                        if (OffScreen(m_RoundEnemies[i]))
                        {
                            m_EnemyChecks.Add(m_RoundEnemies[i]);
                        }
                    }

                    if (m_EnemyChecks.Count > 0)
                    {
                        m_NewRound = false;
                        StartCoroutine(CheckStillOffScreen());
                        yield return new WaitUntil(() => m_EnemyChecks.Count == 0);
                    }
                }


                yield return new WaitForSeconds(WAIT_TIME);

            }
        }

        private bool OffScreen(EnemyHealth e)
        {
            var pos = Camera.main.WorldToViewportPoint(e.transform.position);

            return pos.x < 0f || pos.x > 1f || pos.y < 0f || pos.y > 1f;
        }

        private IEnumerator CheckStillOffScreen()
        {
            yield return new WaitForSeconds(CHECK_WAIT_TIME);

            if (m_NewRound)
            {
                m_NewRound = false;
                m_EnemyChecks.Clear();
                yield return null;
            }

            for (int i = m_EnemyChecks.Count - 1; i >= 0; --i)
            {
                if (m_EnemyChecks[i] == null || m_EnemyChecks[i].gameObject == null)
                {
                    continue;
                }

                if (OffScreen(m_EnemyChecks[i]))
                {
                    m_RoundEnemies.Remove(m_EnemyChecks[i]);
                    m_EnemyChecks[i].Kill(false);
                }
            }

            m_EnemyChecks.Clear();
        }
    }
}