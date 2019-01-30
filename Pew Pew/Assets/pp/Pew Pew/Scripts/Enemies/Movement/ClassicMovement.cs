using UnityEngine;
using System.Collections.Generic;

namespace GameCore
{
    /// <summary>
    /// Controls the classic enemy movement.
    /// </summary>
    public class ClassicMovement : MonoBehaviour, EnemyMove, AdjustableMoveSpeed
    {
        /// <summary>
        /// The movement speed.
        /// </summary>
        public float moveSpeed = 10f;

        /// <summary>
        /// The amount the movement speed is incremented when reaching end of round.
        /// </summary>
        public float moveSpeedInc = 2f;

        /// <summary>
        /// The initial move direction.
        /// </summary>
        public MovementDirection initialMoveDirection;

        /// <summary>
        /// The distance to drop by each time the enemy row reaches the screen edge.
        /// </summary>
        public float yDrop = 0.1f;

        private int m_MoveIndex;
        private Vector2[] m_MoveDirections;
        private ScreenBounds m_Bounds;
        private bool m_ShouldUpdate = false;
        private List<SpriteFadeIn> m_FadeIns = new List<SpriteFadeIn>();
        private Transform m_RightBounds;
        private Transform m_LeftBounds;
        private EnemyHealth[] m_Enemies;

        private bool m_Paused = false;

        void Awake()
        {
            m_Bounds = GameObject.FindGameObjectWithTag("Bounds").GetComponent<ScreenBounds>();
        }

        void Start()
        {
            m_MoveDirections = new Vector2[] { Vector2.left, Vector2.right };

            m_MoveIndex = (initialMoveDirection == MovementDirection.Left) ? 0 : 1;

            m_Enemies = GetComponentsInChildren<EnemyHealth>();

            foreach (var enemy in m_Enemies)
            {
                m_FadeIns.Add(enemy.gameObject.AddComponent<SpriteFadeIn>());
            }

            if (GetComponent<EnemyMoveRegister>() == null)
            {
                gameObject.AddComponent<EnemyMoveRegister>();
            }
        }

        /// <summary>
        /// Begin this instance.
        /// </summary>
        public void Begin()
        {
            foreach (var fade in m_FadeIns)
            {
                fade.StartFadeIn();
            }
        }

        /// <summary>
        /// Increments the speed and yDrop amount.
        /// </summary>
        public void IncrementSpeed()
        {
            moveSpeed += moveSpeedInc;

            yDrop *= 1.3f;
        }

        /// <summary>
        /// Pause this instance.
        /// </summary>
        public void Pause()
        {
            m_Paused = true;
        }

        /// <summary>
        /// Resume this instance.
        /// </summary>
        public void Resume()
        {
            m_Paused = false;
        }

        void Update()
        {
            if (!m_ShouldUpdate)
            {
                CalculateReadyToUpdate();
            }
            else if (!m_Paused)
            {
                if (m_LeftBounds == null)
                {
                    CalculateLeftBounds();
                }

                if (m_RightBounds == null)
                {
                    CalculateRightBounds();
                }

                if (m_LeftBounds == null || m_RightBounds == null)
                {
                    //print("No bounds found");
                    return;
                }

                if (ShouldChangeDirection())
                {
                    ChangeDirection();

                    transform.position = new Vector2(transform.position.x, transform.position.y - yDrop);
                }

                transform.position += (Vector3)m_MoveDirections[m_MoveIndex] * moveSpeed * Time.deltaTime;

            }
        }

        private void CalculateReadyToUpdate()
        {
            foreach (var fadeIn in m_FadeIns)
            {
                if (!fadeIn.finished)
                {
                    return;
                }
            }

            m_ShouldUpdate = true;
        }

        private bool ShouldChangeDirection()
        {
            var curDir = CurrentDirection();

            if (curDir == MovementDirection.Left)
            {
                return m_LeftBounds.position.x <= m_Bounds.GetHorizontalBounds().x;
            }
            else
            {
                return m_RightBounds.position.x >= m_Bounds.GetHorizontalBounds().y;
            }

        }

        private void ChangeDirection()
        {
            m_MoveIndex = (m_MoveIndex + 1) % m_MoveDirections.Length;
        }

        private MovementDirection CurrentDirection()
        {
            return (m_MoveIndex == 0) ? MovementDirection.Left : MovementDirection.Right;
        }

        private void CalculateLeftBounds()
        {
            float leftX = float.MaxValue;
            Transform leftBounds = null;

            foreach (var enemy in m_Enemies)
            {
                if (enemy == null)
                {
                    continue;
                }

                if (enemy.transform.position.x < leftX)
                {
                    leftX = enemy.transform.position.x;
                    leftBounds = enemy.transform;
                }
            }

            if (leftX < float.MaxValue)
            {
                m_LeftBounds = leftBounds;
            }
        }

        private void CalculateRightBounds()
        {
            float rightX = -float.MaxValue;
            Transform rightBounds = null;

            foreach (var enemy in m_Enemies)
            {
                if (enemy == null)
                {
                    continue;
                }

                if (enemy.transform.position.x > rightX)
                {
                    rightX = enemy.transform.position.x;
                    rightBounds = enemy.transform;
                }
            }

            if (rightX > -float.MaxValue)
            {
                m_RightBounds = rightBounds;
            }
        }
    }
}