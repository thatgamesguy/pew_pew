using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace GameCore
{
    /// <summary>
    /// Controls the boss part quick.
    /// </summary>
    public class BossPartQuick : BossPartImpl
    {
        /// <summary>
        /// The movement direction.
        /// </summary>
        public MovementDirection movementDirection = MovementDirection.Left;

        /// <summary>
        /// The movement speed.
        /// </summary>
        public float moveSpeed = 10f;

        /// <summary>
        /// The X position on screen where the part should pause to shoot.
        /// </summary>
        public float xShootPosition = 0f;

        /// <summary>
        /// The delay before moving after shooting.
        /// </summary>
        public float delayBeforeMoving = 0.5f;

        private static Dictionary<MovementDirection, Vector2> MOVE_DIR_LOOKUP = new Dictionary<MovementDirection, Vector2>();

        private bool m_ShouldUpdate = false;
        private Vector2 m_MoveDir;
        private ShootRequestable[] m_Shoots;
        private bool m_Shooting = false;
        private bool m_WrapAroundRequired = false;

        protected override void Awake()
        {
            base.Awake();

            m_Shoots = GetComponentsInChildren<ShootRequestable>();

            if (MOVE_DIR_LOOKUP.Count == 0)
            {
                MOVE_DIR_LOOKUP.Add(MovementDirection.Left, Vector2.left);
                MOVE_DIR_LOOKUP.Add(MovementDirection.Right, Vector2.right);
            }

        }

        protected override void Start()
        {
            base.Start();

            m_MoveDir = MOVE_DIR_LOOKUP[movementDirection];
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
            if (m_ShouldUpdate && !m_Shooting)
            {
                transform.position += (Vector3)m_MoveDir * moveSpeed * Time.deltaTime;

                ApplyWrapAround();

                if (!m_WrapAroundRequired)
                {
                    m_Shooting = ShouldShoot();

                    if (m_Shooting)
                    {
                        StartCoroutine(Shoot());
                    }
                }
            }
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

        private bool ShouldShoot()
        {
            if (movementDirection == MovementDirection.Left)
            {
                if (transform.position.x <= xShootPosition)
                {
                    return true;
                }
            }
            else
            {
                if (transform.position.x >= xShootPosition)
                {
                    return true;
                }
            }

            return false;
        }

        private IEnumerator Shoot()
        {
            m_WrapAroundRequired = true;

            if (m_Shoots != null)
            {
                foreach (var shoot in m_Shoots)
                {
                    shoot.RequestShoot();
                }
            }

            yield return new WaitForSeconds(delayBeforeMoving);

            m_Shooting = false;
        }
    }
}