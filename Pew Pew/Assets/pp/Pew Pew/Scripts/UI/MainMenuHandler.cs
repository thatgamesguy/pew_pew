using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using WarpGrid;

namespace GameCore
{
    /// <summary>
    /// Shows main menu screen and hanles UI requests from scene.
    /// </summary>
    public class MainMenuHandler : MonoBehaviour
    {
        /// <summary>
        /// The text used to diaplay the current highest round.
        /// </summary>
        public Text highscoreText;

        /// <summary>
        /// The radius of the touch effect on the grid.
        /// </summary>
        public float touchGridRadius = 2f;

        /// <summary>
        /// The force if the touch effect on the grid.
        /// </summary>
        public float touchGridForce = 4f;

        /// <summary>
        /// The audio to play on grid touch.
        /// </summary>
        public AudioClip audioOnGridTouch;

        private static RoundPersistentScore HIGH_SCORE;
        private static WarpGrid.WarpingGrid GRID;
        private static AudioPlayer AUDIO_PLAYER;

        void Awake()
        {
            if (HIGH_SCORE == null)
            {
                HIGH_SCORE = GameObject.FindGameObjectWithTag("RoundPersistent").GetComponent<RoundPersistentScore>();
            }

            if (GRID == null)
            {
                GRID = GameObject.FindObjectOfType<WarpGrid.WarpingGrid>();
            }

            if (AUDIO_PLAYER == null)
            {
                AUDIO_PLAYER = Camera.main.GetComponent<AudioPlayer>();
            }
        }

        void Start()
        {
            highscoreText.text = HIGH_SCORE.highestRound.ToString();
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 pos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                RaycastHit2D hitInfo = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(pos), Vector2.zero);

                if (hitInfo)
                {
                    var health = hitInfo.collider.gameObject.GetComponent<EnemyHealth>();

                    if (health)
                    {
                        health.Kill(false);
                    }
                }

                AUDIO_PLAYER.PlayInstance(audioOnGridTouch);
                GRID.ApplyImplosiveForce(touchGridForce, Camera.main.ScreenToWorldPoint(Input.mousePosition), touchGridRadius);
            }
        }

        /// <summary>
        /// Button event. Loads the game scene.
        /// </summary>
        public void Play()
        {
            SceneManager.LoadScene("GameScene");
        }
    }
}