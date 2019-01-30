using UnityEngine;
using System.Collections;

namespace GameCore
{
    /// <summary>
    /// Controls enemies that bouce around the screen.
    /// </summary>
    public class ScreenBoundsBounceMovement : MonoBehaviour, AdjustableMoveSpeed, EnemyMove
    {
        /// <summary>
        /// The layermask for the screen bounds.
        /// </summary>
        public LayerMask hitMask;

        /// <summary>
        /// The movement speed.
        /// </summary>
        public float moveSpeed = 2f;

        /// <summary>
        /// The amount to increment the movement speed near round end.
        /// </summary>
        public float moveSpeedIncrement = 1;

        /// <summary>
        /// Sets whether this instance should continue moving while player is respawning.
        /// </summary>
        public bool continueMovementOnPlayerDeath = false;

        private static readonly float MIN_INITIAL_VECTOR_VALUE = 0.6f;

        private float m_StartTime;
        private bool m_ShouldUpdate = false;
        private Collider2D m_Collider2D;
        private SpriteRenderer m_SpriteRenderer;
        private Vector2 m_CurrentDirection;
        private EnemyHealth m_Health;

        void Awake()
        {
            m_SpriteRenderer = GetComponent<SpriteRenderer>();
            m_Collider2D = GetComponent<Collider2D>();
            m_Health = GetComponent<EnemyHealth>();
        }

        void Start()
        {
            m_Collider2D.enabled = false;
            m_SpriteRenderer.color = m_SpriteRenderer.color.WithAlpha(0f);

            float[] moveVector = new float[2];

            for (int i = 0; i < moveVector.Length; i++)
            {
                moveVector[i] = Random.Range(MIN_INITIAL_VECTOR_VALUE, 1f);

                if (Random.value >= 0.5f)
                {
                    moveVector[i] *= -1f;
                }
            }

            m_CurrentDirection = new Vector2(moveVector[0], moveVector[1]);

            if (GetComponent<EnemyMoveRegister>() == null)
            {
                gameObject.AddComponent<EnemyMoveRegister>();
            }
        }

        void OnEnable()
        {
            m_ShouldUpdate = false;
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
        /// Pause this instance if not continueMovementOnPlayerDeath.
        /// </summary>
        public void Pause()
        {
            if (!continueMovementOnPlayerDeath)
            {
                m_ShouldUpdate = false;
            }
        }

        /// <summary>
        /// Resume this instance.
        /// </summary>
        public void Resume()
        {
            m_ShouldUpdate = true;
        }

        public void IncrementSpeed()
        {
            moveSpeed += moveSpeedIncrement;
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
            RaycastHit2D hit = Physics2D.Raycast(transform.position, m_CurrentDirection, 2f, hitMask);

            if (hit.collider != null)
            {
                Vector3 incomingVec = (Vector3)hit.point - transform.position;
                return Vector3.Reflect(incomingVec, hit.normal).normalized;

            }

            return Vector3.zero;
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