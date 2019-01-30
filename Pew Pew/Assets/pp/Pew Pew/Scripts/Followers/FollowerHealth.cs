using UnityEngine;
using System.Collections;
using PE2D;
using WarpGrid;

namespace GameCore
{
    /// <summary>
    /// Handles followers life, taking damage, spawning projectiles, and destroying.
    /// </summary>
    public class FollowerHealth : MonoBehaviour, HitListener
    {
        /// <summary>
        /// The maximum starting health.
        /// </summary>
        public int maxHealth = 2;

        /// <summary>
        /// The number of particles to spawn on death.
        /// </summary>
        public int numOfParticlesOnDeath = 20;

        /// <summary>
        /// The number of particles to spawn when damage is taken.
        /// </summary>
        public int numOfParticlesOnDamage = 10;

        /// <summary>
        /// The colour of spawned particles.
        /// </summary>
        public Color particleColour;

        [Range(0f, 100f)]
        /// <summary>
        /// The percentage to scale down when hit.
        /// </summary>
        public float percentageScaleDownWhenHit = 10f;

        private int m_CurrentHealth;
        private Vector3 m_InitialScale;
        private ParticleBuilder m_CachedState;

        void Awake()
        {
            m_InitialScale = transform.localScale;
        }

        void Start()
        {
            m_CachedState = new ParticleBuilder()
            {
                velocity = Vector2.zero,
                wrapAroundType = WrapAroundType.None,
                lengthMultiplier = 40f,
                velocityDampModifier = 0.94f,
                removeWhenAlphaReachesThreshold = true,
                canBeCollectedByPlayer = true,
                maxLengthClamp = 1.5f
            };
        }

        void OnEnable()
        {
            m_CurrentHealth = maxHealth;
            transform.localScale = m_InitialScale;
        }

        /// <summary>
        /// Raises the hit event. Applies damage to follower. Destroys follower if current health reaches zero.
        /// </summary>
        /// <param name="damage">Damage taken.</param>
        public void OnHit(int damage)
        {
            m_CurrentHealth -= damage;

            if (m_CurrentHealth <= 0)
            {
                OnDeath();
            }
            else
            {
                TakeDamage();
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Enemy") || other.CompareTag("Blackhole"))
            {
                OnHit(maxHealth);
            }
        }

        private void TakeDamage()
        {
            StartCoroutine(ScaleDown(transform.localScale.x * ((100f - percentageScaleDownWhenHit) / 100f)));
            SpawnExplosion(transform.position, numOfParticlesOnDamage);
        }

        private void OnDeath()
        {
            SpawnExplosion(transform.position, numOfParticlesOnDeath);
            gameObject.SetActive(false);
        }

        private void SpawnExplosion(Vector2 position, int numOfParticles)
        {
            WarpGrid.WarpingGrid.Instance.ApplyExplosiveForce(8f, transform.position, 1.8f);

            float hue1 = UnityEngine.Random.Range(0f, 6f);
            float hue2 = (hue1 + UnityEngine.Random.Range(0f, 2f)) % 6f;
            Color colour1 = StaticExtensions.Color.FromHSV(hue1, 0.5f, 1);
            Color colour2 = StaticExtensions.Color.FromHSV(hue2, 0.5f, 1);

            for (int i = 0; i < numOfParticles; i++)
            {
                float speed = (12f * (1f - 1 / Random.Range(2f, 3f))) * 0.02f;

                m_CachedState.velocity = StaticExtensions.Random.RandomVector2(speed, speed);

                var colour = Color.Lerp(colour1, colour2, UnityEngine.Random.Range(0f, 1f));

                float duration = Random.Range(200f, 320f);
                var initialScale = new Vector2(1f, 1f);


                ParticleFactory.instance.CreateParticle(position, colour, duration, initialScale, m_CachedState);
            }
        }

        private IEnumerator ScaleDown(float targetX)
        {
            while (transform.localScale.x > targetX)
            {
                transform.localScale = transform.localScale * 0.91f;
                yield return new WaitForSeconds(0.02f);
            }
        }
    }
}