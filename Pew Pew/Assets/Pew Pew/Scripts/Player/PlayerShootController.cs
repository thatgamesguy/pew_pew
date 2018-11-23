using UnityEngine;
using System.Collections;

namespace GameCore
{
    /// <summary>
    /// Controls all player weapons. Enables the pausing and resuming of shooting i.e. between rounds, or when the player dies/respawns.
    /// </summary>
    public class PlayerShootController : MonoBehaviour
    {
        private PlayerShoot[] m_PlayerShoots;

        void Awake()
        {
            m_PlayerShoots = GetComponentsInChildren<PlayerShoot>();
        }

        /// <summary>
        /// Begins the shooting of each weapon.
        /// </summary>
        public void BeginShooting()
        {
            foreach (var shoot in m_PlayerShoots)
            {
                if (shoot.gameObject.activeInHierarchy)
                {
                    shoot.BeginShooting();
                }
            }
        }

        /// <summary>
        /// Pauses all.
        /// </summary>
        public void PauseAll()
        {
            foreach (var shoot in m_PlayerShoots)
            {
                shoot.Pause();
            }
        }

        /// <summary>
        /// Resumes all.
        /// </summary>
        public void ResumeAll()
        {
            foreach (var shoot in m_PlayerShoots)
            {
                shoot.Resume();
            }
        }
    }
}