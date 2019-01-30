using UnityEngine;
using System;
using System.Collections.Generic;
using PE2D;

namespace GameCore
{
    /// <summary>
    /// Contract for any entity responsible for tracking enemies in a round.
    /// </summary>
    public interface RoundOwner
    {
        /// <summary>
        /// Registers enemy removed from round.
        /// </summary>
        /// <param name="enemy">Enemy.</param>
        void RemoveEnemyFromRound(RoundEnemy enemy);

        /// <summary>
        /// Registers enemy escaped round.
        /// </summary>
        /// <param name="enemy">Enemy.</param>
        void EnemyEscapedRound(RoundEnemy enemy);
    }

    /// <summary>
    /// Holds all stationary enemies within a round.
    /// </summary>
    public class StationaryContainer : RoundOwner
    {
        private List<RoundEnemy> m_StationaryEnemies = new List<RoundEnemy>();

        /// <summary>
        /// Adds an enemy to structure.
        /// </summary>
        /// <param name="e">Enemy to add.</param>
        public void AddEnemy(RoundEnemy e)
        {
            m_StationaryEnemies.Add(e);
        }

        /// <summary>
        /// Removes stationary enemy from round.
        /// </summary>
        /// <param name="enemy">Enemy to remove.</param>
        public void RemoveEnemyFromRound(RoundEnemy enemy)
        {
            m_StationaryEnemies.Remove(enemy);
        }

        /// <summary>
        /// Removes stationary enemy from round.
        /// </summary>
        /// <param name="enemy">Enemy to remove.</param>
        public void EnemyEscapedRound(RoundEnemy enemy)
        {
            m_StationaryEnemies.Remove(enemy);
        }

        /// <summary>
        /// The current number of stationary enemies in the round.
        /// </summary>
        /// <returns>The enemy count.</returns>
        public int GetEnemyCount()
        {
            return m_StationaryEnemies.Count;
        }

        /// <summary>
        /// Gets the alive enemies in the round.
        /// </summary>
        /// <returns>The alive enemies.</returns>
        public List<EnemyHealth> GetAliveEnemies()
        {
            var enemies = new List<EnemyHealth>();

            foreach (var s in m_StationaryEnemies)
            {
                var health = s.myTransform.GetComponent<EnemyHealth>();

                if (health)
                {
                    enemies.Add(health);
                }
            }

            return enemies;
        }
    }

    /// <summary>
    /// Responsible for round progression: starting and signifying to the GameManager that the round is complete.
    /// </summary>
    public class Round : MonoBehaviour, RoundOwner
    {
        /// <summary>
        /// Invoked every time an enemy is removed.
        /// </summary>
        public Action onEnemyRemoved;

        /// <summary>
        /// Round types.
        /// </summary>
        public enum RoundType
        {
            Wave,
            Challenge,
            Boss
        }

        /// <summary>
        /// The type of the round.
        /// </summary>
        public RoundType roundType = RoundType.Wave;

        /// <summary>
        /// Gets the number of enemies first spawned.
        /// </summary>
        /// <value>The max enemies.</value>
        public int maxEnemies { get { return m_MaxEnemies; } }

        /// <summary>
        /// Gets the number of enemies remaining.
        /// </summary>
        /// <value>The enemies remaining.</value>
        public int enemiesRemaining { get { return m_EnemyCount; } }

        /// <summary>
        /// All non-stationary enemies within the round. Important: enemies that are destroyed are not removed from this list. Null checks are required.
        /// </summary>
        public List<EnemyHealth> enemies = new List<EnemyHealth>();

        private int m_EnemyCount;
        private RoundManagement m_GameManager;
        private EnemyMove[] m_EnemiesMove;
        private EnemyShootStatusChange[] m_EnemiesShoot;
        private int m_MaxEnemies;
        private int m_EnemiesKilled;
        private StationaryContainer m_StationaryEnemies;

        void Awake()
        {
            m_GameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<RoundManagement>();
            m_EnemiesMove = GetComponentsInChildren<EnemyMove>();
            m_EnemiesShoot = GetComponentsInChildren<EnemyShootStatusChange>();

            var roundEnemies = GetComponentsInChildren<RoundEnemy>();

            m_EnemyCount = 0;

            m_StationaryEnemies = new StationaryContainer();

            foreach (var enemy in roundEnemies)
            {
                bool registerAsStationary = true;

                var stationary = enemy.myTransform.GetComponent<StationaryMovement>();
                if (stationary == null)
                {
                    var effector = enemy.myTransform.GetComponent<ParticleEffector>();

                    if (effector == null)
                    {
                        registerAsStationary = false;
                    }

                    enemies.Add(enemy.myTransform.GetComponent<EnemyHealth>());
                }

                if (registerAsStationary)
                {
                    m_StationaryEnemies.AddEnemy(enemy);
                    enemy.RegisterRoundOwner(m_StationaryEnemies);
                }
                else
                {
                    enemy.RegisterRoundOwner(this);
                    m_EnemyCount++;
                }
            }

            m_MaxEnemies = m_EnemyCount;
        }

        /// <summary>
        /// Starts the round. Calls each enemies respective begin methods. Delays starting shooting for 1 second.
        /// </summary>
        public void StartRound()
        {
            foreach (var enemy in m_EnemiesMove)
            {
                enemy.Begin();
            }

            Invoke("StartShooting", 1f);
        }

        /// <summary>
        /// Registers enemy removed from round.
        /// </summary>
        /// <param name="enemy">Enemy.</param>
        public void RemoveEnemyFromRound(RoundEnemy enemy)
        {
            m_EnemyCount--;
            m_EnemiesKilled++;

            OnEnemyRemoved();
        }

        /// <summary>
        /// Registers enemy escaped round.
        /// </summary>
        /// <param name="enemy">Enemy.</param>
        public void EnemyEscapedRound(RoundEnemy enemy)
        {
            m_EnemyCount--;

            OnEnemyRemoved();
        }

        private void OnEnemyRemoved()
        {
            if (onEnemyRemoved != null)
            {
                onEnemyRemoved();
            }

            if (m_EnemyCount <= 0)
            {
                OnRoundOver();
            }
        }

        private void OnRoundOver()
        {
            if (m_StationaryEnemies.GetEnemyCount() > 0)
            {
                var enemies = m_StationaryEnemies.GetAliveEnemies();

                for (int i = enemies.Count - 1; i >= 0; --i)
                {
                    enemies[i].Kill(false);
                }
            }

            if (roundType == RoundType.Challenge)
            {
                m_GameManager.OnChallengeRoundOver(m_EnemiesKilled, m_MaxEnemies);
            }
            else if (roundType == RoundType.Wave)
            {
                m_GameManager.OnRoundOver();
            }
            else if (roundType == RoundType.Boss)
            {
                m_GameManager.OnRoundOver();
            }

            Destroy(gameObject);
        }

        private void StartShooting()
        {
            foreach (var enemy in m_EnemiesShoot)
            {
                enemy.Begin();
            }
        }
    }
}