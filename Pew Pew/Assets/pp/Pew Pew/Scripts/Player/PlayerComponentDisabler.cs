using UnityEngine;
using System.Collections;

namespace GameCore
{
    /// <summary>
    /// Disables specified components when player is killed. Enables components when player is spawned.
    /// </summary>
    [RequireComponent(typeof(PlayerHealth))]
    public class PlayerComponentDisabler : MonoBehaviour
    {
        /// <summary>
        /// The components to enable/disable.
        /// </summary>
        public GameObject[] components;

        private PlayerHealth m_PlayerHealth;

        void Awake()
        {
            m_PlayerHealth = GetComponent<PlayerHealth>();
        }

        void OnEnable()
        {
            m_PlayerHealth.OnDeath += DisableComponents;
            m_PlayerHealth.OnSpawn += EnableComponents;
        }

        void OnDisable()
        {
            m_PlayerHealth.OnDeath -= DisableComponents;
            m_PlayerHealth.OnSpawn -= EnableComponents;
        }

        private void DisableComponents()
        {
            foreach (var component in components)
            {
                component.SetActive(false);
            }
        }

        private void EnableComponents()
        {
            foreach (var component in components)
            {
                component.SetActive(true);
            }
        }
    }
}