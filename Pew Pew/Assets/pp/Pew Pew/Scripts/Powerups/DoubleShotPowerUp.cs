using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Provides ability for player to shoot two projectiles in parallel for a specified amount of time.
    /// </summary>
    public class DoubleShotPowerUp : PowerUpImpl
    {
        /// <summary>
        /// Powerup duration.
        /// </summary>
        public float secPowerUp = 3f;

        private static PlayerShoot PLAYER_SHOOT;

        /// <summary>
        /// Perform the specified powerup action. Finds main player shoot module and invokes PlayerShoot::DoubleShootingForSeconds.
        /// </summary>
        /// <param name="player">Player tranform.</param>
        public override void Perform(Transform player)
        {
            if (PLAYER_SHOOT == null)
            {
                foreach (Transform t in player)
                {
                    if (t.CompareTag("ShootModules"))
                    {
                        foreach (Transform shootModules in t)
                        {
                            if (shootModules.CompareTag("MainShootModule"))
                            {
                                PLAYER_SHOOT = shootModules.GetComponent<PlayerShoot>();
                                break;
                            }
                        }
                    }


                }
            }

            if (PLAYER_SHOOT != null)
            {
                PLAYER_SHOOT.DoubleShootingForSeconds(secPowerUp);
            }

            PARTICLE_EXPLOSION.Spawn(transform.position, numOfParticlesToSpawn, particleColour);

            ShowMessage("DOUBLE SHOT");

            Destroy(gameObject);
        }
    }
}