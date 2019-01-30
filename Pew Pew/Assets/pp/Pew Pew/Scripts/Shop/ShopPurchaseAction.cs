using UnityEngine;
using System;

namespace GameCore
{
    /// <summary>
    /// Actions any shop purchase requests.
    /// </summary>
    public class ShopPurchaseAction : MonoBehaviour
    {
        /// <summary>
        /// Invoked when a shop purchase action is performed.
        /// </summary>
        public Action OnPuchase;

        /// <summary>
        /// The audio clip to play on purchase action.
        /// </summary>
        public AudioClip audioOnPurchase;

        private AudioPlayer m_AudioSource;

        void Awake()
        {
            m_AudioSource = Camera.main.GetComponent<AudioPlayer>();
        }

        /// <summary>
        /// Performs action if actionable. 
        /// </summary>
        /// <param name="action">Action to perform.</param>
        public void DoAction(ShopPurchaseActionableImpl action)
        {
            if (action.IsActionable())
            {
                action.DoAction();

                if (OnPuchase != null)
                {
                    OnPuchase.Invoke();
                }

                m_AudioSource.PlayInstance(audioOnPurchase);
            }
        }

    }
}