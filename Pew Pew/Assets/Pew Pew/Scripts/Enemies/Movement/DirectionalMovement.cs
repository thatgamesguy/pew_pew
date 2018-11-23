using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GameCore
{
    /// <summary>
    /// Controls the directional enemies.
    /// </summary>
    public class DirectionalMovement : MonoBehaviour, AdjustableMoveSpeed, EnemyMove
    {
        /// <summary>
        /// The movement speed.
        /// </summary>
        public float moveSpeed = 5f;

        /// <summary>
        /// The amount to increment the movement speed on round end.
        /// </summary>
        public float moveSpeedIncrement = 2f;

        /// <summary>
        /// The move directions. The enemy is moved in these directions in turn.
        /// </summary>
        public MovementDirection[] moveDirections;

        /// <summary>
        /// The movement offset. The enemy is moved by this amount each time.
        /// </summary>
        public float moveOffset = 2f;

        /// <summary>
        /// The time in seconds the enemy pauses when it reaches its destination before moving in the next direction.
        /// </summary>
        public float pauseOnTargetReach = 0.5f;

        /// <summary>
        /// The number of projectiles to request when shooting.
        /// </summary>
        public int numOfProjectilesToRequest = 1;

        /// <summary>
        /// The rotatation speed.
        /// </summary>
        public float rotateSpeed = 120.0f;

        /// <summary>
        /// The seconds delay before an enemy starts moving.
        /// </summary>
        public float delayedStart = 0f;

        private static readonly int MAX_OFFSCREEN = 3;
        private static float m_MinDistToTarget = 0.2f;
        private static Dictionary<MovementDirection, Vector2> m_MoveDirLookup = new Dictionary<MovementDirection, Vector2>();

        private bool m_SeekingNewTarget = false;
        private int m_CurrentMoveIndex;
        private MovementDirection m_CurrentDirection
        {
            get
            {
                return moveDirections[m_CurrentMoveIndex];
            }
        }
        private SpriteRenderer m_SpriteRenderer;
        private float m_StartTime;
        private bool m_ShouldUpdate = false;
        private Collider2D m_Collider2D;
        private Vector2 m_CurrentTarget;
        private float m_ZRot = 0f;
        private ShootRequestable m_Shoot;
        private Vector3 m_CurAngle;
        private bool m_Rotating = false;
        private int m_OffscreenCount = 0;
        private Vector2 m_StartPos;

        void Awake()
        {
            m_SpriteRenderer = GetComponent<SpriteRenderer>();
            m_Collider2D = GetComponent<Collider2D>();
            m_Shoot = GetComponentInChildren<ShootRequestable>();
        }

        void Start()
        {
            m_StartPos = transform.position;

            m_Collider2D.enabled = false;
            m_SpriteRenderer.color = m_SpriteRenderer.color.WithAlpha(0f);

            m_CurrentMoveIndex = 0;

            if (m_MoveDirLookup.Count == 0)
            {
                m_MoveDirLookup.Add(MovementDirection.Left, Vector2.left);
                m_MoveDirLookup.Add(MovementDirection.Down, Vector2.down);
                m_MoveDirLookup.Add(MovementDirection.Right, Vector2.right);
                m_MoveDirLookup.Add(MovementDirection.Up, Vector2.up);
            }

            SetCurrentTarget();

            m_CurAngle = transform.eulerAngles;

            if (GetComponent<EnemyMoveRegister>() == null)
            {
                gameObject.AddComponent<EnemyMoveRegister>();
            }
        }

        void OnEnable()
        {
            m_SeekingNewTarget = false;
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
            moveSpeed += moveSpeedIncrement;
        }

        void Update()
        {
            if (!m_ShouldUpdate)
            {
                return;
            }

            if (Vector2.Distance(transform.position, m_CurrentTarget) < m_MinDistToTarget)
            {
                if (!m_SeekingNewTarget)
                {
                    m_ZRot += 90f;

                    if (m_ZRot > 359f)
                    {
                        m_ZRot = 0f;
                    }

                    transform.eulerAngles =
                        new Vector3(transform.eulerAngles.x,
                                    transform.eulerAngles.y, m_ZRot);

                    Invoke("IncrementTarget", pauseOnTargetReach);
                    m_SeekingNewTarget = true;
                    StartCoroutine(RotateAngle(90));

                    if (m_Shoot != null)
                    {
                        for (int i = 0; i < numOfProjectilesToRequest; i++)
                        {
                            m_Shoot.RequestShoot();
                        }
                    }
                }

            }

            if (!m_SeekingNewTarget)
            {
                transform.position += (Vector3)m_MoveDirLookup[m_CurrentDirection] * moveSpeed * Time.deltaTime;
            }

        }

        private void ApplyMoveCorrection()
        {
            var viewPos = Camera.main.WorldToViewportPoint(transform.position);

            if (viewPos.x < -0.05f || viewPos.x > 1.05f || viewPos.y < -0.05f || viewPos.y > 1.05f)
            {
                m_OffscreenCount++;
            }
            else
            {
                m_OffscreenCount = 0;
            }

            if (m_OffscreenCount > MAX_OFFSCREEN)
            {
                transform.position = m_StartPos;
                m_OffscreenCount = 0;
            }
        }

        private IEnumerator RotateAngle(float angle)
        {
            if (m_Rotating) yield break; // ignore calls to RotateAngle while rotating
            m_Rotating = true;  // set the flag
            var newAngle = m_CurAngle.z + angle; // calculate the new angle

            while (m_CurAngle.z < newAngle)
            {
                // move a little step at constant speed to the new angle:
                m_CurAngle.z = Mathf.MoveTowards(m_CurAngle.z, newAngle, rotateSpeed * Time.deltaTime);
                transform.eulerAngles = m_CurAngle; // update the object's rotation...
                yield return new WaitForEndOfFrame(); // and let Unity free till the next frame
            }

            m_Rotating = false;
        }

        private void IncrementTarget()
        {
            IncrementMoveIndex();
            SetCurrentTarget();

            m_SeekingNewTarget = false;

            ApplyMoveCorrection();
        }

        private void IncrementMoveIndex()
        {
            m_CurrentMoveIndex = (m_CurrentMoveIndex + 1) % moveDirections.Length;
        }

        private void SetCurrentTarget()
        {
            m_CurrentTarget = GetCurrentTarget();
        }

        private Vector2 GetCurrentTarget()
        {
            return (Vector2)transform.position + m_MoveDirLookup[m_CurrentDirection] * moveOffset;
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


            if (delayedStart > 0f)
            {
                Invoke("StartActions", delayedStart);
            }
            else
            {
                StartActions();
            }

            m_Collider2D.enabled = true;
        }

        private void StartActions()
        {
            m_ShouldUpdate = true;
        }
    }
}