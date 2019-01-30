using UnityEngine;
using System.Collections;

namespace GameCore
{
    /// <summary>
    /// Triggers individual shop button animations.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class ButtonAnimator : MonoBehaviour
    {
        private static readonly string ANIMATE_TRIGGER = "Animate";
        private static readonly string RESET_TRIGGER = "Reset";

        private Animator m_Animator;

        void Awake()
        {
            m_Animator = GetComponent<Animator>();
        }

        /// <summary>
        /// Animate this instance.
        /// </summary>
        public void Animate()
        {
            if (m_Animator)
            {
                m_Animator.SetTrigger(ANIMATE_TRIGGER);
            }
        }

        /// <summary>
        /// Resets button to initial position if reset equals true else disables reset.
        /// </summary>
        /// <param name="reset">Set wether to reset animation</param>
        public void Reset()
        {
            if(m_Animator)
            {
                m_Animator.SetTrigger(RESET_TRIGGER);
            }
        }
    }
}