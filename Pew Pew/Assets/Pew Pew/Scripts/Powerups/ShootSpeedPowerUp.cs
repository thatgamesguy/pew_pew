using UnityEngine;
using System.Collections;
using System;

namespace GameCore
{
    /// <summary>
    /// Increases players shooting speed temporaily when picked up.
    /// </summary>
    public class ShootSpeedPowerUp : PowerUpImpl
    {
        /// <summary>
        /// The seconds decrement between players shots.
        /// </summary>
        public float secsBetweenShotDecrement = 0.4f;

        /// <summary>
        /// The amount of time shot speed is increased.
        /// </summary>
        public float secSpeedIncrease = 3f;

        private static PlayerShoot PLAYER_SHOOT;

        /// <summary>
        /// Perform the specified powerup action. Finds main player shoot module and invokes PlayerShoot::DecrementSecBetweenShotsForSeconds.
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
                PLAYER_SHOOT.DecrementSecBetweenShotsForSeconds(secsBetweenShotDecrement, secSpeedIncrease);
            }

            PARTICLE_EXPLOSION.Spawn(transform.position, numOfParticlesToSpawn, particleColour);

            ShowMessage("SHOOT SPEED");

            Destroy(gameObject);
        }
    }
}