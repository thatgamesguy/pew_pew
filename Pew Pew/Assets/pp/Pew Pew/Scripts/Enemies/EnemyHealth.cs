using UnityEngine;
using System;
using System.Collections;
using WarpGrid;
using PE2D;

namespace GameCore
{
    /// <summary>
    /// Contract for any entity that can take damage or react to damage.
    /// </summary>
    public interface HitListener
    {
        /// <summary>
        /// Raises the hit event.
        /// </summary>
        /// <param name="damage">Damage taken.</param>
        void OnHit(int damage);
    }

    /// <summary>
    /// Contract for any entity that can take damage from a bomb.
    /// </summary>
    public interface BombListener
    {
        /// <summary>
        /// Gets the owner.
        /// </summary>
        /// <value>The owner.</value>
        Transform owner { get; }

        /// <summary>
        /// Apply damage from explosion.
        /// </summary>
        /// <param name="damage">Damage to apply.</param>
        void ExplosionInRange(int damage);
    }

    /// <summary>
    /// Provides contract to providing a hook for an entities onDeath and onHit events.
    /// </summary>
    public interface HitDeathInvoker
    {
        /// <summary>
        /// Gets or sets an action to perform on death.
        /// </summary>
        /// <value>The on death action.</value>
        Action onDeath { get; set; }

        /// <summary>
        /// Gets or sets an action to perform on hit.
        /// </summary>
        /// <value>The on hit action.</value>
        Action onHit { get; set; }
    }

    /// <summary>
    /// Controls enemies health and taking damage.
    /// </summary>
    public class EnemyHealth : MonoBehaviour, HitListener, BombListener, HitDeathInvoker
    {
        /// <summary>
        /// The number of hit points for the enemy.
        /// </summary>
        public int hitPoints = 1;

        /// <summary>
        /// An AudioClip to play when the enemy is destroyed.
        /// </summary>
        public AudioClip audioOnDeath;

        /// <summary>
        /// An AudioClip to play when the enemy takes damage.
        /// </summary>
        public AudioClip audioOnDamage;

        [Range(0f, 100f)]
        /// <summary>
        /// The percentage to scale down when hit.
        /// </summary>
        public float percentageScaleDownWhenHit = 20f;

        /// <summary>
        /// Sets whether this instance should be destroyed when below this Y value.
        /// </summary>
        public float destroyWhenBelowY = -40f;

        /// <summary>
        /// The magnitude of the camera shake to apply on death.
        /// </summary>
        public float camShakeMag = 0.1f;

        /// <summary>
        /// The time in seconds of the camera shake on death.
        /// </summary>
        public float camShakeSec = 0.1f;

        /// <summary>
        /// The colour of the particles to spew on death.
        /// </summary>
        public Color particleColour;

        /// <summary>
        /// A multiplier to apply to the Grid when entity is destroyed.
        /// </summary>
        public float explosiveForceMulti = 1f;

        /// <summary>
        /// If not null, replaces the destroy method when enemy killed.
        /// </summary>
        public Action onDestroyHook;

        /// <summary>
        /// Gets or sets an action to perform on hit.
        /// </summary>
        /// <value>The on hit action.</value>
        public Action onHit
        {
            get { return m_OnHit; }
            set { m_OnHit = value; }
        }

        /// <summary>
        /// Gets or sets an action to perform on death.
        /// </summary>
        /// <value>The on death action.</value>
        public Action onDeath
        {
            get { return m_OnDeath; }
            set { m_OnDeath = value; }
        }

        /// <summary>
        /// Gets the owner.
        /// </summary>
        /// <value>The owner.</value>
        public Transform owner { get { return transform; } }

        /// <summary>
        /// The number of particles to spew on death.
        /// </summary>
        public int numOfParticlesOnDeath = 20;

        /// <summary>
        /// The number of particles to spew when damaged.
        /// </summary>
        public int numOfParticlesOnHit = 10;

        private static readonly float COLOUR_OFFSET = 0.2f;

        private static AudioPlayer m_Audio;
        private static CameraShake m_CameraShake;
        private Action m_OnDeath;
        private Action m_OnHit;
        private int m_CurrentHitPoints;
        private ParticleBuilder m_CachedState;
        private SpriteRenderer m_Renderer;
        private Color m_ToColour;

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

            m_Renderer = GetComponent<SpriteRenderer>();
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
                maxLengthClamp = 1.5f
            };

            m_ToColour = new Color(
                Mathf.Clamp01(particleColour.r + UnityEngine.Random.Range(-COLOUR_OFFSET, COLOUR_OFFSET)),
                Mathf.Clamp01(particleColour.g + UnityEngine.Random.Range(-COLOUR_OFFSET, COLOUR_OFFSET)),
                Mathf.Clamp01(particleColour.b + UnityEngine.Random.Range(-COLOUR_OFFSET, COLOUR_OFFSET)),
                1f
            );
        }

        void OnEnable()
        {
            m_CurrentHitPoints = hitPoints;
        }

        /// <summary>
        /// Apply damage from explosion.
        /// </summary>
        /// <param name="damage">Damage to apply.</param>
        public void ExplosionInRange(int damage)
        {
            CheckDamage(damage, false);
        }

        /// <summary>
        /// Raises the hit event. Damages entity.
        /// </summary>
        /// <param name="damage">Damage taken.</param>
        public void OnHit(int damage)
        {
            if (!GetComponent<SpriteRenderer>().isVisible)
            {
                return;
            }

            CheckDamage(damage, true);
        }

        /// <summary>
        /// Kill the entity.
        /// </summary>
        /// <param name="cameraShake">If set to <c>true</c> camera shake is applied.</param>
        public void Kill(bool cameraShake)
        {
            if (cameraShake)
            {
                m_CameraShake.Begin(camShakeSec, camShakeMag);
            }

            PlayOnDeathAudio();

            if (m_OnDeath != null)
            {
                m_OnDeath();
            }

            if (numOfParticlesOnDeath > 0)
            {
                SpawnExplosion(transform.position, numOfParticlesOnDeath);
            }

            if (onDestroyHook != null)
            {
                onDestroyHook();
            }
            else
            {
                DestroyQuietly();
            }
        }

        /// <summary>
        /// Plays the on death audio.
        /// </summary>
        public void PlayOnDeathAudio()
        {
            m_Audio.PlayInstance(audioOnDeath);
        }

        void Update()
        {
            var pos = Camera.main.WorldToScreenPoint(transform.position);

            if (pos.y < destroyWhenBelowY)
            {
                DestroyQuietly();
            }
        }

        private void CheckDamage(int damageAmount, bool cameraShake)
        {
            m_CurrentHitPoints -= damageAmount;

            if (m_CurrentHitPoints <= 0)
            {
                WarpGrid.WarpingGrid.Instance.ApplyExplosiveForce(8f * explosiveForceMulti, transform.position, 1.8f);
                Kill(cameraShake);
            }
            else
            {
                WarpGrid.WarpingGrid.Instance.ApplyExplosiveForce(8f * explosiveForceMulti * 0.5f, transform.position, 1.8f);
                if (audioOnDamage)
                {
                    m_Audio.PlayInstance(audioOnDamage);
                }

                if (m_OnHit != null)
                {
                    m_OnHit();
                }

                m_Renderer.DoDamageFlash();

                if (percentageScaleDownWhenHit > 0f)
                {
                    StartCoroutine(ScaleDown(transform.localScale.x *
                    ((100f - (percentageScaleDownWhenHit * damageAmount)) / 100f)));
                }

                if (numOfParticlesOnHit > 0)
                {
                    SpawnExplosion(transform.position, numOfParticlesOnHit);
                }
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

        private void SpawnExplosion(Vector2 position, int numOfParticles)
        {

            for (int i = 0; i < numOfParticles; i++)
            {
                //float speed = (12f * (1f - 1 / UnityEngine.Random.Range (2f, 3f))) * 0.01f;
                float speed = (UnityEngine.Random.Range(10f, 14f) * (1f - 1 / UnityEngine.Random.Range(1f, 10f))) * 0.007f;

                Vector2 sprayVel = StaticExtensions.Random.RandomVector2(speed, speed);
                Vector2 pos = (Vector2)transform.position + 1.2f * sprayVel;

                m_CachedState.velocity = sprayVel;

                var colour = Color.Lerp(particleColour, m_ToColour, UnityEngine.Random.Range(0f, 1f));

                float duration = UnityEngine.Random.Range(280f, 480f);
                var initialScale = new Vector2(1f, 1f);


                ParticleFactory.instance.CreateParticle(pos, colour, duration, initialScale, m_CachedState);
            }
        }

    }
}