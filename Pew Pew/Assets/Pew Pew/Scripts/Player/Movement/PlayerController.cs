using UnityEngine;
using System.Collections;

namespace GameCore
{
    /// <summary>
    /// Updates player position based on input.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        /// <summary>
        /// The movment speed when not playing on mobile.
        /// </summary>
        public float desktopMovementSpeed = 10f;

        /// <summary>
        /// The movement speed when playing on mobile.
        /// </summary>
        public float mobileMovementSpeed = 10f;

        private Rigidbody2D m_Rigidbody2D;
        private Vector2 m_Velocity;
        private PlayerInput m_PlayerInput;
        private float m_InitialSpeed;
        private ScreenBounds m_Bounds;
        private bool m_SpeedBoost = false;
        private PlayerHealth m_Health;
        private bool m_MovementPaused = false;

        void Awake()
        {
            m_Rigidbody2D = GetComponent<Rigidbody2D>();
            m_Bounds = GameObject.FindGameObjectWithTag("Bounds").GetComponent<ScreenBounds>();
            m_Health = GetComponent<PlayerHealth>();
        }

        void Start()
        {
            if (Application.isMobilePlatform)
            {
                m_PlayerInput = new TouchInput(transform, mobileMovementSpeed);
            }
            else
            {
                m_PlayerInput = new KeyboardInput(desktopMovementSpeed);
            }
        }

        void OnEnable()
        {
            if (m_SpeedBoost)
            {
                ResetSpeed();
            }

            m_Health.OnDeath += PauseMovement;
            m_Health.OnSpawn += ResumeMovement;
        }

        void OnDisable()
        {
            m_Health.OnDeath -= PauseMovement;
            m_Health.OnSpawn -= ResumeMovement;
        }

        void Update()
        {
            m_Velocity = m_PlayerInput.GetVelocity();

            ClampPosition();
        }

        void FixedUpdate()
        {
            if (!m_MovementPaused)
            {
                m_Rigidbody2D.MovePosition(m_Rigidbody2D.position + m_Velocity * m_PlayerInput.GetMovementSpeed() * Time.deltaTime);
            }
        }

        /// <summary>
        /// Pauses the movement.
        /// </summary>
        public void PauseMovement()
        {
            m_MovementPaused = true;
        }

        /// <summary>
        /// Resumes the movement.
        /// </summary>
        public void ResumeMovement()
        {
            m_MovementPaused = false;
        }

        /// <summary>
        /// Increments the players speed. Called when Ship Speed is purchased in store.
        /// </summary>
        /// <param name="increment">Increment.</param>
        public void IncrementSpeed(float increment = 1f)
        {
            if (m_SpeedBoost)
            {
                m_InitialSpeed += increment;
            }
            else
            {
                m_PlayerInput.SetMovementSpeed(m_PlayerInput.GetMovementSpeed() + increment);
            }
        }

        /// <summary>
        /// Increments the players speed for seconds.
        /// </summary>
        /// <param name="increment">Amount to increase speed.</param>
        /// <param name="time">Time in seconds players speed is increased.</param>
        public void IncrementSpeedForSeconds(float increment, float time)
        {
            if (!m_SpeedBoost)
            {
                m_InitialSpeed = m_PlayerInput.GetMovementSpeed();

                m_PlayerInput.SetMovementSpeed(m_PlayerInput.GetMovementSpeed() + increment);

                m_SpeedBoost = true;

                Invoke("ResetSpeed", time);
            }
        }

        private void ResetSpeed()
        {
            m_SpeedBoost = false;

            m_PlayerInput.SetMovementSpeed(m_InitialSpeed);
        }

        private void ClampPosition()
        {
            var viewportPos = Camera.main.WorldToViewportPoint(transform.position);

            var horizontalBounds = m_Bounds.GetHorizontalViewportBounds();
            viewportPos.x = Mathf.Clamp(viewportPos.x, horizontalBounds.x, horizontalBounds.y);
            transform.position = Camera.main.ViewportToWorldPoint(viewportPos);
        }
    }
}