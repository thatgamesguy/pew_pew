using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace GameCore
{
    /// <summary>
    /// Shows pause screen and hanles UI events from that scene.
    /// </summary>
    public class PauseHandler : MonoBehaviour
    {
        /// <summary>
        /// The text used to display the current round.
        /// </summary>
        public Text currentRoundText;

        /// <summary>
        /// The text used to display the highest round.
        /// </summary>
        public Text highestRoundText;

        /// <summary>
        /// The texts used to display the plauers current point total.
        /// </summary>
        public Text pointsText;

        /// <summary>
        /// The score handler, used to obtain the current point total.
        /// </summary>
        public Score scoreHandler;

        /// <summary>
        /// The pause button.
        /// </summary>
        public Button pauseButton;

        /// <summary>
        /// The objects to hide before showing scene.
        /// </summary>
        public GameObject[] objectsToHide;

        /// <summary>
        /// The parent GameObject of all pause menu UI items.
        /// </summary>
        public GameObject pauseMenu;

        /// <summary>
        /// The current pause status.
        /// </summary>
        public bool isPaused;

        private static readonly float VOLUME_LERP_SECONDS = 0.5f;

        private static GameManager GAME_MANAGER;
        private static RoundPersistentScore HIGH_SCORE;
        private static BGMAudioPlayer BGM_AUDIO;

        void Awake()
        {
            if (GAME_MANAGER == null)
            {
                GAME_MANAGER = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
            }
        }

        void Start()
        {
            if (HIGH_SCORE == null)
            {
                HIGH_SCORE = GameObject.FindGameObjectWithTag("RoundPersistent").GetComponent<RoundPersistentScore>();
            }

            if (BGM_AUDIO == null)
            {
                BGM_AUDIO = GameObject.FindObjectOfType<BGMAudioPlayer>();
            }

            pauseMenu.SetActive(false);

            isPaused = false;


            DisableButton();
        }

        /// <summary>
        /// Enables pause button.
        /// </summary>
        /// <param name="seconds">The number of seconds before the pause button is enabled..</param>
        public void EnableButton(float seconds)
        {
            if (seconds > 0f)
            {
                StartCoroutine(_EnableButton(seconds));
            }
            else
            {
                pauseButton.interactable = true;
            }
        }

        /// <summary>
        /// Disables the pause button.
        /// </summary>
        public void DisableButton()
        {
            pauseButton.interactable = false;
        }

        /// <summary>
        /// Button event. Pauses the game. Pauses round entities and updates UI.
        /// </summary>
        public void Pause()
        {
            GAME_MANAGER.PauseCurrentRoundEntities();

            BGM_AUDIO.SetVolume(0.3f, VOLUME_LERP_SECONDS);

            currentRoundText.text = (GAME_MANAGER.currentRoundIndex + 1).ToString();
            highestRoundText.text = HIGH_SCORE.highestRound.ToString();
            pointsText.text = scoreHandler.score.ToString();

            pauseMenu.SetActive(true);

            foreach (var obj in objectsToHide)
            {
                obj.SetActive(false);
            }

            isPaused = true;
        }

        /// <summary>
        /// Button event. Reloads game scene.
        /// </summary>
        public void Restart()
        {
            SceneManager.LoadScene(1);
        }

        /// <summary>
        /// Button event. Resumes game.
        /// </summary>
        public void Resume()
        {
            GAME_MANAGER.ResumeCurrentRoundEntities();

            BGM_AUDIO.SetVolume(1f, VOLUME_LERP_SECONDS);

            pauseMenu.SetActive(false);

            foreach (var obj in objectsToHide)
            {
                obj.SetActive(true);
            }

            isPaused = false;
        }

        private IEnumerator _EnableButton(float seconds)
        {
            yield return new WaitForSeconds(seconds);

            pauseButton.interactable = true;
        }
    }
}