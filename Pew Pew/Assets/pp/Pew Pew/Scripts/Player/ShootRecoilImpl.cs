using UnityEngine;
using System.Collections;

namespace GameCore
{
    /// <summary>
    /// Contract for any GameObject that can provide recoil.
    /// </summary>
    public interface ShootRecoil
    {
        /// <summary>
        /// Execute recoil.
        /// </summary>
        void Execute();
    }

    /// <summary>
    /// Provides weapon recoil functionality. Where the gun is temporarily moved back by the force of a shot.
    /// </summary>
    public class ShootRecoilImpl : MonoBehaviour, ShootRecoil
    {
        /// <summary>
        /// The weapon.
        /// </summary>
        public Transform weapon;

        /// <summary>
        /// Displacement force.
        /// </summary>
        public float Recoil = 1f;

        /// <summary>
        /// The speed at which the weapon returns to its original position.
        /// </summary>
        public float returnSpeed = 1f;

        private float m_CurrentRecoil;
        private Vector3 m_InitialPos;

        void Awake()
        {
            m_InitialPos = weapon.position;
        }

        /// <summary>
        /// Execute recoil. 
        /// </summary>
        public void Execute()
        {
            m_CurrentRecoil = Recoil;
        }

        void Update()
        {
            if (m_CurrentRecoil > 0f)
            {
                weapon.position = new Vector2(transform.position.x, m_InitialPos.y - m_CurrentRecoil);
                m_CurrentRecoil -= Time.deltaTime * returnSpeed;

                if (m_CurrentRecoil < 0f)
                {
                    m_CurrentRecoil = 0f;
                }
            }
            else
            {
                transform.position = new Vector2(transform.position.x, m_InitialPos.y);
            }
        }
    }
}