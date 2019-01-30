using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace GameCore
{
    /// <summary>
    /// Controls enemies quick movement.
    /// </summary>
    public class EnemyQuickMovement : MonoBehaviour, EnemyMove, AdjustableMoveSpeed
    {
        /// <summary>
        /// The movement speed.
        /// </summary>
        public float moveSpeed = 10f;

        /// <summary>
        /// The amount to increase movement speed near round end.
        /// </summary>
        public float moveSpeedAdjustment = 2f;

        /// <summary>
        /// The movement direction.
        /// </summary>
        public MovementDirection movementDirection = MovementDirection.Left;

        /// <summary>
        /// The x position on screen that enemy will rotate.
        /// </summary>
        public float xTurnAroundPosition = 0f;

        /// <summary>
        /// The rotation speed.
        /// </summary>
        public float rotateSpeed = 120.0f;

        private static Dictionary<MovementDirection, Vector2> MOVE_DIR_LOOKUP = new Dictionary<MovementDirection, Vector2>();

        private SpriteRenderer m_SpriteRenderer;
        private Collider2D m_Collider2D;
        private float m_StartTime;
        private bool m_ShouldUpdate = false;
        private Vector2 m_MoveDir;
        private bool m_TurningAround = false;
        private Vector3 m_CurAngle;
        private bool m_Rotating = false;
        private bool m_WrapAroundRequired = false;
        private ShootRequestable m_Shoot;

        void Awake()
        {
            m_SpriteRenderer = GetComponent<SpriteRenderer>();
            m_Collider2D = GetComponent<Collider2D>();

            if (MOVE_DIR_LOOKUP.Count == 0)
            {
                MOVE_DIR_LOOKUP.Add(MovementDirection.Left, Vector2.left);
                MOVE_DIR_LOOKUP.Add(MovementDirection.Right, Vector2.right);
            }

            m_Shoot = GetComponentInChildren<ShootRequestable>();
        }

        void Start()
        {
            m_Collider2D.enabled = false;
            m_SpriteRenderer.color = m_SpriteRenderer.color.WithAlpha(0f);

            m_CurAngle = transform.eulerAngles;

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

                if (!m_TurningAround)
                {
                    transform.position += (Vector3)m_MoveDir * moveSpeed * Time.deltaTime;

                    ApplyWrapAround();

                    if (!m_WrapAroundRequired)
                    {
                        m_TurningAround = ShouldTurnAround();

                        if (m_TurningAround)
                        {
                            float rotAngle =
                                movementDirection == MovementDirection.Left ?
                                90f : -90f;

                            StartCoroutine(RotateAngle(rotAngle, ReturnRotation));
                        }
                    }
                }
            }
        }

        private void ReturnRotation()
        {
            StartCoroutine(_ReturnRotation());
        }

        private IEnumerator _ReturnRotation()
        {
            m_WrapAroundRequired = true;
            if (m_Shoot != null)
            {
                m_Shoot.RequestShoot();
            }

            yield return new WaitForSeconds(0.3f);

            float rotAngle =
                movementDirection == MovementDirection.Left ?
                -90f : 90f;

            StartCoroutine(RotateAngle(rotAngle, FlagNotTurning));
        }

        private void FlagNotTurning()
        {
            m_TurningAround = false;
        }

        private void ApplyWrapAround()
        {

            if (movementDirection == MovementDirection.Left)
            {
                if (transform.position.x < -3.2f)
                {
                    transform.position = new Vector2(3.2f, transform.position.y);
                    m_WrapAroundRequired = false;
                }
            }
            else
            {
                if (transform.position.x > 3.2f)
                {
                    transform.position = new Vector2(-3.2f, transform.position.y);
                    m_WrapAroundRequired = false;
                }
            }
        }

        private bool ShouldTurnAround()
        {
            if (movementDirection == MovementDirection.Left)
            {
                if (transform.position.x <= xTurnAroundPosition)
                {
                    return true;
                }
            }
            else
            {
                if (transform.position.x >= xTurnAroundPosition)
                {
                    return true;
                }
            }

            return false;
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

        private IEnumerator RotateAngle(float angle, Action callBack)
        {
            if (m_Rotating)
            {
                yield break;
            }

            m_Rotating = true;

            var newAngle = m_CurAngle.z + angle;

            if (angle > 0)
            {


                while (m_CurAngle.z < newAngle)
                {
                    m_CurAngle.z = Mathf.MoveTowards(m_CurAngle.z, newAngle, rotateSpeed * Time.deltaTime);
                    transform.eulerAngles = m_CurAngle;
                    yield return new WaitForEndOfFrame();
                }
            }
            else
            {
                while (m_CurAngle.z > newAngle)
                {
                    m_CurAngle.z = Mathf.MoveTowards(m_CurAngle.z, newAngle, rotateSpeed * Time.deltaTime);
                    transform.eulerAngles = m_CurAngle;
                    yield return new WaitForEndOfFrame();
                }
            }

            m_Rotating = false;

            if (callBack != null)
            {
                callBack();
            }
        }
    }
}