using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GameCore
{
    /// <summary>
    /// Controls groups of vertically moving enemies.
    /// </summary>
    public class VerticalGroupMovement : MonoBehaviour, AdjustableMoveSpeed, EnemyMove
    {
        /// <summary>
        /// The movement speed.
        /// </summary>
        public float moveSpeed = 0.04f;

        /// <summary>
        /// The amount to increase the move speed near round end.
        /// </summary>
        public float moveSpeedAdjustment = 0.01f;

        /// <summary>
        /// The movement direction.
        /// </summary>
        public MovementDirection moveDirection = MovementDirection.Right;

        /// <summary>
        /// Sets whether this instance should move down when it reaches scren edge.
        /// </summary>
        public bool moveDown = true;

        private static float TARGET_FRAME = 0.01f;

        private List<EnemyMoveReceiver> m_EnemyMovement;
        private float m_Accumulation = 0f;
        private MovementState[] m_MoveStates;
        private MovementState m_CurrentState { get { return m_MoveStates[m_CurrentIndex]; } }
        private int m_CurrentIndex = 0;
        private EnemyMoveReceiver m_AliveEnemy;
        private List<SpriteFadeIn> m_FadeIns = new List<SpriteFadeIn>();
        private bool m_ShouldUpdate = false;
        private bool m_Paused = false;

        void Awake()
        {
            m_EnemyMovement = new List<EnemyMoveReceiver>(GetComponentsInChildren<EnemyMoveReceiver>());
        }

        void Start()
        {
            if (GetComponent<EnemyMoveRegister>() == null)
            {
                gameObject.AddComponent<EnemyMoveRegister>();
            }

            if (m_EnemyMovement.Count > 0)
            {
                var bounds = GameObject.FindGameObjectWithTag("Bounds").GetComponent<ScreenBounds>();

                m_MoveStates = new MovementState[4];

                if (moveDirection == MovementDirection.Right)
                {
                    m_MoveStates[0] = new MoveRight(bounds);
                    m_MoveStates[2] = new MoveLeft(bounds);

                }
                else if (moveDirection == MovementDirection.Left)
                {
                    m_MoveStates[0] = new MoveLeft(bounds);
                    m_MoveStates[2] = new MoveRight(bounds);
                }
                else
                {
                    Debug.LogWarning("No move state created for direction: " + moveDirection);
                    return;
                }

                m_MoveStates[1] = new MoveDown(new Vector2(0f, moveDown ? -0.54f : 0f));
                m_MoveStates[3] = new MoveDown(new Vector2(0f, moveDown ? -0.54f : 0f));
            }

            foreach (var enemy in m_EnemyMovement)
            {
                m_FadeIns.Add(enemy.gameObject.AddComponent<SpriteFadeIn>());
            }


        }

        /// <summary>
        /// Begin this instance. Starts fade in for all child objects.
        /// </summary>
        public void Begin()
        {
            foreach (var fade in m_FadeIns)
            {
                fade.StartFadeIn();
            }
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

        /// <summary>
        /// Increments the speed near round end.
        /// </summary>
        public void IncrementSpeed()
        {
            moveSpeed += moveSpeedAdjustment;
        }

        void FixedUpdate()
        {
            if (!m_ShouldUpdate)
            {
                CalculateReadyToUpdate();
                return;
            }

            if (m_Paused)
            {
                return;
            }

            var firstEnemy = GetFirstAliveEnemy();

            if (firstEnemy == null)
            {
                Destroy(gameObject);
                return;
            }

            m_Accumulation += Time.deltaTime;

            if (m_Accumulation >= TARGET_FRAME)
            {

                foreach (var enemy in m_EnemyMovement)
                {
                    if (enemy != null && enemy.gameObject != null)
                    {
                        enemy.DoMove(m_CurrentState.NextMove() * moveSpeed);
                    }
                }

                if (m_CurrentState.CompletedMove(firstEnemy.transform))
                {
                    m_CurrentIndex = (m_CurrentIndex + 1) % m_MoveStates.Length;
                    m_CurrentState.Enter(firstEnemy.transform);
                }

                m_Accumulation -= TARGET_FRAME;
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

        private EnemyMoveReceiver GetFirstAliveEnemy()
        {
            if (m_AliveEnemy != null && m_AliveEnemy.gameObject != null)
            {
                return m_AliveEnemy;
            }

            for (int i = 0; i < m_EnemyMovement.Count; i++)
            {
                var enemy = m_EnemyMovement[i];
                if (enemy != null && enemy.gameObject != null)
                {
                    //   selectedEnemy = i;
                    m_AliveEnemy = enemy;
                    break;
                }

            }

            return m_AliveEnemy;
        }
    }
}