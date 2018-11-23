using UnityEngine;
using System.Collections;

namespace GameCore
{
    /// <summary>
    /// Controls stationary enemies fade in and collider enabled status.
    /// </summary>
    public class StationaryMovement : MonoBehaviour, EnemyMove
    {
        private SpriteRenderer m_SpriteRenderer;
        private float m_StartTime;
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

            if (GetComponent<EnemyMoveRegister>() == null)
            {
                gameObject.AddComponent<EnemyMoveRegister>();
            }
        }

        /// <summary>
        /// Begin this instance. Starts fade in. Enables collider when fade in complete.
        /// </summary>
        public void Begin()
        {
            m_StartTime = Time.time;
            StartCoroutine(FadeIn());
        }

        /// <summary>
        /// Pause this instance.
        /// </summary>
        public void Pause()
        {
        }

        /// <summary>
        /// Resume this instance.
        /// </summary>
        public void Resume()
        {
        }

        private IEnumerator FadeIn()
        {
            float t = 0f;

            while (t < 1f)
            {
                t = (Time.time - m_StartTime) / GameManager.ROUND_BEGIN_TIME;
                m_SpriteRenderer.color = m_SpriteRenderer.color.WithAlpha(Mathf.SmoothStep(0f, 1f, t));
                yield return new WaitForEndOfFrame();
            }

            m_Collider2D.enabled = true;
        }
    }
}