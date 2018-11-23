using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GameCore
{
    /// <summary>
    /// Contract for perfoming Oscillation.
    /// </summary>
    public interface YMovementOscillation
    {
        /// <summary>
        /// Gets an oscillation vector.
        /// </summary>
        /// <returns>The oscillation vector.</returns>
        Vector3 GetOscillation();
    }

    /// <summary>
    /// Implementation of Y Oscillation.
    /// </summary>
    public class YMovementOscillationImpl : YMovementOscillation
    {
        private Transform m_Owner;
        private float m_Scale;
        private float m_YOffset;

        /// <summary>
        /// Initializes a new instance of the <see cref="YMovementOscillationImpl"/> class.
        /// </summary>
        /// <param name="owner">Owner to oscillate.</param>
        /// <param name="scale">Scale of oscillation.</param>
        /// <param name="yOffset">Y offset.</param>
        public YMovementOscillationImpl(Transform owner, float scale, float yOffset)
        {
            m_Owner = owner;
            m_Scale = scale;
            m_YOffset = yOffset;
        }

        /// <summary>
        /// Gets an oscillation vector.
        /// </summary>
        /// <returns>The oscillation vector.</returns>
        public Vector3 GetOscillation()
        {
            var newY = m_Scale * Mathf.Sin(2 * m_Owner.position.x) + m_YOffset;
            return new Vector3(m_Owner.position.x, m_Owner.position.y + newY);
        }
    }

    /// <summary>
    /// Y idle oscillation. No oscillation is performed.
    /// </summary>
    public class YIdleOscillation : YMovementOscillation
    {
        private Transform m_Owner;

        /// <summary>
        /// Initializes a new instance of the <see cref="YIdleOscillation"/> class.
        /// </summary>
        /// <param name="owner">Owner.</param>
        public YIdleOscillation(Transform owner)
        {
            m_Owner = owner;
        }

        /// <summary>
        /// Returns owners position.
        /// </summary>
        /// <returns>The oscillation vector.</returns>
        public Vector3 GetOscillation()
        {
            return m_Owner.position;
        }
    }

    /// <summary>
    /// Controls enemies that move vertically and wrap around the screen.
    /// </summary>
    public class VerticalWrapAroundMovement : MonoBehaviour, AdjustableMoveSpeed, EnemyMove
    {
        /// <summary>
        /// The movement direction.
        /// </summary>
        public MovementDirection movementDirection = MovementDirection.Left;

        /// <summary>
        /// The movement speed.
        /// </summary>
        public float moveSpeed = 10f;

        /// <summary>
        /// The amount to increase movement speed near round end.
        /// </summary>
        public float moveSpeedAdjustment = 2f;

        /// <summary>
        /// Sets whether this instance should move up and down on the y axis.
        /// </summary>
        public bool oscillateY = false;

        /// <summary>
        /// Sets whether this instance should be removed from the round when it has gone offscreen and is the last enemy.
        /// </summary>
        public bool removeWhenLastEnemy = false;

        private static Dictionary<MovementDirection, Vector2> MOVE_DIR_LOOKUP = new Dictionary<MovementDirection, Vector2>();

        private static GameManager GAME_MANAGER;
        private SpriteRenderer m_SpriteRenderer;
        private Collider2D m_Collider2D;
        private float m_StartTime;
        private bool m_ShouldUpdate = false;
        private Vector2 m_MoveDir;
        private YMovementOscillation m_MoveOscillation;
        private RoundEnemy m_RoundEnemy;

        void Awake()
        {
            m_SpriteRenderer = GetComponent<SpriteRenderer>();
            m_Collider2D = GetComponent<Collider2D>();

            if (MOVE_DIR_LOOKUP.Count == 0)
            {
                MOVE_DIR_LOOKUP.Add(MovementDirection.Left, Vector2.left);
                MOVE_DIR_LOOKUP.Add(MovementDirection.Right, Vector2.right);
                MOVE_DIR_LOOKUP.Add(MovementDirection.Up, Vector2.zero);
                MOVE_DIR_LOOKUP.Add(MovementDirection.Down, Vector2.zero);
            }

            if (GAME_MANAGER == null)
            {
                GAME_MANAGER = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
            }

            m_RoundEnemy = GetComponent<RoundEnemy>();
        }

        void Start()
        {
            m_Collider2D.enabled = false;
            m_SpriteRenderer.color = m_SpriteRenderer.color.WithAlpha(0f);

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
            m_MoveDir = MOVE_DIR_LOOKUP[movementDirection];
        }

        /// <summary>
        /// Begin this instance. Starts fade in.
        /// </summary>
        public void Begin()
        {
            m_StartTime = Time.time;
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

        /// <summary>
        /// Increments the speed near round end.
        /// </summary>
        public void IncrementSpeed()
        {
            moveSpeed += moveSpeedAdjustment;
        }

        void Update()
        {
            if (m_ShouldUpdate)
            {
                transform.position += (Vector3)m_MoveDir * moveSpeed * Time.deltaTime;

                transform.position = m_MoveOscillation.GetOscillation();

                if (movementDirection == MovementDirection.Left)
                {
                    if (transform.position.x < -3.2f)
                    {
                        if (GAME_MANAGER == null)
                        {
                            Destroy(gameObject);
                            return;
                        }

                        if (removeWhenLastEnemy && GAME_MANAGER.currentRound.enemiesRemaining == 1)
                        {
                            m_RoundEnemy.EscapedWave();
                            Destroy(gameObject);
                        }
                        else
                        {
                            transform.position = new Vector2(3.2f, transform.position.y);
                        }
                    }
                }
                else
                {
                    if (transform.position.x > 3.2f)
                    {
                        if (GAME_MANAGER == null)
                        {
                            Destroy(gameObject);
                            return;
                        }

                        if (removeWhenLastEnemy && GAME_MANAGER.currentRound.enemiesRemaining == 1)
                        {
                            m_RoundEnemy.EscapedWave();
                            Destroy(gameObject);
                        }
                        else
                        {
                            transform.position = new Vector2(-3.2f, transform.position.y);
                        }
                    }
                }


            }
        }

        private IEnumerator FadeIn()
        {
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