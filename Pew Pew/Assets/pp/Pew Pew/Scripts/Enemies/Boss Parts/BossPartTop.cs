using UnityEngine;
using System.Collections;
using System;

namespace GameCore
{
    /// <summary>
    /// Controls the boss top part.
    /// </summary>
    public class BossPartTop : BossPartImpl
    {
        /// <summary>
        /// The movement speed.
        /// </summary>
        public float moveSpeed = 5f;

        /// <summary>
        /// The layer mask for the screen bounds.
        /// </summary>
        public LayerMask hitMask;

        /// <summary>
        /// The rotation speed.
        /// </summary>
        public float rotateSpeed = 5f;

        private static readonly float MIN_INITIAL_VECTOR_VALUE = 0.6f;

        private EnemyShoot[] m_EnemyShoots;
        private bool m_ShouldUpdate = false;
        private Vector2 m_CurrentDirection;

        protected override void Awake()
        {
            base.Awake();

            m_EnemyShoots = GetComponentsInChildren<EnemyShoot>();
        }

        protected override void Start()
        {
            foreach (var shoot in m_EnemyShoots)
            {
                shoot.StopActivation();
            }

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

            base.Start();
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
            StartCoroutine(RotateDegrees(360f));

            foreach (var shoot in m_EnemyShoots)
            {
                shoot.Begin();
            }

            m_ShouldUpdate = true;
        }

        void Update()
        {
            if (m_ShouldUpdate)
            {
                transform.position += (Vector3)m_CurrentDirection
                    * moveSpeed * Time.deltaTime;
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

        private IEnumerator RotateDegrees(float degrees)
        {
            while (true)
            {
                var angle = (Mathf.Sin(Time.time * rotateSpeed) + 1f) / 2f * degrees;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

                yield return new WaitForEndOfFrame();
            }
        }

        private Vector3 GetReflection()
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position,
                m_CurrentDirection, 2f, hitMask);

            if (hit.collider != null)
            {
                Vector3 incomingVec = (Vector3)hit.point - transform.position;
                return Vector3.Reflect(incomingVec, hit.normal).normalized;
            }

            return Vector3.zero;
        }
    }
}