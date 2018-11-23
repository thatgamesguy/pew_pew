using UnityEngine;
using System.Collections;

namespace GameCore
{
    /// <summary>
    /// A contract for any entity that can pool a projectile.
    /// </summary>
    public interface ProjectileReturn
    {
        /// <summary>
        /// Adds the projectile to a pool.
        /// </summary>
        /// <param name="p">The projectile to pool.</param>
        void PoolProjectile(Projectile p);
    }

    /// <summary>
    /// Provides shoot functionality for the player. Projectiles are retrieved from a pool.
    /// Also provides burst functionality.
    /// </summary>
    public class PlayerShoot : MonoBehaviour, ProjectileReturn
    {
        /// <summary>
        /// The projectile damage.
        /// </summary>
        public int damage = 1;

        /// <summary>
        /// The bullet prefab to spawn.
        /// </summary>
        public GameObject bulletPrefab;

        /// <summary>
        /// The seconds between projectile release.
        /// </summary>
        public float secsBetweenShot = 0.2f;

        /// <summary>
        /// The audio to play on shoot.
        /// </summary>
        public AudioClip audioOnShoot;

        [Header("Bursts")]
        /// <summary>
        /// The number of bullets per burst. This amount is increased through shop purchases.
        /// </summary>
        public int bulletsPerBurst = 1;

        /// <summary>
        /// The second delay between bullets in a burst.
        /// </summary>
        public float secDelayBetweenBulletsInBurst = 0.2f;

        /// <summary>
        /// The number of projectiles to create at the beginning of the game.
        /// </summary>
        public int numToPool = 12;

        private static readonly float MIN_SEC_BETWEEN_SHOTS = 0.2f;
        private static readonly float DOUBLE_SHOT_OFFSET = 0.08f;

        private bool m_Shooting = true;
        private bool m_Paused = false;
        private AudioPlayer m_Audio;
        private ObjectPool<Projectile> m_ObjectPool;
        private float m_InitialSecBetweenShots;
        private ShootRecoil m_Recoil;
        private bool m_SpeedBoost = false;
        private bool m_DoubleShooting = false;

        void Awake()
        {
            m_Audio = Camera.main.GetComponent<AudioPlayer>();
            m_Recoil = GetComponent<ShootRecoil>();
        }

        void OnEnable()
        {
            if (m_ObjectPool == null)
            {
                m_ObjectPool = new ObjectPool<Projectile>(bulletPrefab, numToPool);
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

            if (m_SpeedBoost)
            {
                ResetSecBetweenShots();
            }

            m_DoubleShooting = false;
        }

        void OnDisable()
        {
            m_Shooting = false;
        }

        /// <summary>
        /// Pause this instance.
        /// </summary>
        public void Pause()
        {
            m_Paused = true;
        }

        /// <summary>
        /// Resume this instance.
        /// </summary>
        public void Resume()
        {
            m_Paused = false;
        }

        /// <summary>
        /// Begin shooting.
        /// </summary>
        public void BeginShooting()
        {
            m_Shooting = true;
            StartCoroutine(Shoot());
        }

        /// <summary>
        /// Adds the projectile to a pool for later use.
        /// </summary>
        /// <param name="p">The projectile to pool.</param>
        /// <param name="projectile">Projectile.</param>
        public void PoolProjectile(Projectile projectile)
        {
            m_ObjectPool.PoolObject(projectile);
        }

        /// <summary>
        /// Decrements the seconds between shots. The effect lasts for the number of seconds passed.
        /// </summary>
        /// <param name="decrement">The amount to decrement the time between shots.</param>
        /// <param name="seconds">The amount of time the decrement lasts.</param>
        public void DecrementSecBetweenShotsForSeconds(float decrement, float seconds)
        {
            if (!m_SpeedBoost)
            {
                m_SpeedBoost = true;

                m_InitialSecBetweenShots = secsBetweenShot;

                secsBetweenShot -= decrement;

                if (secsBetweenShot < MIN_SEC_BETWEEN_SHOTS)
                {
                    secsBetweenShot = MIN_SEC_BETWEEN_SHOTS;
                }

                Invoke("ResetSecBetweenShots", seconds);

            }
        }

        /// <summary>
        /// Decrements the seconds between shots permantly.
        /// </summary>
        /// <param name="decrement">The amount to decrement the time between shots.</param>
        public void DecrementSecBetweenShots(float decrement)
        {
            if (m_SpeedBoost)
            {
                m_InitialSecBetweenShots -= decrement;

                if (m_InitialSecBetweenShots < MIN_SEC_BETWEEN_SHOTS)
                {
                    m_InitialSecBetweenShots = MIN_SEC_BETWEEN_SHOTS;
                }
            }
            else
            {
                secsBetweenShot -= decrement;

                if (secsBetweenShot < MIN_SEC_BETWEEN_SHOTS)
                {
                    secsBetweenShot = MIN_SEC_BETWEEN_SHOTS;
                }
            }
        }

        /// <summary>
        /// Increases the damage of the projectiles.
        /// </summary>
        /// <param name="increment">The amount to increment damage.</param>
        public void IncrementDamage(int increment = 1)
        {
            damage += increment;
        }

        /// <summary>
        /// Increment the number of shots in a burst.
        /// </summary>
        /// <param name="increment">The number of bullets to add to each burst.</param>
        public void IncrementShotBurst(int increment = 1)
        {
            bulletsPerBurst += increment;
        }

        /// <summary>
        /// Adds temporary powerup, enabling player to shoot two parallel projectiles.
        /// </summary>
        /// <param name="seconds">The seconds the powerup lasts.</param>
        public void DoubleShootingForSeconds(float seconds)
        {
            m_DoubleShooting = true;

            Invoke("ResetDoubleShooting", seconds);
        }

        private void ResetDoubleShooting()
        {
            m_DoubleShooting = false;
        }

        private void ResetSecBetweenShots()
        {
            m_SpeedBoost = false;
            secsBetweenShot = m_InitialSecBetweenShots;
        }

        private IEnumerator Shoot()
        {
            yield return new WaitForSeconds(Random.Range(0f, secsBetweenShot));

            while (m_Shooting)
            {
                float waitTime = secsBetweenShot;

                if (!m_Paused)
                {
                    for (int i = 0; i < bulletsPerBurst; i++)
                    {
                        if (m_DoubleShooting)
                        {
                            bool shotOne = ShootProjectileAtPosition(transform.position -
                                (transform.right * DOUBLE_SHOT_OFFSET), i == 0 ? damage : 1);
                            bool shotTwo = ShootProjectileAtPosition(transform.position + (
                                transform.right * DOUBLE_SHOT_OFFSET), i == 0 ? damage : 1);

                            waitTime = (shotOne && shotTwo) ? secsBetweenShot : secsBetweenShot * 0.1f;
                        }
                        else
                        {
                            bool shot = ShootProjectileAtPosition(transform.position, i == 0 ? damage : 1);

                            waitTime = shot ? secsBetweenShot : secsBetweenShot * 0.1f;
                        }

                        if (bulletsPerBurst > 1)
                        {
                            yield return new WaitForSeconds(secDelayBetweenBulletsInBurst);
                        }
                    }
                }

                yield return new WaitForSeconds(waitTime);

            }
        }

        private bool ShootProjectileAtPosition(Vector2 position, int projDmg)
        {
            var projectile = GetProjectile();

            if (projectile)
            {
                projectile.transform.position = position;
                projectile.Initialise(this, Vector2.up, projDmg);
                m_Audio.PlayInstance(audioOnShoot);

                if (m_Recoil != null)
                {
                    m_Recoil.Execute();
                }

                return true;
            }

            return false;
        }

        private Projectile GetProjectile()
        {
            return m_ObjectPool.GetObject();
        }
    }
}