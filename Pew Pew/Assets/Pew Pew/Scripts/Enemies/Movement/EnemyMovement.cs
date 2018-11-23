using UnityEngine;
using System;
using System.Collections;

namespace GameCore
{
    /// <summary>
    /// Possible enemy movement directions.
    /// </summary>
    public enum MovementDirection
    {
        Up,
        Left,
        Down,
        Right
    }

    /// <summary>
    /// Contract for all enemies that can begin, pause, and resume actions.
    /// </summary>
    public interface EnemyMove
    {
        void Begin();
        void Pause();
        void Resume();
    }

    /// <summary>
    /// Controls standard enemy movement.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
    public class EnemyMovement : MonoBehaviour, AdjustableMoveSpeed, EnemyMove
    {
        /// <summary>
        /// The movement speed.
        /// </summary>
        public float moveSpeed = 0.05f;

        /// <summary>
        /// The amount to increase movement speed when near round end.
        /// </summary>
        public float moveSpeedAdjustment = 0.01f;

        /// <summary>
        /// The initial movement direction.
        /// </summary>
        public MovementDirection initialMoveDir = MovementDirection.Left;

        private static float TARGET_FRAME = 0.01f;

        private float m_Accumulation = 0f;
        private MovementState[] m_MoveStates;
        private MovementState m_CurrentState { get { return m_MoveStates[m_CurrentIndex]; } }
        private int m_CurrentIndex = 0;
        private SpriteRenderer m_SpriteRenderer;
        private float m_StartTime;
        private bool m_ShouldUpdate = false;
        private Collider2D m_Collider2D;

        void Awake()
        {
            m_SpriteRenderer = GetComponent<SpriteRenderer>();
            m_Collider2D = GetComponent<Collider2D>();
        }

        void OnEnable()
        {
            m_ShouldUpdate = false;
        }

        void Start()
        {
            if (GetComponent<EnemyMoveRegister>() == null)
            {
                gameObject.AddComponent<EnemyMoveRegister>();
            }

            m_Collider2D.enabled = false;
            m_SpriteRenderer.color = m_SpriteRenderer.color.WithAlpha(0f);

            var bounds = GameObject.FindGameObjectWithTag("Bounds").GetComponent<ScreenBounds>();

            m_MoveStates = new MovementState[4];

            if (initialMoveDir == MovementDirection.Left)
            {
                m_MoveStates[0] = new MoveLeft(bounds);
                m_MoveStates[2] = new MoveRight(bounds);
            }
            else
            {
                m_MoveStates[0] = new MoveRight(bounds);
                m_MoveStates[2] = new MoveLeft(bounds);
            }

            m_MoveStates[1] = new MoveDown(new Vector2(0f, -0.58f));
            m_MoveStates[3] = new MoveDown(new Vector2(0f, -0.58f));


            m_CurrentIndex = 0;

            m_CurrentState.Enter(transform);

        }

        /// <summary>
        /// Begin this instance.
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

        void FixedUpdate()
        {
            if (!m_ShouldUpdate)
            {
                return;
            }

            m_Accumulation += Time.deltaTime;

            if (m_Accumulation >= TARGET_FRAME)
            {
                transform.position += (Vector3)m_CurrentState.NextMove() * moveSpeed;

                if (m_CurrentState.CompletedMove(transform))
                {
                    m_CurrentIndex = (m_CurrentIndex + 1) % m_MoveStates.Length;
                    m_CurrentState.Enter(transform);
                }

                m_Accumulation -= TARGET_FRAME;
            }
        }
    }
}