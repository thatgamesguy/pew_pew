using UnityEngine;
using System.Collections;

namespace GameCore
{
    /// <summary>
    /// Contract for any entity that can shoot projectiles.
    /// </summary>
    public interface ShootRequestable
    {
        void RequestShoot();
    }

    /// <summary>
    /// Provides functionality to request projectiles from a pool.
    /// </summary>
    public class EnemyShootWhenRequested : MonoBehaviour, ProjectileReturn, ShootRequestable
    {
        /// <summary>
        /// The projectile prefab.
        /// </summary>
        public GameObject projectilePrefab;

        /// <summary>
        /// The audio to play on shoot.
        /// </summary>
        public AudioClip audioOnShoot;

        /// <summary>
        /// The projectiles damage.
        /// </summary>
        public int damage = 1;

        /// <summary>
        /// The possible shoot directions.
        /// </summary>
        public Vector2[] shootDirections;

        /// <summary>
        /// The number of projectiles to pool.
        /// </summary>
        public int numProjectilesToPool = 4;

        private AudioPlayer m_Audio;
        private ObjectPool<Projectile> m_ObjectPool;
        private int m_CurrentShootDirIndex;
        private SpriteRenderer m_Renderer;

        void Awake()
        {
            m_Audio = Camera.main.GetComponent<AudioPlayer>();
            m_Renderer = transform.parent.GetComponent<SpriteRenderer>();
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
                    numProjectilesToPool);
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

        /// <summary>
        /// Requests to shoot a projectile. A projectile will be released as long as the pool returns a projectile.
        /// </summary>
        public void RequestShoot()
        {
            if (!m_Renderer.isVisible)
            {
                return;
            }

            var projectile = GetProjectile();

            if (projectile)
            {
                Vector2 shootDir = shootDirections[m_CurrentShootDirIndex];

                projectile.transform.position = transform.position;
                projectile.Initialise(
                    this,
                    shootDir,
                    damage);
                m_Audio.PlayInstance(audioOnShoot);

                m_CurrentShootDirIndex = (m_CurrentShootDirIndex + 1) % shootDirections.Length;
            }
        }

        /// <summary>
        /// Pools the projectile. Returns the projectile to a pool to be reused later.
        /// </summary>
        /// <param name="projectile">Projectile to disable and place into the pool.</param>
        public void PoolProjectile(Projectile projectile)
        {
            m_ObjectPool.PoolObject(projectile);
        }

        private Projectile GetProjectile()
        {
            return m_ObjectPool.GetObject();
        }
    }
}