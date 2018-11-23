using UnityEngine;
using System;

namespace GameCore
{
    /// <summary>
    /// Provides bonus particles.
    /// </summary>
    public class BonusScorePowerUp : PowerUpImpl
    {
        /// <summary>
        /// Perform the specified powerup action. Spawns additional particles at powerup location.
        /// </summary>
        /// <param name="player">Player tranform.</param>
        public override void Perform(Transform player)
        {
            PARTICLE_EXPLOSION.Spawn(transform.position, numOfParticlesToSpawn, particleColour);

            ShowMessage("BONUS SCORE");

            Destroy(gameObject);
        }
    }
}