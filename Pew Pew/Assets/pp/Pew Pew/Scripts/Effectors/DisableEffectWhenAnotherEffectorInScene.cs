using UnityEngine;
using PE2D;

namespace GameCore
{
    /// <summary>
    /// Disables this particle effector when another effector is present in scene.
    /// </summary>
    public class DisableEffectWhenAnotherEffectorInScene : MonoBehaviour
    {
        /// <summary>
        /// The particle effector to disable.
        /// </summary>
        public ParticleEffector particleEffector;

        private float m_InitialForce;
        private PlayerHealth m_Health;

        void Awake()
        {
            m_InitialForce = particleEffector.force;
            m_Health = transform.parent.GetComponent<PlayerHealth>();
        }

        void OnEnable()
        {
            m_Health.OnDeath += DisableEffector;
            m_Health.OnSpawn += EnableEffector;
        }

        void OnDisable()
        {
            m_Health.OnDeath -= DisableEffector;
            m_Health.OnSpawn -= EnableEffector;
        }

        /// <summary>
        /// Enables the effector.
        /// </summary>
        public void EnableEffector()
        {
            particleEffector.force = m_InitialForce;
        }

        /// <summary>
        /// Disables the effector.
        /// </summary>
        public void DisableEffector()
        {
            particleEffector.force = 0f;
        }

        private void DisableIfAnotherEffectorPresent()
        {
            bool shouldDisable = false;

            var effectors = GameObject.FindObjectsOfType<ParticleEffector>();

            foreach (var effector in effectors)
            {
                if (effector.gameObject.GetInstanceID() != particleEffector.gameObject.GetInstanceID())
                {
                    shouldDisable = true;
                    break;
                }
            }

            if (shouldDisable)
            {
                DisableEffector();
            }
            else
            {
                EnableEffector();
            }
        }

        private void Repel()
        {
            particleEffector.effectorType = EffectorType.Repel;
        }

        private void Attract()
        {
            particleEffector.effectorType = EffectorType.Attraction;
        }
    }
}