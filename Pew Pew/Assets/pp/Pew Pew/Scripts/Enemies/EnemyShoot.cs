using UnityEngine;
using System.Collections;

namespace GameCore
{
    /// <summary>
    /// Contract for any entity that can begin, pause, or resume shooting.
    /// </summary>
    public interface EnemyShootStatusChange
    {
        /// <summary>
        /// Begin shooting.
        /// </summary>
        void Begin();

        /// <summary>
        /// Pause shooting.
        /// </summary>
        void Pause();

        /// <summary>
        /// Resume shooting.
        /// </summary>
        void Resume();
    }

    /// <summary>
    /// Shoots projectiles. Projectiles are pooled.
    /// </summary>
    public class EnemyShoot : MonoBehaviour, ProjectileReturn,
        AdjustableShootSpeed, EnemyShootStatusChange
    {
        /// <summary>
        /// The projectile prefab to spawn.
        /// </summary>
        public GameObject projectilePrefab;

        /// <summary>
        /// The seconds between shoot requests.
        /// </summary>
        public float secsBetweenShot = 1.2f;

        /// <summary>
        /// The amount to decrement time between shoot requests near round end.
        /// </summary>
        public float shootSpeedDecrement = 0.3f;

        /// <summary>
        /// The audio to play on shoot.
        /// </summary>
        public AudioClip audioOnShoot;

        /// <summary>
        /// Sets the Y value to begin shooting. Enemies above this value will not shoot.
        /// </summary>
        public float beginShootingWhenBelowScreenY = 100f;

        /// <summary>
        /// The damage to apply when projectile hits player.
        /// </summary>
        public int damage = 1;

        /// <summary>
        /// The possible shoot directions.
        /// </summary>
        public Vector2[] shootDirections;

        /// <summary>
        /// Sets whether projectiles are shot based on owners rotation.
        /// </summary>
        public bool shootBasedOnRotation = false;

        private static readonly int NUM_PROJ_TO_POOL = 2;

        private bool m_Shooting = false;
        private AudioPlayer m_Audio;
        private ObjectPool<Projectile> m_ObjectPool;
        private int m_CurrentShootDirIndex;
        private SpriteRenderer m_SpriteRenderer;
        private bool m_StopNextActivate = false;
        private bool m_Paused = false;

        void Awake()
        {
            m_Audio = Camera.main.GetComponent<AudioPlayer>();
            m_SpriteRenderer = transform.parent.GetComponent<SpriteRenderer>();
        }

        void Start()
        {
            if (shootDirections == null || shootDirections.Length == 0)
            {
                shootDirections = new Vector2[1];
                shootDirections[0] = -Vector2.up;
            }

            m_CurrentShootDirIndex = Random.Range(0, shootDirections.Length);

            if (m_ObjectPool == null)
            {
                m_ObjectPool = new ObjectPool<Projectile>(projectilePrefab,
                    NUM_PROJ_TO_POOL);
            }
            else
            {
                // Remove any child projectiles
                var activeProjectiles = m_ObjectPool.GetActive();

                foreach (var activeProjectile in activeProjectiles)
                {
                    m_ObjectPool.PoolObject(activeProjectile);
                }
            }
        }

        void OnEnable()
        {
            GameManager.EnemyShoots.Add(this);
        }

        void OnDisable()
        {
            GameManager.EnemyShoots.Remove(this);

            m_Shooting = false;
        }

        /// <summary>
        /// Begin shooting.
        /// </summary>
        public void Begin()
        {
            if (m_StopNextActivate)
            {
                m_StopNextActivate = false;
            }
            else
            {
                BeginShooting();
            }
        }

        /// <summary>
        /// Pause shooting.
        /// </summary>
        public void Pause()
        {
            m_Paused = true;
        }

        /// <summary>
        /// Resume shooting.
        /// </summary>
        public void Resume()
        {
            Invoke("_Resume", Random.Range(0.5f, 1.5f));
        }

        /// <summary>
        /// Stops the next activation attempt of this module.
        /// </summary>
        public void StopActivation()
        {
            m_StopNextActivate = true;
        }

        /// <summary>
        /// Decrements the time between shot requests.
        /// </summary>
        public void IncrementSpeed()
        {
            secsBetweenShot -= shootSpeedDecrement;
        }

        /// <summary>
        /// Pools the projectile.
        /// </summary>
        /// <param name="projectile">Projectile.</param>
        public void PoolProjectile(Projectile projectile)
        {
            m_ObjectPool.PoolObject(projectile);
        }

        private void _Resume()
        {
            m_Paused = false;
        }

        private void BeginShooting()
        {
            m_Shooting = true;
            StartCoroutine(Shoot());
        }

        private IEnumerator Shoot()
        {
            yield return new WaitForSeconds(Random.Range(0f, secsBetweenShot));

            while (m_Shooting)
            {
                if (IsVisible() && !m_Paused)
                {
                    var projectile = GetProjectile();

                    if (projectile)
                    {
                        Vector2 shootDir = shootDirections[m_CurrentShootDirIndex];

                        if (shootBasedOnRotation)
                        {
                            shootDir = (Vector2)transform.up;
                        }

                        projectile.transform.position = transform.position;
                        projectile.Initialise(
                            this,
                            shootDir,
                            damage);
                        m_Audio.PlayInstance(audioOnShoot);

                        m_CurrentShootDirIndex = (m_CurrentShootDirIndex + 1) % shootDirections.Length;

                        // Add some randomness to shot order.
                        yield return new WaitForSeconds(Random.Range(0f, secsBetweenShot * 0.5f));
                    }
                }

                yield return new WaitForSeconds(secsBetweenShot);

            }
        }

        private Projectile GetProjectile()
        {
            return m_ObjectPool.GetObject();
        }

        private bool IsVisible()
        {
            if (m_SpriteRenderer == null)
            {
                return true;
            }

            return m_SpriteRenderer.isVisible;
        }
    }
}