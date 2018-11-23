using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace GameCore
{
    /// <summary>
    /// Handles players score (points) for a specific run. Has functionality to add (when points are collected) and remove (when player purchases items at the shop) points.
    /// Updates the score UI incrementally.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class Score : MonoBehaviour
    {
        /// <summary>
        /// Gets or sets the players current score.
        /// </summary>
        /// <value>The score.</value>
        public int score { get; set; }

        private Text m_Text;
        private int m_CurrentScore;
        private int m_ScoreToAdd;

        void Awake()
        {
            m_Text = GetComponent<Text>();
        }

        void Start()
        {
            m_CurrentScore = 0;
            StartCoroutine(UpdateScore());
        }

        /// <summary>
        /// Adds to players score. 
        /// </summary>
        /// <param name="score">Score.</param>
        public void AddScore(int score)
        {
            this.score += score;
            m_ScoreToAdd += score;
        }

        /// <summary>
        /// Removes from players score.
        /// </summary>
        /// <param name="score">Score.</param>
        public void RemoveScore(int score)
        {
            this.score -= score;
            m_ScoreToAdd -= score;
        }

        private void Update()
        {
            if(Input.GetKeyUp(KeyCode.L))
            {
                AddScore(500);
            }
        }

        private IEnumerator UpdateScore()
        {
            while (true)
            {

                float waitTime = 0.5f;

                if (m_ScoreToAdd != 0)
                {

                    int inc = Mathf.Abs(m_ScoreToAdd) > 50 ? 5 : 1;
                    if (m_ScoreToAdd > 0)
                    {
                        m_Text.text = (m_CurrentScore += inc).ToString("D4");
                        m_ScoreToAdd -= inc;
                    }
                    else
                    {
                        m_Text.text = (m_CurrentScore -= inc).ToString("D4");
                        m_ScoreToAdd += inc;
                    }

                    waitTime = Mathf.Abs(m_ScoreToAdd) > 50 ? 0.0001f : 0.001f;
                }

                yield return new WaitForSeconds(waitTime);
            }

        }
    }
}