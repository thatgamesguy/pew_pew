using UnityEngine;
using System.Collections;

namespace GameCore
{
    /// <summary>
    /// Functionality for collecting and activating powerups.
    /// </summary>
    public class PowerUpCollector : MonoBehaviour
    {
        private PlayerHealth m_Health;
        private bool m_CanPickup = true;

        void Awake()
        {
            m_Health = GetComponent<PlayerHealth>();
        }

        void OnEnable()
        {
            m_Health.OnDeath += DisablePickup;
            m_Health.OnSpawn += EnablePickup;
        }

        void OnDisable()
        {
            m_Health.OnDeath -= DisablePickup;
            m_Health.OnSpawn -= EnablePickup;
        }

        private void EnablePickup()
        {
            m_CanPickup = true;
        }

        private void DisablePickup()
        {
            m_CanPickup = false;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (m_CanPickup && other.CompareTag("Upgrade"))
            {
                var powerups = other.GetComponents<PowerUp>();

                foreach (var powerup in powerups)
                {
                    powerup.Perform(transform);
                }
            }
        }
    }
}