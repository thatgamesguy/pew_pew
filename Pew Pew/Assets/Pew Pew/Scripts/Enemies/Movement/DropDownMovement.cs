using UnityEngine;
using System.Collections;

namespace GameCore
{
    /// <summary>
    /// Controls the drop down enemymovement.
    /// </summary>
    public class DropDownMovement : MonoBehaviour, EnemyMove, AdjustableMoveSpeed
    {
        [Header("Twitch")]
        /// <summary>
        /// The speed the enemy twitches while waiting to drop down.
        /// </summary>
        public float twitchSpeed;

        /// <summary>
        /// The amount to increment twitch speed when near round end.
        /// </summary>
        public float twitchSpeedInc;

        /// <summary>
        /// The twitch range. The enemy moves with a radius of this size when twitching.
        /// </summary>
        public float twitchRange = 1.1f;

        [Header("Drop")]
        /// <summary>
        /// The minimum and maximum seconds between dropping down.
        /// </summary>
        public Vector2 minMaxSecsBetweenDrop;

        /// <summary>
        /// The initial speed at which the enemy drops down.
        /// </summary>
        public float dropSpeed;

        /// <summary>
        /// The amount to increment the movement speed on the enemy.
        /// </summary>
        public float dropSpeedInc;

        /// <summary>
        /// The speed at which the enemy drops down.
        /// </summary>
        public float dropSpeedUp = 15f;

        [Header("Bounce")]
        /// <summary>
        /// The speed the enemy bounces up.
        /// </summary>
        public float bounceUpSpeed;

        /// <summary>
        /// The amount to increase the enemies bounce up speed.
        /// </summary>
        public float bounceUpSpeedInc;

        /// <summary>
        /// The distance to move up before falling down.
        /// </summary>
        public float bounceUpDistance = 0.6f;

        private static readonly float TOP = 5.2F;
        private static readonly float BOTTOM = -5.2F;

        private float m_CurrentDropSec;
        private float m_DropY;
        private bool m_WrappedAround = false;
        private bool m_DidBounce = false;
        private float m_CurrentDropSpeed;
        private SpriteRenderer m_SpriteRenderer;
        private Collider2D m_Collider2D;
        private float m_StartTime;
        private bool m_ShouldUpdate = false;

        void Awake()
        {
            m_SpriteRenderer = GetComponent<SpriteRenderer>();
            m_Collider2D = GetComponent<Collider2D>();
        }

        void Start()
        {
            m_Collider2D.enabled = false;
            m_SpriteRenderer.color = m_SpriteRenderer.color.WithAlpha(0f);
            m_CurrentDropSec = GetNextDropTime();

            if (GetComponent<EnemyMoveRegister>() == null)
            {
                gameObject.AddComponent<EnemyMoveRegister>();
            }
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
        /// Increments the drop, twitch, and bounce up speed.
        /// </summary>
        public void IncrementSpeed()
        {
            dropSpeed += dropSpeedInc;
            twitchSpeed += twitchSpeedInc;
            bounceUpSpeed += bounceUpSpeedInc;
        }

        void Update()
        {
            if (!m_ShouldUpdate)
            {
                return;
            }

            bool posUpdated = false;

            if (transform.position.y < -10f)
            {
                transform.position = new Vector2(transform.position.x, TOP);
                posUpdated = true;
            }

            var viewportPos = Camera.main.WorldToViewportPoint(transform.position);

            if (viewportPos.x < -0.05f || viewportPos.x > 1.05f)
            {
                transform.position = new Vector2(0f, TOP);
                posUpdated = true;
            }

            if (posUpdated)
            {
                m_CurrentDropSec = -1f;
                m_DidBounce = true;
                m_WrappedAround = true;
            }

            m_CurrentDropSec -= Time.deltaTime;

            if (m_CurrentDropSec > 0f)
            {
                Twitch();
                m_DropY = transform.position.y + 0.2f;
                m_WrappedAround = false;
                m_DidBounce = false;
                m_CurrentDropSpeed = dropSpeed;
            }
            else
            {
                if (!m_DidBounce && (transform.position.y < (m_DropY + bounceUpDistance)))
                {
                    BounceUp();
                }
                else
                {
                    m_DidBounce = true;
                    Drop();
                }
            }

        }

        private void Twitch()
        {
            var x = Random.Range(-twitchRange, twitchRange);
            var y = Random.Range(-twitchRange, twitchRange);
            var direction = new Vector3(x, y, 0f);
            //if you need the vector to have a specific length:
            direction = direction.normalized * twitchSpeed * Time.deltaTime;

            transform.position += direction;
        }

        private void BounceUp()
        {
            transform.position += Vector3.up * bounceUpSpeed * Time.deltaTime;
        }

        private void Drop()
        {
            m_CurrentDropSpeed += dropSpeedUp * Time.deltaTime;

            transform.position += Vector3.down * m_CurrentDropSpeed * Time.deltaTime;

            if (transform.position.y <= BOTTOM && !m_WrappedAround)
            {
                transform.position = new Vector2(transform.position.x, TOP);
                m_WrappedAround = true;
            }

            if (m_WrappedAround && transform.position.y <= m_DropY)
            {
                m_CurrentDropSec = GetNextDropTime();
            }
        }

        private float GetNextDropTime()
        {
            return Random.Range(minMaxSecsBetweenDrop.x, minMaxSecsBetweenDrop.y);
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