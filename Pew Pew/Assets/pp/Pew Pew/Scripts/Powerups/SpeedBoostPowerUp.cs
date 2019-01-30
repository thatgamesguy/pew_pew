using UnityEngine;
using PE2D;

namespace GameCore
{
    /// <summary>
    /// Increases players movement speed temporarily.
    /// </summary>
    public class SpeedBoostPowerUp : PowerUpImpl
    {
        /// <summary>
        /// The amount to increase movement speed.
        /// </summary>
        public float speedIncrease = 5f;

        /// <summary>
        /// How long the players movement speed is increased.
        /// </summary>
        public float secSpeedIncrease = 3f;

        /// <summary>
        /// Perform the specified powerup action. Invokes PlayerController::IncrementSpeedForSeconds
        /// </summary>
        /// <param name="player">Player tranform.</param>
        public override void Perform(Transform player)
        {
            var controller = player.GetComponent<PlayerController>();

            controller.IncrementSpeedForSeconds(speedIncrease, secSpeedIncrease);

            PARTICLE_EXPLOSION.Spawn(transform.position, numOfParticlesToSpawn, particleColour);

            ShowMessage("SPEED BOOST");

            Destroy(gameObject);
        }
    }
}