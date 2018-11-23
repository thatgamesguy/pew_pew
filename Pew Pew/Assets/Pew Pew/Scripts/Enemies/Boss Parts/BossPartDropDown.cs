using UnityEngine;
using System.Collections;
using System;

namespace GameCore
{
    /// <summary>
    /// Controls the boss part drop down.
    /// </summary>
    public class BossPartDropDown : BossPartImpl
    {
        /// <summary>
        /// The horizontal movement speed.
        /// </summary>
        public float horMoveSpeed = 5f;

        /// <summary>
        /// The minimum and maximum time before the boss part drops down.
        /// A random number is selected between these two values.
        /// </summary>
        public Vector2 minMaxTimeToDropDown = new Vector2(2f, 5f);

        /// <summary>
        /// The initial drop movement speed.
        /// </summary>
        public float dropSpeed;

        /// <summary>
        /// The drop speed increment.
        /// </summary>
        public float dropSpeedUp = 15f;

        /// <summary>
        /// The speed the boss part moves up before dropping down.
        /// </summary>
        public float bounceUpSpeed;

        /// <summary>
        /// THe distance to move up before dropping down.
        /// </summary>
        public float bounceUpDistance = 0.6f;

        /// <summary>
        /// The second delay between projectile shoot requests.
        /// </summary>
        public float secDelayBetweenProjShoot = 1f;

        private bool m_ShouldUpdate = false;
        private Transform m_Player;
        private float m_CurrentDropDownTime = 0f;
        private float m_DropY;
        private bool m_WrappedAround = false;
        private bool m_DidBounce = false;
        private float m_CurrentDropSpeed;
        private float timeToDropDown;
        private ShootRequestable[] m_Shoots;
        private float m_CurrentShootSec;

        protected override void Awake()
        {
            base.Awake();

            m_Shoots = GetComponentsInChildren<ShootRequestable>();

            m_Player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        protected override void DoActivation()
        {
            timeToDropDown = UnityEngine.Random.Range(minMaxTimeToDropDown.x, minMaxTimeToDropDown.y);

            m_ShouldUpdate = true;
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

        void Update()
        {
            if (m_ShouldUpdate)
            {
                m_CurrentDropDownTime += Time.deltaTime;


                if (m_CurrentDropDownTime >= timeToDropDown)
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
                else
                {
                    var dir = m_Player.position - transform.position;
                    transform.position += new Vector3(dir.x, 0f) * horMoveSpeed * Time.deltaTime;

                    m_DropY = transform.position.y + 0.2f;
                    m_WrappedAround = false;
                    m_DidBounce = false;
                    m_CurrentDropSpeed = dropSpeed;

                    if (m_Shoots != null)
                    {
                        m_CurrentShootSec += Time.deltaTime;

                        if (m_CurrentShootSec >= secDelayBetweenProjShoot)
                        {

                            foreach (var shoot in m_Shoots)
                            {
                                shoot.RequestShoot();
                            }

                            m_CurrentShootSec = 0f;
                        }
                    }
                }
            }
        }

        private void BounceUp()
        {
            transform.position += Vector3.up * bounceUpSpeed * Time.deltaTime;
        }

        private void Drop()
        {
            m_CurrentDropSpeed += dropSpeedUp * Time.deltaTime;

            transform.position += Vector3.down * m_CurrentDropSpeed * Time.deltaTime;

            if (transform.position.y <= -5.2f)
            {
                transform.position = new Vector2(transform.position.x, 5.2f);
                m_WrappedAround = true;
            }

            if (m_WrappedAround && transform.position.y <= m_DropY)
            {
                m_CurrentDropDownTime = 0f;
                timeToDropDown = UnityEngine.Random.Range(minMaxTimeToDropDown.x, minMaxTimeToDropDown.y);
            }
        }

    }
}