using UnityEngine;
using System.Collections;

namespace GameCore
{
    /// <summary>
    /// Rotates GameObject on z axis.
    /// </summary>
    public class Rotate : MonoBehaviour
    {
        /// <summary>
        /// The target revolution in degrees per second (e.g. 360 = 1 full rotation per second).
        /// </summary>
        public float rotateSpeed = 80f;

        /// <summary>
        /// The object has a 50% chance to rotate either left or right.
        /// </summary>
        public bool randomSign = false;

        /// <summary>
        /// The rotation will not begin until Activate is called.
        /// </summary>
        public bool waitToActivate = false;

        private Vector3 rotationEuler;
        private bool m_ShouldUpdate = true;

        void Start()
        {
            if (randomSign)
            {
                if (Random.value >= 0.5f)
                {
                    rotateSpeed *= -1f;
                }
            }

            m_ShouldUpdate = !waitToActivate;
        }


        /// <summary>
        /// Begins rotation. If waitToActivate is false, rotation is started in Start method.
        /// </summary>
        public void Activate()
        {
            m_ShouldUpdate = true;
        }

        void Update()
        {
            if (m_ShouldUpdate)
            {
                rotationEuler -= Vector3.forward * rotateSpeed * Time.deltaTime;
                transform.rotation = Quaternion.Euler(rotationEuler);
            }
        }
    }
}