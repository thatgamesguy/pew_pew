using UnityEngine;
using System.Collections;

namespace GameCore
{
    /// <summary>
    /// Shows pop up text when player collects a particle and updates players score.
    /// </summary>
    public class PointPopUpUI : MonoBehaviour
    {
        /// <summary>
        /// The points text prefab.
        /// </summary>
        public GameObject pointsTextPrefab;

        /// <summary>
        /// The audio to play on point.
        /// </summary>
        public AudioClip audioOnPoint;

        private static readonly int NUM_TO_SHOW = 15;

        private PointsText[] m_PointsPool;
        private int m_CurrentIndex = 0;
        private Score m_ScoreText;
        private AudioPlayer m_Audio;

        void Awake()
        {
            m_ScoreText = GameObject.FindGameObjectWithTag("UI").GetComponentInChildren<Score>();
            m_Audio = Camera.main.GetComponent<AudioPlayer>();
        }

        void Start()
        {
            m_PointsPool = new PointsText[NUM_TO_SHOW];

            for (int i = 0; i < NUM_TO_SHOW; i++)
            {
                m_PointsPool[i] = ((GameObject)Instantiate(pointsTextPrefab, transform.position, Quaternion.identity)).GetComponent<PointsText>();
                m_PointsPool[i].gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Shows point text at position.
        /// </summary>
        /// <param name="position">Position to show point text.</param>
        public void ShowAtPosition(Vector3 position)
        {
            if (audioOnPoint != null)
            {
                m_Audio.PlayInstance(audioOnPoint);
            }

            m_PointsPool[m_CurrentIndex].SetScore(1);
            m_PointsPool[m_CurrentIndex].Show(position);

            m_ScoreText.AddScore(1);

            m_CurrentIndex = (m_CurrentIndex + 1) % NUM_TO_SHOW;
        }

        /// <summary>
        /// Shows the specified text at position.
        /// </summary>
        /// <param name="text">Text to display.</param>
        /// <param name="position">Position to show text.</param>
        public void ShowTextAtPosition(object text, Vector2 position)
        {
            m_PointsPool[m_CurrentIndex].SetText(text);
            m_PointsPool[m_CurrentIndex].Show(position);

            m_CurrentIndex = (m_CurrentIndex + 1) % NUM_TO_SHOW;
        }
    }
}