using UnityEngine;
using System.Collections;

namespace GameCore
{
    /// <summary>
    /// Contract for getting players next move.
    /// </summary>
    public interface PlayerInput
    {
        /// <summary>
        /// Gets the players movement speed.
        /// </summary>
        /// <returns>The movement speed.</returns>
        float GetMovementSpeed();

        /// <summary>
        /// Sets the players movement speed.
        /// </summary>
        /// <param name="amount">Move speed.</param>
        void SetMovementSpeed(float amount);

        /// <summary>
        /// Gets the velocity. The players next move.
        /// </summary>
        /// <returns>The velocity.</returns>
        Vector2 GetVelocity();
    }

    /// <summary>
    /// Provides method to control player based on keyboard input.
    /// </summary>
    public class KeyboardInput : PlayerInput
    {
        private float m_MovementSpeed;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardInput"/> class.
        /// </summary>
        /// <param name="moveSpeed">Move speed.</param>
        public KeyboardInput(float moveSpeed)
        {
            m_MovementSpeed = moveSpeed;
        }

        /// <summary>
        /// Gets the velocity. The players next move.
        /// </summary>
        /// <returns>The velocity.</returns>
        public Vector2 GetVelocity()
        {
            return new Vector2(Input.GetAxisRaw("Horizontal"), 0f);
        }

        /// <summary>
        /// Gets the players movement speed.
        /// </summary>
        /// <returns>The movement speed.</returns>
        public float GetMovementSpeed()
        {
            return m_MovementSpeed;
        }

        /// <summary>
        /// Sets the players movement speed.
        /// </summary>
        /// <param name="amount">Move speed.</param>
        public void SetMovementSpeed(float amount)
        {
            m_MovementSpeed = amount;
        }
    }

    /// <summary>
    /// Provides a method to control the player based on mobile touch input.
    /// </summary>
    public class TouchInput : PlayerInput
    {
        private Transform m_Player;
        private float m_MovementSpeed;

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchInput"/> class.
        /// </summary>
        /// <param name="player">Player.</param>
        /// <param name="moveSpeed">Move speed.</param>
        public TouchInput(Transform player, float moveSpeed)
        {
            m_Player = player;
            m_MovementSpeed = moveSpeed;
        }

        /// <summary>
        /// Gets the velocity. The players next move.
        /// </summary>
        /// <returns>The velocity.</returns>
        public Vector2 GetVelocity()
        {
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Stationary ||
                             touch.phase == TouchPhase.Moved)
                {

                    var position = Camera.main.ScreenToWorldPoint(touch.position);

                    var heading = position - m_Player.position;

                    var distance = heading.magnitude;
                    var direction = heading / distance;
                    direction.y = 0f;
                    return direction;
                }
            }

            return Vector2.zero;
        }

        /// <summary>
        /// Gets the players movement speed.
        /// </summary>
        /// <returns>The movement speed.</returns>
        public float GetMovementSpeed()
        {
            return m_MovementSpeed;
        }

        /// <summary>
        /// Sets the players movement speed.
        /// </summary>
        /// <param name="amount">Move speed.</param>
        public void SetMovementSpeed(float amount)
        {
            m_MovementSpeed = amount;
        }
    }
}