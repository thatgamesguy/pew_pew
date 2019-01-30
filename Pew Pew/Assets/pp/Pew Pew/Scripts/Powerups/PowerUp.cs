using UnityEngine;
using System.Collections;
using PE2D;

namespace GameCore
{
    /// <summary>
    /// Contract for all in-game powerups.
    /// </summary>
    public interface PowerUp
    {
        /// <summary>
        /// Perform the specified powerup action.
        /// </summary>
        /// <param name="player">Player tranform.</param>
        void Perform(Transform player);
    }

    /// <summary>
    /// The abstract base class for all powerups. Provides access to UI text system (to show powerup name) and any common fields.
    /// </summary>
    public abstract class PowerUpImpl : MonoBehaviour, PowerUp
    {
        /// <summary>
        /// The maximum time the powerup can be on the floor before either dissapearing or bring picked up by the player.
        /// </summary>
        public float maxTimeAlive = 2f;

        /// <summary>
        /// The seconds before the powerup starts flashing. Used to indicate to the player that the powerup will shortly be removed from the game unless they pick it up.
        /// </summary>
        public float flashTime = 1.5f;

        /// <summary>
        /// The time between flashes.
        /// </summary>
        public float timeBetweenFlashes = 0.07f;

        [Header("Explosion")]
        /// <summary>
        /// The colour of particles spawned when powerup is picked up.
        /// </summary>
        public Color particleColour;

        /// <summary>
        /// The number of particles to spawn when player is picked up.
        /// </summary>
        public int numOfParticlesToSpawn = 10;

        protected static PowerUpParticleExplosion PARTICLE_EXPLOSION;

        private static PointPopUpUI POINTS_FACTORY;
        private static PauseHandler PAUSE_HANDLER;

        private float m_CurrentTimeAlive = 0f;
        private SpriteRenderer m_Renderer;
        private bool m_FlashingRunning = false;

        void Awake()
        {
            m_Renderer = GetComponent<SpriteRenderer>();

            if (m_Renderer == null)
            {
                m_Renderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (!PAUSE_HANDLER)
            {
                PAUSE_HANDLER = GameObject.FindGameObjectWithTag("UI").GetComponent<PauseHandler>();
            }
        }

        protected virtual void Start()
        {
            m_CurrentTimeAlive = 0f;

            if (PARTICLE_EXPLOSION == null)
            {
                PARTICLE_EXPLOSION = new PowerUpParticleExplosion();
            }

            m_FlashingRunning = true;
            StartCoroutine(StartFlashing());
        }

        /// <summary>
        /// Perform the specified powerup action. Implement in concrete base classes.
        /// </summary>
        /// <param name="player">Player tranform.</param>
        public abstract void Perform(Transform player);

        protected void ShowMessage(object msg)
        {
            if (POINTS_FACTORY == null)
            {
                POINTS_FACTORY = GameObject.FindGameObjectWithTag("PointsUI").GetComponent<PointPopUpUI>();
            }

            POINTS_FACTORY.ShowTextAtPosition(msg, transform.position);
        }

        void Update()
        {
            if (PAUSE_HANDLER.isPaused) { return; }

            m_CurrentTimeAlive += Time.deltaTime;

            if (m_CurrentTimeAlive >= maxTimeAlive)
            {
                m_FlashingRunning = false;
                Destroy(gameObject);
            }
        }

        private IEnumerator StartFlashing()
        {
            while (m_FlashingRunning)
            {
                if (PAUSE_HANDLER.isPaused)
                {
                    yield return new WaitUntil(() => !PAUSE_HANDLER.isPaused);
                }

                if (m_CurrentTimeAlive >= flashTime)
                {

                    m_Renderer.enabled = !m_Renderer.enabled;

                    yield return new WaitForSeconds(timeBetweenFlashes);

                    timeBetweenFlashes *= 0.98f;
                }
                else
                {
                    yield return new WaitForEndOfFrame();
                }
            }
        }
    }

    /// <summary>
    /// Spawns particle explosion on particle pick up.
    /// </summary>
    public class PowerUpParticleExplosion
    {
        /// <summary>
        /// Spawn the specified particle explosion at position, with numOfParticles and of particleColour.
        /// </summary>
        /// <param name="position">The position of explosion.</param>
        /// <param name="numOfParticles">The number of particles.</param>
        /// <param name="particleColour">The particle colour.</param>
        public void Spawn(Vector2 position,
            int numOfParticles, Color particleColour)
        {
            float hue1 = UnityEngine.Random.Range(0f, 6f);
            float hue2 = (hue1 + UnityEngine.Random.Range(0f, 2f)) % 6f;
            Color colour1 = StaticExtensions.Color.FromHSV(hue1, 0.5f, 1);
            Color colour2 = StaticExtensions.Color.FromHSV(hue2, 0.5f, 1);

            for (int i = 0; i < numOfParticles; i++)
            {
                float speed = (12f * (1f - 1 / Random.Range(2f, 3f))) * 0.02f;

                var state = new ParticleBuilder()
                {
                    velocity = StaticExtensions.Random.RandomVector2(speed, speed),
                    wrapAroundType = WrapAroundType.None,
                    lengthMultiplier = 40f,
                    velocityDampModifier = 0.94f,
                    removeWhenAlphaReachesThreshold = true,
                    canBeCollectedByPlayer = true,
                    maxLengthClamp = 1.5f
                };

                var colour = Color.Lerp(colour1, colour2, UnityEngine.Random.Range(0f, 1f));

                float duration = Random.Range(200f, 320f);
                var initialScale = new Vector2(1f, 1f);

                ParticleFactory.instance.CreateParticle(position, colour, duration, initialScale, state);
            }
        }
    }
}