using UnityEngine;
using System.Collections;

namespace WarpGrid
{
    /// <summary>
    /// A moveable point on the grid.
    /// </summary>
    public class PointMass
    {
        /// <summary>
        /// Current point position.
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// Current point velocity.
        /// </summary>
        public Vector2 Velocity;

        /// <summary>
        /// The inverse mass of the point (lower numbers result in a point with a higher mass).
        /// </summary>
        public float InverseMass;

        private Vector2 acceleration;
        private float damping = 0.98f;

        /// <summary>
        /// Initializes a new instance of the <see cref="PointMass"/> class.
        /// </summary>
        /// <param name="position">Position.</param>
        /// <param name="invMass">Inv mass.</param>
        public PointMass(Vector2 position, float invMass)
        {
            Position = position;
            InverseMass = invMass;
        }

        /// <summary>
        /// Applies a force to the point.
        /// </summary>
        /// <param name="force">Force.</param>
        public void ApplyForce(Vector2 force)
        {
            acceleration += force * InverseMass;
        }

        /// <summary>
        /// Dampens the effect of force application.
        /// </summary>
        /// <param name="factor">Factor.</param>
        public void IncreaseDamping(float factor)
        {
            damping *= factor;
        }

        /// <summary>
        /// Update this instance. Updates velocity and position of point/
        /// </summary>
        public void Update()
        {
            Velocity += acceleration;
            Position += Velocity;
            acceleration = Vector3.zero;
            if (Velocity.sqrMagnitude < 0.001f * 0.001f)
                Velocity = Vector3.zero;

            Velocity *= damping;
            damping = 0.98f;
        }
    }
}