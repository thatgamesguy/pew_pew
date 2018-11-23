using UnityEngine;
using System.Collections;
using PE2D;

namespace GameCore
{
    /// <summary>
    /// Provides shield functionality for player (when purchased through the in-game store).
    /// </summary>
    public class Shield : MonoBehaviour, HitListener
    {
        /// <summary>
        /// The amount of damage given when an enemy hits the shield.
        /// </summary>
        public int damage = 10;

        /// <summary>
        /// The speed at which the shield orbits the player.
        /// </summary>
        public float orbitSpeed = 40f;

        /// <summary>
        /// The color of the particles released when the shield is destroyed.
        /// </summary>
        public Color colorExplosion;

        /// <summary>
        /// The number of particles released on death.
        /// </summary>
        public int numOfParticlesOnDeath = 80;

        /// <summary>
        /// Removes the shield from the game. Spawns particle explosion.
        /// </summary>
        /// <param name="damage">Damage taken. As the shield can only take one hit, this is ignored.</param>
        public void OnHit(int damage)
        {
            Remove();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if ((other.gameObject.CompareTag("Enemy") && other.gameObject.GetComponent<StationaryMovement>() == null)
                || other.gameObject.CompareTag("Blackhole"))
            {

                var hits = other.GetComponents<HitListener>();

                foreach (var hit in hits)
                {
                    hit.OnHit(damage);
                }

                Remove();
            }
        }

        private void Remove()
        {
            SpawnExplosion(transform.position);

            gameObject.SetActive(false);
        }

        private void SpawnExplosion(Vector2 position)
        {
            float hue1 = UnityEngine.Random.Range(0f, 6f);
            float hue2 = (hue1 + UnityEngine.Random.Range(0f, 2f)) % 6f;
            Color colour1 = StaticExtensions.Color.FromHSV(hue1, 0.5f, 1);
            Color colour2 = StaticExtensions.Color.FromHSV(hue2, 0.5f, 1);

            for (int i = 0; i < numOfParticlesOnDeath; i++)
            {
                float speed = (12f * (1f - 1 / 2f)) * 0.015f;

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


                var colour = Color.Lerp(colour1, colour2, UnityEngine.Random.Range(0f, 1f));


                float duration = Random.Range(200f, 320f);
                var initialScale = new Vector2(1f, 1f);

                ParticleFactory.instance.CreateParticle(position, colour, duration, initialScale, state);
            }
        }
    }
}