using UnityEngine;
using System.Collections;

namespace GameCore
{
    /// <summary>
    /// Lerps between min and max scale over time.
    /// </summary>
    public class ScaleOscillation : MonoBehaviour
    {
        /// <summary>
        /// The minimum scale.
        /// </summary>
        public float minScale = 0.7f;

        /// <summary>
        /// The maximum scale.
        /// </summary>
        public float maxScale = 1.3f;

        /// <summary>
        /// The scale speed.
        /// </summary>
        public float scaleSpeed = 5f;

        /// <summary>
        /// The amount to decrease scale on hit.
        /// </summary>
        public float scaleDecreaseOnHit;

        private bool m_ScalingUp = false;
        private Blackhole m_Health;

        void Awake()
        {
            m_Health = GetComponent<Blackhole>();
        }

        void OnEnable()
        {
            m_Health.onHit += OnHit;
        }

        void OnDisable()
        {
            m_Health.onHit -= OnHit;
        }

        void Update()
        {
            Vector2 curSc = transform.localScale;

            if (m_ScalingUp)
            {
                curSc += Vector2.one * scaleSpeed * Time.deltaTime;

                if (curSc.x > maxScale)
                {
                    curSc = new Vector2(maxScale, maxScale);
                    m_ScalingUp = false;
                }
            }
            else
            {
                curSc -= Vector2.one * scaleSpeed * Time.deltaTime;

                if (curSc.x < minScale)
                {
                    curSc = new Vector2(minScale, minScale);
                    m_ScalingUp = true;
                }
            }

            transform.localScale = curSc;
        }

        private void OnHit()
        {
            minScale -= scaleDecreaseOnHit;
            maxScale -= scaleDecreaseOnHit;

            m_ScalingUp = false;
        }
    }
}