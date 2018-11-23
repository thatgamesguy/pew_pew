using UnityEngine;
using System.Collections;
using System;

namespace GameCore
{
    /// <summary>
    /// Controls the boss part seperate ship.
    /// </summary>
    public class BossPartSeperateShip : BossPartImpl
    {
        /// <summary>
        /// The movement speed.
        /// </summary>
        public float moveSpeed = 10f;

        /// <summary>
        /// The hit mask for the screen bounds.
        /// </summary>
        public LayerMask hitMask;

        private static readonly float MIN_INITIAL_VECTOR_VALUE = 0.6f;

        private bool m_ShouldUpdate;
        private Vector2 m_CurrentDirection;
        private Rotate m_Rotate;
        private EnemyHealth m_Health;

        protected override void Awake()
        {
            base.Awake();

            m_Health = GetComponent<EnemyHealth>();
            m_Rotate = GetComponent<Rotate>();
        }


        protected override void Start()
        {
            base.Start();

            float[] moveVector = new float[2];

            for (int i = 0; i < moveVector.Length; i++)
            {
                moveVector[i] = UnityEngine.Random.Range(MIN_INITIAL_VECTOR_VALUE, 1f);

                if (UnityEngine.Random.value >= 0.5f)
                {
                    moveVector[i] *= -1f;
                }
            }

            m_CurrentDirection = new Vector2(moveVector[0], moveVector[1]);
        }

        /// <summary>
        /// Pause this instance.
        /// </summary>
        public override void Pause()
        {
            m_ShouldUpdate = false;
        }

        /// <summary>
        /// Resume this instance.
        /// </summary>
        public override void Resume()
        {
            if (m_HasBeenActivated)
            {
                m_ShouldUpdate = true;
            }
        }

        protected override void DoActivation()
        {
            m_ShouldUpdate = true;

            if (m_Rotate)
            {
                m_Rotate.Activate();
            }
        }

        void Update()
        {
            if (m_ShouldUpdate)
            {
                transform.position += (Vector3)m_CurrentDirection * moveSpeed * Time.deltaTime;
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("BoundsCollider"))
            {
                var reflect = GetReflection();
                if (reflect != Vector3.zero)
                {
                    m_CurrentDirection = reflect;
                }
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("BoundsCollider"))
            {
                var viewportPos = Camera.main.WorldToViewportPoint(transform.position);

                if (viewportPos.x < 0f || viewportPos.x > 1f || viewportPos.y < 0f
                    || viewportPos.y > 1f)
                {
                    m_Health.Kill(false);
                }
            }
        }

        private Vector3 GetReflection()
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position,
                m_CurrentDirection, 20f, hitMask);

            if (hit.collider != null)
            {
                Vector3 incomingVec = (Vector3)hit.point - transform.position;
                return Vector3.Reflect(incomingVec, hit.normal).normalized;

            }

            return Vector3.zero;
        }
    }
}