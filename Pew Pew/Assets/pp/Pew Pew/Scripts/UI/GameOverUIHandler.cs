using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace GameCore
{
    /// <summary>
    /// Shows the game over screen and handles UI requests from that screen.
    /// </summary>
    public class GameOverUIHandler : MonoBehaviour
    {
        /// <summary>
        /// The text used to show the current round number.
        /// </summary>
        public Text currentRoundText;

        /// <summary>
        /// The text used to show the highest round.
        /// </summary>
        public Text highestRoundText;

        /// <summary>
        /// Objects to hide before showing the game over screen.
        /// </summary>
        public GameObject[] objectsToHide;

        private static GameManager GAME_MANAGER;
        private static RoundPersistentScore HIGH_SCORE;
        private static BGMAudioPlayer BGM_AUDIO;

        void Awake()
        {
            if (GAME_MANAGER == null)
            {
                GAME_MANAGER = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
            }

            if (HIGH_SCORE == null)
            {
                HIGH_SCORE = GameObject.FindGameObjectWithTag("RoundPersistent").GetComponent<RoundPersistentScore>();
            }

            if (BGM_AUDIO == null)
            {
                BGM_AUDIO = GameObject.FindObjectOfType<BGMAudioPlayer>();
            }
        }

        void Start()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Show this instance. Updates currentRoundText and highestRoundText. Pauses entities in scene.
        /// </summary>
        public void Show()
        {
            GAME_MANAGER.PauseCurrentRoundEntities();
            BGM_AUDIO.PlayGameOverBGM();

            currentRoundText.text = (GAME_MANAGER.currentRoundIndex + 1).ToString();
            highestRoundText.text = HIGH_SCORE.highestRound.ToString();

            gameObject.SetActive(true);

            foreach (var obj in objectsToHide)
            {
                obj.SetActive(false);
            }
        }

        /// <summary>
        /// Button event. Reloads game scene.
        /// </summary>
        public void Restart()
        {
            SceneManager.LoadScene(1);
        }

        /// <summary>
        /// Button event. Loads main menu scene.
        /// </summary>
        public void MainMenu()
        {
            SceneManager.LoadScene(0);
        }
    }
}