using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace GameCore
{
    /// <summary>
    /// Lerps texts alpha over specified number of seconds.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class FadeOutText : MonoBehaviour
    {
        /// <summary>
        /// The seconds to wait before beginning fade out.
        /// </summary>
        public float secsToFadeOut;

        /// <summary>
        /// The time to spend fading out (seconds).
        /// </summary>
        public float fadeOutTime;

        private Text m_Text;

        void Awake()
        {
            m_Text = GetComponent<Text>();
        }

        void Start()
        {
            StartCoroutine(DoFadeOut());
        }

        private IEnumerator DoFadeOut()
        {
            yield return new WaitForSeconds(secsToFadeOut);

            m_Text.CrossFadeAlpha(0f, fadeOutTime, true);

            yield return new WaitForSeconds(fadeOutTime);

            Destroy(gameObject);
        }
    }
}