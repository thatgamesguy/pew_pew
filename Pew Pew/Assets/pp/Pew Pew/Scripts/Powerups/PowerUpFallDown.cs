using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Attached to each powerup. Enables powerups to fall into a position where they can be picked up by the player.
    /// </summary>
    public class PowerUpFallDown : MonoBehaviour
    {
        /// <summary>
        /// The negative y movement speed.
        /// </summary>
        public float movementSpeed = 1f;

        /// <summary>
        /// The target y position.
        /// </summary>
        public float minY = -2.9f;

        private static PauseHandler PAUSE_HANDLER;

        private bool m_ShouldUpdate = true;

        private void Awake()
        {
            if (!PAUSE_HANDLER)
            {
                PAUSE_HANDLER = GameObject.FindGameObjectWithTag("UI").GetComponent<PauseHandler>();
            }
        }

        void OnEnable()
        {
            m_ShouldUpdate = true;
        }

        void Update()
        {
            if (!PAUSE_HANDLER.isPaused && m_ShouldUpdate)
            {
                transform.position += Vector3.down * movementSpeed * Time.deltaTime;

                if (transform.position.y <= minY)
                {
                    m_ShouldUpdate = false;
                }
            }
        }
    }
}