using UnityEngine;
using System.Collections;

namespace GameCore
{
    /// <summary>
    /// Lerps a sprites alpha from 0 to 1 over a set time defined by GameManager::ROUND_BEGIN_TIME.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
    public class SpriteFadeIn : MonoBehaviour
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="SpriteFadeIn"/> has finished lerping alpha.
        /// </summary>
        /// <value><c>true</c> if finished; otherwise, <c>false</c>.</value>
        public bool finished { get; private set; }

        private float m_StartTime;
        private SpriteRenderer m_SpriteRenderer;
        private Collider2D m_Collider2D;

        void Awake()
        {
            m_SpriteRenderer = GetComponent<SpriteRenderer>();
            m_Collider2D = GetComponent<Collider2D>();
        }

        void Start()
        {
            m_Collider2D.enabled = false;
            m_SpriteRenderer.color = m_SpriteRenderer.color.WithAlpha(0f);
        }

        /// <summary>
        /// Starts the fade in.
        /// </summary>
        /// <param name="maxAlpha">The maximum alpha.</param>
        public void StartFadeIn(float maxAlpha = 1f)
        {
            finished = false;
            m_StartTime = Time.time;
            StartCoroutine(FadeIn(maxAlpha));
        }

        private IEnumerator FadeIn(float maxAlpha)
        {
            float t = 0f;

            while (t < 1f)
            {
                t = (Time.time - m_StartTime) / GameManager.ROUND_BEGIN_TIME;
                m_SpriteRenderer.color = m_SpriteRenderer.color.WithAlpha(Mathf.SmoothStep(0f, maxAlpha, t));
                yield return new WaitForEndOfFrame();
            }

            m_Collider2D.enabled = true;

            finished = true;
        }
    }
}