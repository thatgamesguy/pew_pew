using UnityEngine;
using System.Collections;

namespace WarpGrid
{
    /// <summary>
    /// Connects two PointMass on a grid. 
    /// </summary>
    public struct Spring
    {
        /// <summary>
        /// PointMass 1.
        /// </summary>
        public PointMass End1;

        /// <summary>
        /// PointMass 2.
        /// </summary>
        public PointMass End2;

        /// <summary>
        /// The points will move to be within this range of each other.
        /// </summary>
        public float TargetLength;

        /// <summary>
        /// Signifies how easy it is for the springs to be pulled apart.
        /// </summary>
        public float Stiffness;

        /// <summary>
        /// Provides a dampening effect on the movement of the connected point masses.
        /// </summary>
        public float Damping;

        /// <summary>
        /// Initializes a new instance of the <see cref="Spring"/> struct.
        /// </summary>
        /// <param name="end1">First point.</param>
        /// <param name="end2">Second point.</param>
        /// <param name="stiffness">Stiffness.</param>
        /// <param name="damping">Damping.</param>
        public Spring(PointMass end1, PointMass end2,
            float stiffness, float damping)
        {
            End1 = end1;
            End2 = end2;
            Stiffness = stiffness;
            Damping = damping;
            TargetLength = Vector3.Distance(end1.Position, end2.Position) * 0.95f;
        }

        /// <summary>
        /// Applies a pulling force to each attached point.
        /// </summary>
        public void Update()
        {
            if (End1.Velocity != Vector2.zero || End2.Velocity != Vector2.zero)
            {
                var x = End1.Position - End2.Position;

                float length = x.magnitude;

                if (length <= TargetLength)
                {
                    return;
                }

                x = (x / length) * (length - TargetLength);
                var dv = End2.Velocity - End1.Velocity;
                var force = Stiffness * x - dv * Damping;

                End1.ApplyForce(-force);
                End2.ApplyForce(force);
            }
        }
    }
}