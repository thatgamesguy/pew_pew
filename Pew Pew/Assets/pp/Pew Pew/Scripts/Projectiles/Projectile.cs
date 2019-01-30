using UnityEngine;
using PE2D;
using System.Collections.Generic;
using WarpGrid;

namespace GameCore
{
    /// <summary>
    /// Contract for any projectile that can be returned to a pool.
    /// </summary>
    public interface PoolableProjectile
    {
        /// <summary>
        /// Gets the damage of projectile.
        /// </summary>
        /// <value>The damage.</value>
        int damage { get; }

        /// <summary>
        /// Returns the projectile to an object pool.
        /// </summary>
        void ReturnProjectile();
    }

    /// <summary>
    /// The standard projectile. Is poolable and effected by blackhole and repel GameObjects.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour, PoolableProjectile
    {
        /// <summary>
        /// The force applied to the Rigidbody attached to the projectile.
        /// </summary>
        public float moveForce = 100f;

        /// <summary>
        /// The maximum time the projectile is alive before being returned to a pool.
        /// </summary>
        public float timeAlive = 2f;

        /// <summary>
        /// Effectors force is multipled by this.
        /// </summary>
        public float effectorMultiplier = 200f;

        /// <summary>
        /// Gets the damage of projectile.
        /// </summary>
        /// <value>The damage.</value>
        public int damage { get; private set; }

        protected Rigidbody2D m_Rigidbody2D;
        protected float m_CurrentTimeAlive = 0f;
        protected bool m_Paused = false;

        private static readonly float EFFCTOR_OFFSET = 10000f;

        private ProjectileReturn m_Owner;
        private List<ParticleEffector> m_Effectors;
        private Vector2 m_StoredVel;

        protected virtual void Awake()
        {
            m_Rigidbody2D = GetComponent<Rigidbody2D>();
        }

        void OnEnable()
        {
            m_CurrentTimeAlive = 0f;

            var effectors = GameObject.FindObjectsOfType<ParticleEffector>();

            m_Effectors = new List<ParticleEffector>();

            for (int i = 0; i < effectors.Length; i++)
            {
                if (effectors[i].effectProjectiles)
                {
                    m_Effectors.Add(effectors[i]);
                }
            }
        }

        /// <summary>
        /// Initialise the specified projectile with: owner, direction and damage.
        /// Applies force to move the projectile in the desired direction.
        /// </summary>
        /// <param name="owner">The object pool.</param>
        /// <param name="dir">Movement direction.</param>
        /// <param name="damage">Damage.</param>
        public void Initialise(ProjectileReturn owner,
            Vector2 dir, int damage)
        {
            this.damage = damage;
            m_Owner = owner;
            m_Rigidbody2D.AddForce(dir * moveForce);

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        /// <summary>
        /// Returns the projectile to an object pool.
        /// </summary>
        public void ReturnProjectile()
        {
            m_Owner.PoolProjectile(this);
        }

        /// <summary>
        /// Pause this instance. Stores velocity at pause (used in resume).
        /// </summary>
        public void Pause()
        {
            m_StoredVel = m_Rigidbody2D.velocity;
            m_Rigidbody2D.velocity = Vector2.zero;
            m_Paused = true;
        }

        /// <summary>
        /// Resume this instance. Restores velocity saved when paused.
        /// </summary>
        public void Resume()
        {
            m_Rigidbody2D.velocity = m_StoredVel;
            m_Paused = false;
        }

        protected virtual void Update()
        {
            if (m_Paused) { return; }

            WarpGrid.WarpingGrid.Instance.ApplyDirectedForce(m_Rigidbody2D.velocity * 0.002f, transform.position, .5f);

            if (m_Effectors != null && m_Effectors.Count > 0)
            {
                Vector2 force = Vector2.zero;

                for (int i = 0; i < m_Effectors.Count; i++)
                {
                    var effector = m_Effectors[i];

                    if (effector == null)
                    {
                        m_Effectors.RemoveAt(i);
                        continue;
                    }

                    if (!effector.enabled || !effector.gameObject.activeSelf
                        || effector.force == 0f || !effector.effectProjectiles)
                    {
                        continue;
                    }

                    var heading = effector.transform.position - transform.position;

                    if (heading.sqrMagnitude > effector.distance * effector.distance)
                    {
                        continue;
                    }

                    if (effector.effectorType == EffectorType.Attraction)
                    {
                        float distance = heading.magnitude;
                        var n = heading / distance;

                        force += (Vector2)(EFFCTOR_OFFSET * n / (distance * distance + EFFCTOR_OFFSET)) * effector.force;
                    }
                    else if (effector.effectorType == EffectorType.Repel)
                    {
                        float distance = heading.magnitude;
                        var n = heading / distance;

                        force -= (Vector2)(EFFCTOR_OFFSET * n / (distance * distance + EFFCTOR_OFFSET)) * (effector.force * 8f);

                    }
                    else if (effector.effectorType == EffectorType.BlackHole)
                    {
                        float distance = heading.magnitude;
                        var n = heading / distance;

                        force += (Vector2)(EFFCTOR_OFFSET * n / (distance * distance + EFFCTOR_OFFSET)) * (effector.force * 2f);

                        if (distance < effector.rotateDistance)
                        {
                            force += 45 * new Vector2(n.y, -n.x) / (distance + 100) * effector.force;
                        }
                    }
                }

                if (force != Vector2.zero)
                {
                    force *= 30f * Time.deltaTime;
                    m_Rigidbody2D.AddForce(force * effectorMultiplier);

                    float angle = Mathf.Atan2(m_Rigidbody2D.velocity.y, m_Rigidbody2D.velocity.x) * Mathf.Rad2Deg - 90;
                    transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                }
            }

            m_CurrentTimeAlive += Time.deltaTime;

            if (m_CurrentTimeAlive >= timeAlive)
            {
                ReturnProjectile();
            }
        }

        private Vector2 ScaleTo(Vector2 vector, float length)
        {
            return vector * (length / vector.magnitude);
        }
    }
}