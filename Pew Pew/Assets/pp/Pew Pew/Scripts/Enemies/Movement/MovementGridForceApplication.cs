using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarpGrid;

namespace GameCore
{
    /// <summary>
    /// Applies a directional force to the background grid based on owners direction and velocity.
    /// </summary>
    public class MovementGridForceApplication : MonoBehaviour
    {
        /// <summary>
        /// The force radius.
        /// </summary>
        public float radius = 1f;

        /// <summary>
        /// The multiplier to apply to velocity.
        /// </summary>
        public float forceMultiplier = 1f;

        private static readonly float VELOCITY_MULTI = 0.0015f;
        private static readonly float UPDATE_INTERVAL = 0.33f;

        private Vector2 m_LastPosition;
        private float m_Time;

        void OnEnable()
        {
            m_LastPosition = transform.position;
        }

        void Update()
        {
            if (Time.timeScale == 0f)
            {
                return;
            }

            m_Time += Time.deltaTime;

            if (m_Time >= UPDATE_INTERVAL)
            {
                m_Time = 0f;

                Vector2 velocity = ((Vector2)transform.position - m_LastPosition) / Time.deltaTime;

                m_LastPosition = transform.position;
                WarpGrid.WarpingGrid.Instance.ApplyDirectedForce(velocity * VELOCITY_MULTI * forceMultiplier, transform.position, radius);
            }
        }
    }
}