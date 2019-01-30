using UnityEngine;
using System;
using System.Collections;
using WarpGrid;
using PE2D;

namespace GameCore
{
    /// <summary>
    /// Handles player health, applying damage, losing lives, and respawning.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerHealth : MonoBehaviour, HitListener
    {
        /// <summary>
        /// The sprite renderers to enable/disable on death/respawn.
        /// </summary>
        public SpriteRenderer[] spriteRenderers;

        /// <summary>
        /// The initial lives. Additional lives can be purchased by the player in the store.
        /// </summary>
        public int initialLives = 3;

        /// <summary>
        /// The particle colour on death.
        /// </summary>
        public Color particleColourOnDeath;

        /// <summary>
        /// The on death action. Called when player loses a life.
        /// </summary>
        public Action OnDeath;

        /// <summary>
        /// Invoked when player dies, just before re-spawning.
        /// </summary>
        public Action OnPlayerDeathPreSpawn;

        /// <summary>
        /// The on spawn action. Called when player spawns.
        /// </summary>
        public Action OnSpawn;

        /// <summary>
        /// The audio to player on player death.
        /// </summary>
        public AudioClip audioOnPlayerDeath;

        /// <summary>
        /// The number of seconds it takes for the player to respawn.
        /// </summary>
        public float secondsToRespawn = 2f;

        private static readonly int NUM_OF_PARTICLES_ON_DEATH = 40;

        private int m_CurrentLives;
        private GameManager m_GameManager;
        private PlayerItemUI m_PlayerLivesUI;
        private PlayerShootController m_ShootController;
        private AudioPlayer m_AudioPlayer;
        private BGMAudioPlayer m_AudioControls;
        private Collider2D m_Collider;

        void Awake()
        {
            m_GameManager = GameObject.FindObjectOfType<GameManager>();
            m_PlayerLivesUI = GameObject.FindGameObjectWithTag("LivesUI").GetComponentInChildren<PlayerItemUI>();

            m_ShootController = GetComponent<PlayerShootController>();

            m_AudioPlayer = FindObjectOfType<AudioPlayer>();

            m_AudioControls = FindObjectOfType<BGMAudioPlayer>();

            if (m_AudioControls == null)
            {
                var audioObj = new GameObject("BGM");
                audioObj.AddComponent<AudioSource>();
                m_AudioControls = audioObj.AddComponent<BGMAudioPlayer>();
            }

            m_Collider = GetComponent<Collider2D>();

            m_CurrentLives = initialLives;
        }

        void OnEnable()
        {
            m_PlayerLivesUI.SetItemCount(m_CurrentLives - 1);
        }

        /// <summary>
        /// Increments the number of players lives.
        /// </summary>
        public void IncrementLives()
        {
            m_CurrentLives++;
            m_PlayerLivesUI.SetItemCount(m_CurrentLives - 1);
        }

        /// <summary>
        /// Removes a players life. 
        /// </summary>
        /// <param name="damage">Damage taken.</param>
        public void OnHit(int damage)
        {
            LostLife();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Enemy") || other.CompareTag("Blackhole"))
            {
                var otherHealth = other.GetComponent<EnemyHealth>();

                if (otherHealth != null)
                {
                    otherHealth.Kill(false);
                }

                LostLife();
            }
        }

        private void LostLife()
        {
            m_ShootController.PauseAll();

            m_CurrentLives--;

            m_AudioPlayer.PlayInstance(audioOnPlayerDeath);

            WarpGrid.WarpingGrid.Instance.ApplyExplosiveForce(4f, transform.position, 3f);

            SpawnExplosion(transform.position);

            if (OnDeath != null)
            {
                OnDeath();
            }

            foreach (var rend in spriteRenderers)
            {
                rend.enabled = false;
            }

            m_Collider.enabled = false;

            if (m_CurrentLives <= 0) // Game over 
            {
                m_PlayerLivesUI.SetItemCount(0);
                m_GameManager.OnPlayerDeathGameOver();
            }
            else
            {
                m_AudioControls.SetVolume(0.2f, secondsToRespawn * 0.4f);
                m_AudioControls.SetVolume(1f, secondsToRespawn * 0.6f);

                m_AudioControls.SetPitch(0.98f, secondsToRespawn * 0.4f);
                m_AudioControls.SetPitch(1f, secondsToRespawn * 0.6f);

                m_PlayerLivesUI.SetItemCount(m_CurrentLives - 1);

                m_GameManager.OnPlayerDied();

                if (gameObject.activeSelf)
                {
                    StartCoroutine(ResetAfterSeconds(secondsToRespawn));
                }
            }
        }

        private void SpawnExplosion(Vector2 position)
        {
            float hue1 = UnityEngine.Random.Range(0f, 6f);
            float hue2 = (hue1 + UnityEngine.Random.Range(0f, 2f)) % 6f;
            Color colour1 = StaticExtensions.Color.FromHSV(hue1, 0.5f, 1);
            Color colour2 = StaticExtensions.Color.FromHSV(hue2, 0.5f, 1);

            for (int i = 0; i < NUM_OF_PARTICLES_ON_DEATH; i++)
            {
                float speed = (12f * (1f - 1 / UnityEngine.Random.Range(2f, 3f))) * 0.02f;

                var state = new ParticleBuilder()
                {
                    velocity = StaticExtensions.Random.RandomVector2(speed, speed),
                    wrapAroundType = WrapAroundType.None,
                    lengthMultiplier = 40f,
                    velocityDampModifier = 0.94f,
                    removeWhenAlphaReachesThreshold = true,
                    canBeCollectedByPlayer = false,
                    maxLengthClamp = 1.5f
                };

                var colour = Color.Lerp(colour1, colour2, UnityEngine.Random.Range(0f, 1f));

                float duration = UnityEngine.Random.Range(200f, 320f);
                var initialScale = new Vector2(1f, 1f);


                ParticleFactory.instance.CreateParticle(position, colour, duration, initialScale, state);
            }
        }

        private IEnumerator ResetAfterSeconds(float seconds)
        {
            yield return new WaitForSeconds(seconds);

            if (OnPlayerDeathPreSpawn != null)
            {
                OnPlayerDeathPreSpawn();
            }

            transform.position = new Vector3(0f, -3f, 0f);

            float startWait = 0.08f;

            for (int i = 0; i < 8; i++)
            {
                foreach (var rend in spriteRenderers)
                {
                    rend.enabled = !rend.enabled;
                }

                yield return new WaitForSeconds(startWait *= 0.9f);
            }

            foreach (var rend in spriteRenderers)
            {
                rend.enabled = true;
            }

            m_Collider.enabled = true;

            if (OnSpawn != null)
            {
                OnSpawn();
            }

            m_ShootController.BeginShooting();
            m_ShootController.ResumeAll();

            m_GameManager.OnPlayerRespawned();
        }
    }
}