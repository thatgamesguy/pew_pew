using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Attach to an audio source. Used to play instances of AudioClip. 
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioPlayer : MonoBehaviour
    {
        private static BGMAudioPlayer m_AudioStatus;

        private AudioSource m_Audio;

        void Awake()
        {
            m_Audio = GetComponent<AudioSource>();

            if (m_AudioStatus == null)
            {
                m_AudioStatus = FindObjectOfType<BGMAudioPlayer>();
            }
        }

        /// <summary>
        /// Play AudioClip if not muted.
        /// </summary>
        /// <param name="clip">AudioClip to play</param>
        public void PlayInstance(AudioClip clip)
        {
            if (!m_AudioStatus.muted)
            {
                m_Audio.PlayOneShot(clip);
            }
        }
    }
}

