using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PE2D;
using WarpGrid;

namespace GameCore
{
    /// <summary>
    /// Responsible for animating the bomb and damaging enemies on explosion.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class Bomb : MonoBehaviour
    {
        /// <summary>
        /// The audio to play on explosion.
        /// </summary>
        public AudioClip audioToPlayOnExplode;

        /// <summary>
        /// The audio to play on countdown to explosion.
        /// </summary>
        public AudioClip audioToPlayOnFlash;

        /// <summary>
        /// The damage to apply to enemies in proximity.
        /// </summary>
        public int damage = 1;

        /// <summary>
        /// The radius. Enemies within this radius have damage applied to them.
        /// </summary>
        public float radius = 2.5f;

        /// <summary>
        /// The seconds from bomb placement to explosion.
        /// </summary>
        public float secsToExplode = 5f;

        /// <summary>
        /// The colors to loop over while counting down to explosion.
        /// </summary>
        public Color[] colors;

        private static readonly float FLASH_OFFSET = 10f;
        private static readonly int NUM_OF_PARTICLES_ON_DEATH = 20;
        private static AudioPlayer m_Audio;
        private static CameraShake m_Shake;

        private int m_PreviousColour = -1;
        private SpriteRenderer m_Renderer;
        private float m_CurrentColour;
        private float m_CurrentTime;
        private bool m_ShouldUpdate;

        void Awake()
        {
            if (m_Audio == null)
            {
                m_Audio = Camera.main.GetComponent<AudioPlayer>();
            }

            if (m_Shake == null)
            {
                m_Shake = Camera.main.GetComponent<CameraShake>();
            }

            m_Renderer = GetComponent<SpriteRenderer>();
        }

        void Start()
        {
            m_CurrentColour = Random.Range(0, colors.Length - 1);

            m_CurrentTime = 0f;
            m_ShouldUpdate = true;
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

        void Update()
        {
            if (m_ShouldUpdate)
            {
                AnimateColour();

                m_CurrentTime += Time.deltaTime;

                if (m_CurrentTime >= secsToExplode)
                {
                    m_ShouldUpdate = false;
                    Explode();
                }
            }
        }

        private void AnimateColour()
        {
            int currentColour = Mathf.RoundToInt(m_CurrentColour);

            if (m_PreviousColour != currentColour)
            {
                if (audioToPlayOnFlash != null)
                {
                    m_Audio.PlayInstance(audioToPlayOnFlash);
                }

                m_Renderer.color = colors[currentColour];
                m_PreviousColour = currentColour;
            }

            m_CurrentColour += FLASH_OFFSET * m_CurrentTime * Time.deltaTime;

            if (m_CurrentColour > colors.Length - 1)
            {
                m_CurrentColour = 0f;
            }
        }

        private void Explode()
        {
            var hitObjs = Physics2D.OverlapCircleAll(transform.position, radius);

            foreach (var hitObj in hitObjs)
            {
                if (hitObj.CompareTag("Enemy") || hitObj.CompareTag("Blackhole"))
                {
                    var explosion = hitObj.GetComponent<BombListener>();

                    if (explosion != null)
                    {
                        explosion.ExplosionInRange(damage);
                    }
                }
            }

            m_Audio.PlayInstance(audioToPlayOnExplode);
            m_Shake.Begin(0.2f, 0.5f);

            m_Renderer.enabled = false;

            SpawnExplosion(transform.position);

            Destroy(gameObject);
        }

        private IEnumerator DestroyEnemies(List<BombListener> enemies)
        {
            foreach (var enemy in enemies)
            {
                if (enemy != null)
                {
                    enemy.ExplosionInRange(damage);
                    yield return new WaitForSeconds(0.025f);
                }
            }

            Destroy(gameObject);
        }

        private void SpawnExplosion(Vector2 position)
        {
            WarpGrid.WarpingGrid.Instance.ApplyExplosiveForce(15f, transform.position, radius * 1.5f);

            for (int i = 0; i < NUM_OF_PARTICLES_ON_DEATH; i++)
            {
                float speed = (12f * (1f - 1 / 2f)) * 0.02f;

                var state = new ParticleBuilder()
                {
                    velocity = StaticExtensions.Random.RandomVector2(speed, speed),
                    wrapAroundType = WrapAroundType.None,
                    lengthMultiplier = 20f,
                    velocityDampModifier = 0.94f,
                    removeWhenAlphaReachesThreshold = true,
                    canBeCollectedByPlayer = true,
                    maxLengthClamp = 1.5f
                };

                float duration = Random.Range(200f, 320f);
                var initialScale = new Vector2(1f, 1f);

                ParticleFactory.instance.CreateParticle(position,
                    colors[Mathf.RoundToInt(m_CurrentColour)], duration, initialScale, state);
            }
        }
    }
}