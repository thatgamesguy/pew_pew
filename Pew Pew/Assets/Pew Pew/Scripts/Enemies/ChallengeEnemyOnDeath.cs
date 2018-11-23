using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PE2D;

namespace GameCore
{
    /// <summary>
    /// Handles animation and particle emission on challenge enemies death.
    /// </summary>
    public class ChallengeEnemyOnDeath : MonoBehaviour
    {
        /// <summary>
        /// The particle colours to spew on death.
        /// </summary>
        public Color particleColour;

        private ParticleBuilder m_CachedParticle;
        private ChallengeMovement m_Movement;
        private EnemyHealth m_EnemyHealth;

        void Awake()
        {
            m_EnemyHealth = GetComponent<EnemyHealth>();
            m_Movement = GetComponent<ChallengeMovement>();
        }

        void Start()
        {
            m_CachedParticle = new ParticleBuilder()
            {
                velocity = Vector2.zero,
                wrapAroundType = WrapAroundType.None,
                lengthMultiplier = 50f,
                velocityDampModifier = 0.94f,
                removeWhenAlphaReachesThreshold = true,
                canBeCollectedByPlayer = false,
                ignoreEffectors = true,
                maxLengthClamp = 1.5f
            };
        }

        void OnEnable()
        {
            m_EnemyHealth.onDestroyHook += OnDeath;
        }

        void OnDisable()
        {
            m_EnemyHealth.onDestroyHook -= OnDeath;
        }

        private void OnDeath()
        {
            m_Movement.Pause();

            StartCoroutine(ScaleDown());
        }

        private IEnumerator ScaleDown()
        {
            while (transform.localScale.x > 0.1f)
            {
                transform.localScale -= Vector3.one * 12f * Time.deltaTime;
                yield return null;
            }

            SpawnExplosion(transform.position, 10);
            Destroy(gameObject);
        }

        private void SpawnExplosion(Vector2 position, int numOfParticles)
        {
            for (int i = 0; i < numOfParticles; i++)
            {
                float speed = (UnityEngine.Random.Range(10f, 14f) * (1f - 1 / UnityEngine.Random.Range(1f, 10f))) * 0.007f;

                Vector2 sprayVel = StaticExtensions.Random.RandomVector2(speed, speed);
                Vector2 pos = (Vector2)transform.position + 1.2f * sprayVel;

                m_CachedParticle.velocity = sprayVel;

                float duration = UnityEngine.Random.Range(280f, 480f);
                var initialScale = new Vector2(1f, 1f);

                ParticleFactory.instance.CreateParticle(pos, particleColour, duration, initialScale, m_CachedParticle);
            }
        }
    }
}