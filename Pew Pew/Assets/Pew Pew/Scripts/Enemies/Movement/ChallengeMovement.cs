using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace GameCore
{
    /// <summary>
    /// Controls the challenge enemies movement.
    /// </summary>
    public class ChallengeMovement : MonoBehaviour, EnemyMove
    {
        /// <summary>
        /// The movement direction.
        /// </summary>
        public MovementDirection moveDirection = MovementDirection.Left;

        /// <summary>
        /// The movement speed.
        /// </summary>
        public float moveSpeed = 3f;

        /// <summary>
        /// Bounce the charact on the y axis as it moves across the screen.
        /// </summary>
        public bool oscillateY = false;

        /// <summary>
        /// Invoked when enemy escapes wave.
        /// </summary>
        public Action onEscapedWave;

        /// <summary>
        /// The delay before the enemy starts moving.
        /// </summary>
        public float startDelay = 0f;

        private static readonly Dictionary<MovementDirection, Vector2> MOVE_DIR_LOOKUP = new Dictionary<MovementDirection, Vector2>();

        private SpriteRenderer m_SpriteRenderer;
        private float m_StartTime;
        private bool m_ShouldUpdate = false;
        private Collider2D m_Collider2D;
        private bool m_BecameVisible;
        private YMovementOscillation m_MoveOscillation;

        void Awake()
        {
            m_SpriteRenderer = GetComponent<SpriteRenderer>();
            m_Collider2D = GetComponent<Collider2D>();
        }

        void Start()
        {
            m_Collider2D.enabled = false;
            m_SpriteRenderer.color = m_SpriteRenderer.color.WithAlpha(0f);

            if (MOVE_DIR_LOOKUP.Count == 0)
            {
                MOVE_DIR_LOOKUP.Add(MovementDirection.Left, Vector2.left);
                MOVE_DIR_LOOKUP.Add(MovementDirection.Down, Vector2.down);
                MOVE_DIR_LOOKUP.Add(MovementDirection.Right, Vector2.right);
                MOVE_DIR_LOOKUP.Add(MovementDirection.Up, Vector2.up);
            }

            if (oscillateY)
            {
                m_MoveOscillation = new YMovementOscillationImpl(transform, 0.01f, 0f);
            }
            else
            {
                m_MoveOscillation = new YIdleOscillation(transform);
            }

            if (GetComponent<EnemyMoveRegister>() == null)
            {
                gameObject.AddComponent<EnemyMoveRegister>();
            }
        }

        void OnEnable()
        {
            m_ShouldUpdate = false;
            m_BecameVisible = m_SpriteRenderer.isVisible;
        }

        /// <summary>
        /// Begin this instance. Starts fade in.
        /// </summary>
        public void Begin()
        {
            StartCoroutine(FadeIn());
        }

        /// <summary>
        /// Pause this instance.
        /// </summary>
        public void Pause()
        {
            m_ShouldUpdate = false;
        }

        /// <summary>
        /// Resume this instance.
        /// </summary>
        public void Resume()
        {
            m_ShouldUpdate = true;
        }

        void Update()
        {
            if (m_ShouldUpdate)
            {
                transform.position += (Vector3)MOVE_DIR_LOOKUP[moveDirection] * moveSpeed * Time.deltaTime;
                transform.position = m_MoveOscillation.GetOscillation();

                if (!m_BecameVisible)
                {
                    m_BecameVisible = m_SpriteRenderer.isVisible;
                }

                if (LeftScreenBounds())
                {
                    if (onEscapedWave != null)
                    {
                        onEscapedWave();
                    }

                    Destroy(gameObject);
                }
            }
        }

        private bool LeftScreenBounds()
        {
            return m_BecameVisible && !m_SpriteRenderer.isVisible;
        }

        private IEnumerator FadeIn()
        {
            if (startDelay > 0f)
            {
                yield return new WaitForSeconds(startDelay);
            }

            m_StartTime = Time.time;

            float t = 0f;

            while (t < 1f)
            {
                t = (Time.time - m_StartTime) / GameManager.ROUND_BEGIN_TIME;
                m_SpriteRenderer.color = m_SpriteRenderer.color.WithAlpha(Mathf.SmoothStep(0f, 1f, t));
                yield return new WaitForEndOfFrame();
            }

            m_ShouldUpdate = true;
            m_Collider2D.enabled = true;
        }
    }
}