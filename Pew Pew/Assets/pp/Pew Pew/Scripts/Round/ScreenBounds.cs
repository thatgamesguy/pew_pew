using UnityEngine;
using System.Collections;

namespace GameCore
{
    /// <summary>
    /// Holds data about the screen bounds.
    /// </summary>
    public class ScreenBounds : MonoBehaviour
    {
        /// <summary>
        /// The lower vertical bounds. Enemies below this will cause damage.
        /// </summary>
        public float lowerVerticalBounds = 0.2f;

        private static readonly float SCREEN_OFFSET = 0.1f;

        private Vector2 m_HorizontalViewportBounds;
        private Vector2 m_VerticalViewportBounds;
        private Vector2 m_HorizontalBounds;

        void Awake()
        {
            m_HorizontalViewportBounds = new Vector2(0f + SCREEN_OFFSET, 1f - SCREEN_OFFSET);
            m_VerticalViewportBounds = new Vector2(lowerVerticalBounds, 1f);


            m_HorizontalBounds = new Vector2(
                Camera.main.ViewportToWorldPoint(new Vector2(m_HorizontalViewportBounds.x, 0f)).x,
                Camera.main.ViewportToWorldPoint(new Vector2(m_HorizontalViewportBounds.y, 0f)).x
            );
        }

        /// <summary>
        /// Gets the horizontal bounds. X = left, Y = right.
        /// </summary>
        /// <returns>The horizontal bounds.</returns>
        public Vector2 GetHorizontalBounds()
        {
            return m_HorizontalBounds;
        }

        /// <summary>
        /// Gets the horizontal viewport bounds. X = left, Y = right.
        /// </summary>
        /// <returns>The horizontal viewport bounds.</returns>
        public Vector2 GetHorizontalViewportBounds()
        {
            return m_HorizontalViewportBounds;
        }

        /// <summary>
        /// Gets the vertical viewport bounds. X = bottom, Y = top.
        /// </summary>
        /// <returns>The vertical viewport bounds.</returns>
        public Vector2 GetVerticalViewportBounds()
        {
            return m_VerticalViewportBounds;
        }

        /// <summary>
        /// Determines whether the specified viewportPos is within the screen bounds.
        /// </summary>
        /// <returns><c>true</c> if the specified viewportPos is within bounds; otherwise, <c>false</c>.</returns>
        /// <param name="viewportPos">Viewport position.</param>
        public bool IsWithinBounds(Vector2 viewportPos)
        {
            return viewportPos.x >= m_HorizontalViewportBounds.x &&
                viewportPos.x <= m_HorizontalViewportBounds.y &&
                viewportPos.y >= m_VerticalViewportBounds.x &&
                viewportPos.y <= m_VerticalViewportBounds.y;
        }
    }
}