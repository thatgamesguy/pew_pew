using UnityEngine;
using PE2D;
using System.Collections;
using WarpGrid;

namespace GameCore
{
    /// <summary>
    /// Enables a projectile to change heading based on players current location.
    /// </summary>
    public class HomingProjectile : Projectile
    {
        /// <summary>
        /// The speed at which the projectile can turn.
        /// </summary>
        public float turnSpeed = 20f;

        /// <summary>
        /// The delay before projectile starts homing towards players location.
        /// </summary>
        public float delayToTurn = 0.8f;

        /// <summary>
        /// The number of particles to spawn when projectile is removed from game.
        /// </summary>
        public int numOfParticlesToSpawnWhenTimeUp = 10;

        private static Transform PLAYER;

        private float m_CurrentExecutionTime = 0f;

        protected override void Awake()
        {
            base.Awake();

            if (PLAYER == null)
            {
                PLAYER = GameObject.FindGameObjectWithTag("Player").transform;
            }
        }

        protected override void Update()
        {
            if (m_Paused) { return; }

            m_CurrentExecutionTime += Time.deltaTime;

            if (m_CurrentExecutionTime >= delayToTurn)
            {
                Vector3 vectorToTarget = PLAYER.position - transform.position;
                float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg - 90;

                Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
                transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * turnSpeed);

            }

            m_Rigidbody2D.AddForce(transform.up * moveForce);

            m_CurrentTimeAlive += Time.deltaTime;

            if (m_CurrentTimeAlive >= timeAlive)
            {
                RemoveFromGame();
            }
        }

        private void RemoveFromGame()
        {
            SpawnExplosion();
            ReturnProjectile();
        }

        private void SpawnExplosion()
        {
            WarpGrid.WarpingGrid.Instance.ApplyExplosiveForce(5f, transform.position, 1.3f);

            float hue1 = UnityEngine.Random.Range(0f, 6f);
            float hue2 = (hue1 + UnityEngine.Random.Range(0f, 2f)) % 6f;
            Color colour1 = StaticExtensions.Color.FromHSV(hue1, 0.5f, 1);
            Color colour2 = StaticExtensions.Color.FromHSV(hue2, 0.5f, 1);


            for (int i = 0; i < numOfParticlesToSpawnWhenTimeUp; i++)
            {
                float speed = (12f * (1f - 1 / UnityEngine.Random.Range(2f, 3f))) * 0.005f;

                var state = new ParticleBuilder()
                {
                    velocity = StaticExtensions.Random.RandomVector2(speed, speed),
                    wrapAroundType = WrapAroundType.None,
                    lengthMultiplier = 30f,
                    velocityDampModifier = 0.94f,
                    removeWhenAlphaReachesThreshold = true,
                    canBeCollectedByPlayer = false,
                    ignoreEffectors = true,
                    maxLengthClamp = 1.5f
                };

                var colour = Color.Lerp(colour1, colour2, UnityEngine.Random.Range(0f, 1f));

                float duration = UnityEngine.Random.Range(280f, 480f);
                var initialScale = new Vector2(1f, 1f);


                ParticleFactory.instance.CreateParticle(transform.position,
                    colour, duration, initialScale, state);
            }
        }
    }
}