using UnityEngine;
using System.Collections;

namespace GameCore
{
    /// <summary>
    /// Data class for powerup spawns.
    /// </summary>
    [System.Serializable]
    public class PowerUpSpawn
    {
        /// <summary>
        /// The prefab to spawn.
        /// </summary>
        public GameObject powerUpPrefab;

        /// <summary>
        /// The relative chance to spawn this powerup.
        /// </summary>
        public float weight;
    }

    /// <summary>
    /// Spawns a powerup when this an entity with this script attached dies.
    /// </summary>
    [RequireComponent(typeof(EnemyHealth))]
    public class DropPowerUpOnDeath : MonoBehaviour
    {
        /// <summary>
        /// The power ups that can be spawned. Consists of prefab and weighted spawn chance.
        /// </summary>
        public PowerUpSpawn[] powerUps;

        private static GameManager GAME_MANAGER;

        private EnemyHealth m_EnemyHealth;
        private float m_TotalWeight;

        void Awake()
        {
            m_EnemyHealth = GetComponent<EnemyHealth>();

            if (GAME_MANAGER == null)
            {
                GAME_MANAGER = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
            }
        }

        void Start()
        {
            CalculateTotalWeight();
        }

        void OnEnable()
        {
            m_EnemyHealth.onDeath += SpawnPowerUp;
        }

        void OnDisable()
        {
            m_EnemyHealth.onDeath -= SpawnPowerUp;
        }

        private void CalculateTotalWeight()
        {
            foreach (var spawn in powerUps)
            {
                m_TotalWeight += spawn.weight;
            }
        }

        private void SpawnPowerUp()
        {
            if (GAME_MANAGER != null)
            {
                int index = GAME_MANAGER.currentRound.enemiesRemaining < 2 ? 0 : GetSpawnIndex();

                Instantiate(powerUps[index].powerUpPrefab, transform.position, Quaternion.identity);
            }
        }

        private int GetSpawnIndex()
        {

            var randomIndex = -1;
            var random = Random.value * m_TotalWeight;

            for (int i = 0; i < powerUps.Length; ++i)
            {
                random -= powerUps[i].weight;

                if (random <= 0f)
                {
                    randomIndex = i;
                    break;
                }
            }

            return randomIndex;
        }
    }
}