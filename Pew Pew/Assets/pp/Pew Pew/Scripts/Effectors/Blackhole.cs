using UnityEngine;
using System;
using System.Collections;
using PE2D;
using WarpGrid;

namespace GameCore
{
    /// <summary>
    /// Attached to a blackhole or repel. Controls health.
    /// </summary>
    public class Blackhole : MonoBehaviour, HitListener, HitDeathInvoker
    {
        /// <summary>
        /// The starting health.
        /// </summary>
        public int hitPoints = 1;

        /// <summary>
        /// The audio to play on death.
        /// </summary>
        public AudioClip audioOnDeath;

        /// <summary>
        /// The audio to play on damage.
        /// </summary>
        public AudioClip audioOnDamage;

        /// <summary>
        /// The colour of the particles that are released by this object (if spewParticles is true).
        /// </summary>
        public Color particleSpewColour;

        /// <summary>
        /// The percentage to scale down when hit.
        /// </summary>
        [Range(0f, 100f)]
        public float percentageScaleDownWhenHit = 20f;

        /// <summary>
        /// The number of particles to spawn on death.
        /// </summary>
        public int numOfParticlesOnDeath = 30;

        /// <summary>
        /// The number of particles to spawn on hit.
        /// </summary>
        public int numOfParticlesOnHit = 20;

        /// <summary>
        /// Gets or sets the on hit action. This is called when the object has been damaged.
        /// </summary>
        /// <value>The on hit action.</value>
        public Action onHit
        {
            get { return m_OnHit; }
            set { m_OnHit = value; }
        }

        /// <summary>
        /// Gets or sets the on death action. This is called when the object is killed.
        /// </summary>
        /// <value>The on death.</value>
        public Action onDeath
        {
            get { return m_OnDeath; }
            set { m_OnDeath = value; }
        }

        /// <summary>
        /// Sets whether this instance should release particles whilst it is alive.
        /// </summary>
        public bool spewParticles = true;

        private static readonly float TIME_TO_SPRAY = 0.25f;
        private static AudioPlayer m_Audio;
        private static CameraShake m_CameraShake;

        private Action m_OnDeath;
        private Action m_OnHit;
        private int m_CurrentHitPoints;
        private float sprayAngle = 0;
        private float m_CurrentSprayTime = 0f;
        private ParticleBuilder m_CachedState;

        void Awake()
        {
            if (m_Audio == null)
            {
                m_Audio = Camera.main.GetComponent<AudioPlayer>();
            }

            if (m_CameraShake == null)
            {
                m_CameraShake = Camera.main.GetComponent<CameraShake>();
            }

            CustomParticle.UpdateEffectorList();
        }

        void Start()
        {
            m_CachedState = new ParticleBuilder()
            {
                velocity = Vector2.zero,
                wrapAroundType = WrapAroundType.None,
                lengthMultiplier = 50f,
                velocityDampModifier = 0.94f,
                removeWhenAlphaReachesThreshold = true,
                canBeCollectedByPlayer = true,
                ignoreInitialEffectorTime = true,
                maxLengthClamp = 1.5f

            };

        }

        void OnEnable()
        {
            m_CurrentHitPoints = hitPoints;
        }

        void Update()
        {
            m_CurrentSprayTime += Time.deltaTime;

            if (spewParticles && m_CurrentSprayTime >= TIME_TO_SPRAY)
            {
                m_CurrentSprayTime = 0f;
                Vector2 sprayVel = StaticExtensions.Math.FromPolar(
                    sprayAngle, UnityEngine.Random.Range(.6f, 1.2f)) * .11f;
                Vector2 pos = (Vector2)transform.position + 0.05f
                    * new Vector2(sprayVel.y, -sprayVel.x) +
                    StaticExtensions.Random.RandomVector2(.05f, .2f);

                m_CachedState.velocity = sprayVel;
                var duration = UnityEngine.Random.Range(280f, 480f);
                var initialScale = new Vector2(1f, 1f);

                ParticleFactory.instance.CreateParticle(pos, particleSpewColour, duration, initialScale, m_CachedState);
            }

            sprayAngle -= (Mathf.PI * 2f) / 50f;

            WarpGrid.WarpingGrid.Instance.ApplyImplosiveForce((float)Mathf.Sin(sprayAngle / 2) * .15f * .4f, transform.position, 2.5f);
        }

        /// <summary>
        /// Applies explosive damage to object.
        /// </summary>
        /// <param name="damage">Damage to apply.</param>
        public void ExplosionInRange(int damage)
        {
            CheckDamage(damage, false);
        }

        /// <summary>
        /// Applies damage to object.
        /// </summary>
        /// <param name="damage">Damage to apply.</param>
        public void OnHit(int damage)
        {
            if (!GetComponent<SpriteRenderer>().isVisible)
            {
                return;
            }

            CheckDamage(damage, true);
        }

        /// <summary>
        /// Kills this instance. Plays audio, raises on death event, spawns explosion, and destroys GameObject.
        /// </summary>
        /// <param name="cameraShake">If set to <c>true</c> camera shake is applied.</param>
        public void Kill(bool cameraShake)
        {
            if (cameraShake)
            {
                m_CameraShake.Begin(0.1f, 0.1f);
            }

            PlayOnDeathAudio();

            if (m_OnDeath != null)
            {
                m_OnDeath();
            }

            SpawnExplosion(numOfParticlesOnDeath);
            DestroyQuietly();
        }

        /// <summary>
        /// Plays the on death audio.
        /// </summary>
        public void PlayOnDeathAudio()
        {
            m_Audio.PlayInstance(audioOnDeath);
        }

        private void CheckDamage(int damageAmount, bool cameraShake)
        {
            m_CurrentHitPoints -= damageAmount;

            if (m_CurrentHitPoints <= 0)
            {
                WarpGrid.WarpingGrid.Instance.ApplyExplosiveForce(8f, transform.position, 3f);
                Kill(cameraShake);
            }
            else
            {
                WarpGrid.WarpingGrid.Instance.ApplyExplosiveForce(4f, transform.position, 3f);

                if (audioOnDamage)
                {
                    m_Audio.PlayInstance(audioOnDamage);
                }

                if (m_OnHit != null)
                {
                    m_OnHit();
                }


                if (percentageScaleDownWhenHit > 0f)
                {
                    StartCoroutine(ScaleDown(transform.localScale.x *
                        ((100f - (percentageScaleDownWhenHit * damageAmount)) / 100f)));
                }

                SpawnExplosion(numOfParticlesOnHit);
            }
        }

        private void DestroyQuietly()
        {
            Destroy(gameObject);
        }

        private IEnumerator ScaleDown(float targetX)
        {
            while (transform.localScale.x > targetX)
            {
                transform.localScale = transform.localScale * 0.91f;
                yield return new WaitForSeconds(0.02f);
            }
        }

        private void SpawnExplosion(int numParticles)
        {
            float hue = (float)((3 * Time.realtimeSinceStartup) % 6);
            Color color = StaticExtensions.Color.HSVToColor(hue, 0.25f, 1);
            float startOffset = UnityEngine.Random.Range(0f, (Mathf.PI * 2f) / numParticles);

            for (int i = 0; i < numParticles; i++)
            {
                Vector2 sprayVel = (StaticExtensions.Math.FromPolar((Mathf.PI * 2f)
                    * i / numParticles + startOffset,
                    UnityEngine.Random.Range(8f, 16f))) * 0.008f;

                Vector2 pos = (Vector2)transform.position + 0.2f * sprayVel;

                m_CachedState.velocity = sprayVel;

                float duration = UnityEngine.Random.Range(280f, 480f);
                var initialScale = new Vector2(1f, 1f);

                ParticleFactory.instance.CreateParticle(pos, color, duration, initialScale, m_CachedState);
            }
        }
    }
}