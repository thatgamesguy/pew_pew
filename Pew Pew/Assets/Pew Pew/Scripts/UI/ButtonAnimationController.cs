using UnityEngine;
using System;
using System.Collections;

namespace GameCore
{
    /// <summary>
    /// Holds collections of ButtonAnimator. Responsible for playing the button aniamtions, with a small delay between each row.
    /// </summary>
    public class ButtonAnimationController : MonoBehaviour
    {
        /// <summary>
        /// The ButtonAnimator attached to the left column of shop buttons. These should be added from top to bottom i.e. the top left button is at index 0.
        /// </summary>
        public ButtonAnimator[] buttonLeftAnimators;

        /// <summary>
        /// The ButtonAnimator attached to the right column of shop buttons. These should be added from top to bottom i.e. the top right button is at index 0.
        /// </summary>
        public ButtonAnimator[] buttonRightAnimators;

        /// <summary>
        /// The delay between animating each row.
        /// </summary>
        public float delayBetweenAnimations = 0.2f;

        /// <summary>
        /// Invoked when shop has finished animating.
        /// </summary>
        public Action OnAnimationComplete;

        private bool m_FirstRun = true;

        void OnEnable()
        {
            StartCoroutine(PlayAnimations());
        }

        void OnDisable()
        {
            if(m_FirstRun)
            {
                m_FirstRun = false;
                return;
            }

            ResetAnimation();
        }

        private IEnumerator PlayAnimations()
        {
            int max = buttonLeftAnimators.Length >= buttonRightAnimators.Length ?
                buttonLeftAnimators.Length : buttonRightAnimators.Length;

            for (int i = 0; i < max; i++)
            {
                if (i < buttonLeftAnimators.Length)
                {
                    buttonLeftAnimators[i].Animate();
                }

                if (i < buttonRightAnimators.Length)
                {
                    buttonRightAnimators[i].Animate();
                }

                yield return new WaitForSeconds(delayBetweenAnimations);
            }

            yield return new WaitForSeconds(1f);

            if (OnAnimationComplete != null)
            {
                OnAnimationComplete();
            }
        }

        private void ResetAnimation()
        {
            foreach (var b in buttonLeftAnimators)
            {
                b.Reset();
            }

            foreach (var b in buttonRightAnimators)
            {
                b.Reset();
            }
        }
    }
}