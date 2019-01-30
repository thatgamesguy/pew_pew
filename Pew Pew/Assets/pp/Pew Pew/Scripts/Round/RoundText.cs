using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace GameCore
{
    /// <summary>
    /// Updates onscreen text to signify a round start or end.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class RoundText : MonoBehaviour
    {
        /// <summary>
        /// The text background. Phased in and out with text.
        /// </summary>
        public Image background;

        private static readonly string ROUND_PRE_TEXT = "WAVE ";
        private static readonly string ROUND_COMPLETE_TEXT = "WAVE COMPLETE";
        private static readonly string GAME_OVER_TEXT = "GAME OVER";
        private static readonly string CHALLENGE_WAVE_START_TEXT = "CHALLENGE WAVE";
        private static readonly string CHALLENGE_WAVE_COMPLETE_TEXT = "WAVE COMPLETE";
        private static readonly string BOSS_WAVE_START_TEXT = "BOSS WAVE";
        private static readonly string BOSS_WAVE_COMPLETE_TEXT = "BOSS WAVE\nCOMPLETE";
        private static readonly string ALL_ROUNDS_COMPLETE_TEXT = "ROUNDS COMPLETE!";

        private Text m_Text;
        private bool m_ChallengeScoreProcessed = true;
        private float m_BackgroundStartAlpha;

        void Awake()
        {
            m_Text = GetComponent<Text>();
            m_Text.color = m_Text.color.WithAlpha(0f);

            m_BackgroundStartAlpha = background.color.a;
            background.color = background.color.WithAlpha(0f);
        }

        /// <summary>
        /// Sets the round number. Does not show text if not already onscreen.
        /// </summary>
        /// <param name="roundNumber">Round number.</param>
        public void SetRoundNumber(int roundNumber)
        {
            m_Text.text = ROUND_PRE_TEXT + roundNumber.ToString();
        }

        /// <summary>
        /// Sets text to RoundText::
        /// </summary>
        public void SetWaveCompleteText()
        {
            m_Text.text = ROUND_COMPLETE_TEXT;
        }

        /// <summary>
        /// Sets text to RoundText::CHALLENGE_WAVE_START_TEXT
        /// </summary>
        public void SetChallengeWaveStartText()
        {
            m_Text.text = CHALLENGE_WAVE_START_TEXT;
        }

        /// <summary>
        /// Sets text to RoundText::BOSS_WAVE_START_TEXT
        /// </summary>
        public void SetBossWaveStartText()
        {
            m_Text.text = BOSS_WAVE_START_TEXT;
        }

        /// <summary>
        /// Sets text to RoundText::BOSS_WAVE_COMPLETE_TEXT
        /// </summary>
        public void SetBossCompleteText()
        {
            m_Text.text = BOSS_WAVE_COMPLETE_TEXT;
        }

        /// <summary>
        /// Sets text to RoundText::CHALLENGE_WAVE_COMPLETE_TEXT
        /// </summary>
        public void SetChallengeWaveCompleteText()
        {
            m_Text.text = CHALLENGE_WAVE_COMPLETE_TEXT;
        }

        /// <summary>
        /// Sets text to RoundText::ALL_ROUNDS_COMPLETE_TEXT
        /// </summary>
        public void SetRoundsCompleteText()
        {
            m_Text.text = ALL_ROUNDS_COMPLETE_TEXT;
        }

        /// <summary>
        /// Sets text to RoundText::GAME_OVER_TEXT
        /// </summary>
        public void SetGameOver()
        {
            m_Text.text = GAME_OVER_TEXT;
        }

        /// <summary>
        /// Calculates and shows the total challenge enemies killed as a percentage.
        /// </summary>
        /// <param name="enemiesKilled">Enemies killed.</param>
        /// <param name="maxEnemies">Max enemies.</param>
        public void CalculatePercentage(float enemiesKilled, float maxEnemies)
        {
            m_ChallengeScoreProcessed = false;
            StartCoroutine(_CalculatePercentage(enemiesKilled, maxEnemies));
        }

        /// <summary>
        /// Shows text onscreen for a number of seconds.
        /// </summary>
        /// <param name="showSeconds">Number of seconds to show text.</param>
        /// <param name="fadeOutSeconds">Time to fade out text.</param>
        /// <param name="callbackOnFadeOut">Callback on fade out.</param>
        public void ShowForSeconds(float showSeconds,
            float fadeOutSeconds, Action callbackOnFadeOut = null)
        {
            StartCoroutine(_ShowForSeconds(showSeconds, fadeOutSeconds, callbackOnFadeOut));
        }

        /// <summary>
        /// Waits for challenge percentage calculation to finish.
        /// </summary>
        /// <param name="fadeOutTimeSeconds">Fade out time seconds.</param>
        /// <param name="onComplete">On complete.</param>
        public void WaitForChallengePercentageToBeCalculated(float fadeOutTimeSeconds,
            Action onComplete)
        {
            StartCoroutine(_WaitForChallengePercentageToBeCalculated(fadeOutTimeSeconds, onComplete));
        }

        private IEnumerator _WaitForChallengePercentageToBeCalculated(float fadeOutTimeSeconds,
            Action onComplete)
        {
            background.CrossFadeAlpha(m_BackgroundStartAlpha, .5f, false);
            m_Text.CrossFadeAlpha(1f, .5f, false);
            m_Text.color = m_Text.color.WithAlpha(1f);
            background.color = background.color.WithAlpha(m_BackgroundStartAlpha);

            while (!m_ChallengeScoreProcessed)
            {
                yield return new WaitForEndOfFrame();
            }

            m_Text.CrossFadeAlpha(0.0f, fadeOutTimeSeconds, false);
            background.CrossFadeAlpha(0f, fadeOutTimeSeconds, false);

            yield return new WaitForSeconds(1f);

            if (onComplete != null)
            {
                onComplete();
            }
        }

        private IEnumerator _CalculatePercentage(float enemiesKilled, float maxEnemies)
        {
            float percent = 0f;
            float target = (enemiesKilled / maxEnemies) * 100f;

            if (target > 0f)
            {
                do
                {
                    percent += Time.deltaTime * 50f;

                    if (percent > target)
                    {
                        percent = target;
                    }

                    m_Text.text = enemiesKilled + "/" + maxEnemies + "="
                        + percent.ToString("0.00") + "%";

                    yield return new WaitForEndOfFrame();
                }
                while (percent < target);

                yield return new WaitForSeconds(1f);
            }
            else
            {
                m_Text.text = enemiesKilled + "/" + maxEnemies + "= 0.00%";

                yield return new WaitForSeconds(2f);
            }

            m_ChallengeScoreProcessed = true;
        }

        private IEnumerator _ShowForSeconds(float showSeconds,
            float fadeOutSeconds, Action callbackOnFadeOut)
        {
            background.CrossFadeAlpha(m_BackgroundStartAlpha, .5f, false);
            m_Text.CrossFadeAlpha(1f, .5f, false);

            m_Text.color = m_Text.color.WithAlpha(1f);
            background.color = background.color.WithAlpha(m_BackgroundStartAlpha);

            yield return new WaitForSeconds(showSeconds + .5f);

            if (callbackOnFadeOut != null)
            {
                callbackOnFadeOut();
            }

            m_Text.CrossFadeAlpha(0.0f, fadeOutSeconds, false);

            background.CrossFadeAlpha(0.0f, fadeOutSeconds, false);
        }
    }
}