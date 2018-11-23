using UnityEngine;
using System.Collections;

namespace GameCore
{
    /// <summary>
    /// Attached to each points text. Handles text movement and fade out.
    /// </summary>
    [RequireComponent(typeof(TextMesh))]
    public class PointsText : MonoBehaviour
    {
        /// <summary>
        /// The upwards movement speed.
        /// </summary>
        public float moveSpeed = 1f;

        private TextMesh m_TextMesh;
        private Color m_InitialColour;

        void Awake()
        {
            m_TextMesh = GetComponent<TextMesh>();
            m_InitialColour = m_TextMesh.color;
        }

        /// <summary>
        /// Show the point text at the specified position.
        /// </summary>
        /// <param name="position">Position to show points text.</param>
        public void Show(Vector3 position)
        {
            transform.position = position;
            m_TextMesh.color = m_InitialColour;
            gameObject.SetActive(true);
            StartCoroutine(FadeOut());
        }

        /// <summary>
        /// Sets text to string value of score.
        /// </summary>
        /// <param name="score">Score to display.</param>
        public void SetScore(int score)
        {
            m_TextMesh.text = score.ToString();
        }

        /// <summary>
        /// Sets the text.
        /// </summary>
        /// <param name="text">Text to display.</param>
        public void SetText(object text)
        {
            m_TextMesh.text = text.ToString();
        }

        void Update()
        {
            transform.position += Vector3.up * moveSpeed * Time.deltaTime;
        }

        private IEnumerator FadeOut()
        {
            while (m_TextMesh.color.a > 0.1f)
            {
                m_TextMesh.color = new Color(m_InitialColour.r, m_InitialColour.g, m_InitialColour.b, m_TextMesh.color.a - 0.2f);
                yield return new WaitForSeconds(.1f);
            }
        }
    }
}