using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace GameCore
{
    /// <summary>
    /// Controls the directional boss part.
    /// </summary>
    public class BossPartDirectional : BossPartImpl
    {
        /// <summary>
        /// The movement speed.
        /// </summary>
        public float moveSpeed = 5f;

        /// <summary>
        /// The movement directions.
        /// </summary>
        public MovementDirection[] moveDirections;

        /// <summary>
        /// The distance to move each time.
        /// </summary>
        public float moveOffset = 2f;

        /// <summary>
        /// The minimum distance part must be to target before next direction is calculated.
        /// </summary>
        public float minDistToTarget = 0.1f;

        /// <summary>
        /// The seconds to pause when target.
        /// </summary>
        public float pauseOnTargetReach = 0.5f;

        /// <summary>
        /// The rotate speed.
        /// </summary>
        public float rotateSpeed = 320.0f;

        /// <summary>
        /// The number of projectiles to request on each shoot attempt.
        /// </summary>
        public int numOfProjectilesToRequest = 1;

        private static Dictionary<MovementDirection, Vector2> m_MoveDirLookup = new Dictionary<MovementDirection, Vector2>();

        private ShootRequestable m_Shoot;
        private float m_ZRot = 0f;
        private Vector2 m_CurrentTarget;
        private bool m_SeekingNewTarget = false;
        private bool m_ShouldUpdate = false;
        private int m_CurrentMoveIndex;
        private Vector3 m_CurAngle;
        private bool m_Rotating = false;

        private int m_ProjCount = 0;

        private MovementDirection m_CurrentDirection
        {
            get
            {
                return moveDirections[m_CurrentMoveIndex];
            }
        }

        protected override void Awake()
        {
            base.Awake();

            m_CurrentMoveIndex = 0;

            if (m_MoveDirLookup.Count == 0)
            {
                m_MoveDirLookup.Add(MovementDirection.Left, Vector2.left);
                m_MoveDirLookup.Add(MovementDirection.Down, Vector2.down);
                m_MoveDirLookup.Add(MovementDirection.Right, Vector2.right);
                m_MoveDirLookup.Add(MovementDirection.Up, Vector2.up);
            }

            m_Shoot = GetComponentInChildren<ShootRequestable>();
        }

        protected override void Start()
        {
            base.Start();

            SetCurrentTarget();

            m_CurAngle = transform.eulerAngles;
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
        }

        void Update()
        {
            if (m_ShouldUpdate)
            {
                if (Vector2.Distance(transform.position, m_CurrentTarget) < minDistToTarget)
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
                            bool shouldShoot = (m_ProjCount = (m_ProjCount + 1) % 2) == 0;

                            if (shouldShoot)
                            {
                                for (int i = 0; i < numOfProjectilesToRequest; i++)
                                {
                                    m_Shoot.RequestShoot();
                                }
                            }
                        }
                    }

                }

                if (!m_SeekingNewTarget)
                {
                    transform.position += (Vector3)m_MoveDirLookup[m_CurrentDirection] * moveSpeed * Time.deltaTime;
                }
            }
        }

        private void IncrementTarget()
        {
            IncrementMoveIndex();
            SetCurrentTarget();

            m_SeekingNewTarget = false;
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

        private IEnumerator RotateAngle(float angle)
        {
            if (m_Rotating)
                yield break; // ignore calls to RotateAngle while rotating
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
    }
}