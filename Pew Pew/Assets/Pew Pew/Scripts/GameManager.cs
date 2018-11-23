using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using PE2D;

namespace GameCore
{
    /// <summary>
    /// Contract for any class that can perform actions when a round or challenge round finishes.
    /// </summary>
    public interface RoundManagement
    {
        void OnChallengeRoundOver(int enemiesKilled, int maxEnemies);
        void OnRoundOver();
    }

    /// <summary>
    /// Controls game flow. Starts game, initialises new rounds. Maintains list of entities within rounds to pause and resume movement.
    /// </summary>
    public class GameManager : MonoBehaviour, RoundManagement
    {
        /// <summary>
        /// Collection of EnemyMove in current round. Used to pause and resume enemy movement.
        /// </summary>
        public static List<EnemyMove> EnemyMoves = new List<EnemyMove>();

        /// <summary>
        /// Collection of EnemyShoots in current round. Used to pause and resume enemy shooting.
        /// </summary>
        public static List<EnemyShoot> EnemyShoots = new List<EnemyShoot>();

        /// <summary>
        /// The time after the shop is closed and a new round begins.
        /// </summary>
        public static readonly float ROUND_BEGIN_TIME = 1f;

        /// <summary>
        /// Invoked when a round starts.
        /// </summary>
        public Action onRoundStart;

        /// <summary>
        /// Invoked when a round finishes. Before shop is opened.
        /// </summary>
        public Action onRoundOver;

        /// <summary>
        /// Invoked when player has lost all lives.
        /// </summary>
        public Action onPlayerDeathGameOver;

        /// <summary>
        /// Invoked when player has died.
        /// </summary>
        public Action onPlayerDeath;

        /// <summary>
        /// Disabled for release. Outputs current round index to screen for debug purposes.
        /// </summary>
        //public Text debugText;

        /// <summary>
        /// The parent object for the shop UI.
        /// </summary>
        public GameObject shop;

        /// <summary>
        /// The parent object for the player/
        /// </summary>
        public GameObject player;

        /// <summary>
        /// Collection of rounds. The rounds are played sequentially.
        /// </summary>
        public GameObject[] roundPrefabs;

        /// <summary>
        /// When player dies, all enemies below this y position are killed. This prevents an enemy from killing the player as soon as they respawn.
        /// </summary>
        public float minimumYToKillEnemyOnPlayerDeath = 2f;

        /// <summary>
        /// Gets a value indicating whether this instance is playing.
        /// </summary>
        /// <value><c>true</c> if this instance is playing; otherwise, <c>false</c>.</value>
        public bool IsPlaying { get; private set; }

        /// <summary>
        /// Gets the current round.
        /// </summary>
        /// <value>The current round.</value>
        public Round currentRound { get { return m_CurrentRound; } }

        /// <summary>
        /// Gets the index of the current round.
        /// </summary>
        /// <value>The index of the current round.</value>
        public int currentRoundIndex { get { return m_RoundIndex; } }

        /// <summary>
        /// The class responsible for showing and updating the game over screen.
        /// </summary>
        public GameOverUIHandler gameOverHandler;

        /// <summary>
        /// The audio clip to play on wave complete.
        /// </summary>
        public AudioClip audioOnWaveComplete;

        /// <summary>
        /// The class responsible for showing and updating the pause screne.
        /// </summary>
        public PauseHandler pause;

        private RoundText m_RoundText;
        private CameraShake m_CameraShake;
        private int m_RoundIndex;
        private PlayerShootController m_PlayerShootController;
        private PlayerController m_PlayerController;
        private RoundPersistentScore m_RoundStorage;
        private ShopController m_ShopController;
        private float m_ChallengeEnemiesKilled;
        private float m_ChallengeEnemiesMax;
        private Round m_CurrentRound;
        private BGMAudioPlayer m_BGMAudioPlayer;
        private AudioPlayer m_AudioPlayer;

        void Awake()
        {
            m_RoundText = GameObject.FindGameObjectWithTag("UI").GetComponentInChildren<RoundText>();
            m_ShopController = shop.GetComponent<ShopController>();
            m_CameraShake = Camera.main.GetComponent<CameraShake>();
            m_PlayerShootController = player.GetComponent<PlayerShootController>();
            m_PlayerController = player.GetComponent<PlayerController>();

            var storage = GameObject.FindGameObjectWithTag("RoundPersistent");

            if (storage == null)
            {
                storage = new GameObject("Round_Persistent")
                {
                    tag = "RoundPersistent"
                };
                m_RoundStorage = storage.AddComponent<RoundPersistentScore>();
            }
            else
            {
                m_RoundStorage = storage.GetComponent<RoundPersistentScore>();
            }

            m_BGMAudioPlayer = FindObjectOfType<BGMAudioPlayer>();

            if (m_BGMAudioPlayer == null)
            {
                var audioObj = new GameObject("BGM");
                audioObj.AddComponent<AudioSource>();
                m_BGMAudioPlayer = audioObj.AddComponent<BGMAudioPlayer>();
            }

            m_AudioPlayer = Camera.main.GetComponent<AudioPlayer>();
        }

        void Start()
        {
            if (roundPrefabs.Length > 0)
            {
                m_RoundIndex = 0;
                StartNextRound();
            }
        }

        /// <summary>
        /// Called when player dies. Finds all enemies below a certain y value (set by minimumYToKillEnemyOnPlayerDeath) and destroys them.
        /// The remaining enmies movement and shooting are paused.
        /// </summary>
        public void OnPlayerDied()
        {
            m_CameraShake.Begin(0.3f, .5f);

            if (m_CurrentRound.roundType == Round.RoundType.Wave)
            {
                var allEnemies = GameObject.FindObjectsOfType<EnemyHealth>();

                if (allEnemies.Length > 0)
                {
                    const bool cameraShake = false;

                    foreach (var enemy in allEnemies)
                    {

                        if (enemy.transform.position.y < minimumYToKillEnemyOnPlayerDeath)
                        {

                            var move = enemy.GetComponent<EnemyMove>();

                            if (move != null && !(move is StationaryMovement))
                            {
                                var viewPos = Camera.main.WorldToViewportPoint(enemy.transform.position);

                                if (viewPos.x >= -0.1f && viewPos.y <= 1.1f)
                                {
                                    enemy.Kill(cameraShake);
                                }
                            }
                        }
                    }
                }
            }

            foreach (var e in EnemyMoves)
            {
                e.Pause();
            }

            foreach (var enemy in EnemyShoots)
            {
                enemy.Pause();
            }

            var aliveParticles = ParticleFactory.instance.GetLiveParticles();
            foreach (var p in aliveParticles)
            {
                p.canBeCollectedByPlayer = false;
                p.ignoreEffectors = true;
            }

        }

        /// <summary>
        /// Pauses all current round enemies moving and shooting.
        /// </summary>
        public void PauseCurrentRoundEntities()
        {
            foreach (var e in EnemyMoves)
            {
                e.Pause();
            }

            foreach (var enemy in EnemyShoots)
            {
                enemy.Pause();
            }

            m_PlayerShootController.PauseAll();
            m_PlayerController.PauseMovement();

            var aliveParticles = ParticleFactory.instance.GetLiveParticles();
            foreach (var p in aliveParticles)
            {
                p.Pause();
            }

            var projeciles = GameObject.FindObjectsOfType<Projectile>();
            foreach (var p in projeciles)
            {
                p.Pause();
            }

            var bombs = GameObject.FindObjectsOfType<Bomb>();
            foreach (var b in bombs)
            {
                b.Pause();
            }
        }

        /// <summary>
        /// Resumes current round enemies moving and shooting.
        /// </summary>
        public void ResumeCurrentRoundEntities()
        {
            foreach (var e in EnemyMoves)
            {
                e.Resume();
            }

            foreach (var enemy in EnemyShoots)
            {
                enemy.Resume();
            }

            m_PlayerShootController.ResumeAll();
            m_PlayerController.ResumeMovement();

            var aliveParticles = ParticleFactory.instance.GetLiveParticles();
            foreach (var p in aliveParticles)
            {
                p.Resume();
            }

            var projeciles = GameObject.FindObjectsOfType<Projectile>();
            foreach (var p in projeciles)
            {
                p.Resume();
            }

            var bombs = GameObject.FindObjectsOfType<Bomb>();
            foreach (var b in bombs)
            {
                b.Resume();
            }
        }

        /// <summary>
        /// Called when player respawns. Resumes enemy movement and shooting and updates effectors status.
        /// </summary>
        public void OnPlayerRespawned()
        {
            foreach (var e in EnemyMoves)
            {
                e.Resume();
            }

            foreach (var enemy in EnemyShoots)
            {
                enemy.Resume();
            }

            var aliveParticles = ParticleFactory.instance.GetLiveParticles();
            foreach (var p in aliveParticles)
            {
                p.canBeCollectedByPlayer = true;
                p.ignoreEffectors = false;
            }
        }

        /// <summary>
        /// Called when player has died and has no lives remaining. Shows game over text and shpws game over screen.
        /// </summary>
        public void OnPlayerDeathGameOver()
        {
            m_CameraShake.Begin(0.3f, .5f);

            m_RoundText.SetGameOver();
            m_RoundText.ShowForSeconds(1f, 0.2f, _ShowGameOverMenu);

            var powerups = GameObject.FindObjectsOfType<PowerUpImpl>();
            foreach (var p in powerups)
            {
                p.gameObject.SetActive(false);
            }

        
        }

        /// <summary>
        /// Change this method to change what happens when player completes the game. Currently the game over screen is displayed.
        /// </summary>
        public void OnRoundsComplete()
        {
            m_RoundText.SetRoundsCompleteText();
            m_RoundText.ShowForSeconds(3f, 0.2f, _ShowGameOverMenu);
        }


        /// <summary>
        /// Opens shop when all particles have left scene.
        /// </summary>
        public void OnRoundOver()
        {
            pause.DisableButton();

            m_BGMAudioPlayer.SetPitch(1f, 1f);
            m_BGMAudioPlayer.SetVolume(0.2f, 1f);

            IsPlaying = false;
            m_PlayerShootController.PauseAll();

            m_AudioPlayer.PlayInstance(audioOnWaveComplete);
            m_RoundText.SetWaveCompleteText();
            m_RoundText.ShowForSeconds(2f, 1f, _WaitForParticles);
        }

        /// <summary>
        /// Shows boss wave complete text and opens show when particles have left scene.
        /// </summary>
        public void OnBossRoundOver()
        {
            IsPlaying = false;
            m_PlayerShootController.PauseAll();

            m_RoundText.SetBossCompleteText();
            m_RoundText.ShowForSeconds(2f, 1f, _WaitForParticles);
        }

        /// <summary>
        /// Shows challenge wave complete percentage calculation and opens shop when particles have left scene.
        /// </summary>
        /// <param name="enemiesKilled">Enemies killed.</param>
        /// <param name="enemiesInRound">Enemies in round.</param>
        public void OnChallengeRoundOver(int enemiesKilled, int enemiesInRound)
        {
            pause.DisableButton();

            m_ChallengeEnemiesKilled = enemiesKilled;
            m_ChallengeEnemiesMax = enemiesInRound;

            IsPlaying = false;
            m_PlayerShootController.PauseAll();

            m_RoundText.SetChallengeWaveCompleteText();
            m_RoundText.ShowForSeconds(2f, 1f, _WaitForParticlesChallengeWave);
        }


        private void _WaitForParticlesChallengeWave()
        {
            StartCoroutine(WaitForParticlesChallengeWave());
        }

        private IEnumerator WaitForParticlesChallengeWave()
        {
            while (ParticleFactory.instance.ParticlesRemaining())
            {
                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForSeconds(1f);


            // Show enemies killed
            m_RoundText.CalculatePercentage(m_ChallengeEnemiesKilled, m_ChallengeEnemiesMax);
            m_RoundText.WaitForChallengePercentageToBeCalculated(1f,
                SpawnChallengeBonus);
        }

        private void SpawnChallengeBonus()
        {
            //spawn bonus here
            float bonusParticles = (m_ChallengeEnemiesKilled / m_ChallengeEnemiesMax) * 200f;

            SpawnBonusParticles(transform.position, (int)Mathf.Ceil(bonusParticles));

            StartCoroutine(WaitForParticles());
        }

        private void SpawnBonusParticles(Vector2 position,
                                          int numOfParticles)
        {
            float hue1 = UnityEngine.Random.Range(0f, 6f);
            float hue2 = (hue1 + UnityEngine.Random.Range(0f, 2f)) % 6f;
            Color colour1 = StaticExtensions.Color.FromHSV(hue1, 0.5f, 1);
            Color colour2 = StaticExtensions.Color.FromHSV(hue2, 0.5f, 1);

            for (int i = 0; i < numOfParticles; i++)
            {
                float speed = (12f * (1f - 1 / UnityEngine.Random.Range(2f, 3f))) * 0.02f;

                var state = new ParticleBuilder()
                {
                    velocity = StaticExtensions.Random.RandomVector2(speed, speed),
                    wrapAroundType = WrapAroundType.None,
                    lengthMultiplier = 40f,
                    velocityDampModifier = 0.94f,
                    removeWhenAlphaReachesThreshold = true,
                    canBeCollectedByPlayer = true,
                    maxLengthClamp = 1.5f
                };

                float duration = UnityEngine.Random.Range(200f, 320f);
                var initialScale = new Vector2(1f, 1f);
                var colour = Color.Lerp(colour1, colour2, UnityEngine.Random.Range(0f, 1f));

                ParticleFactory.instance.CreateParticle(position, colour, duration, initialScale, state);
            }
        }


        private void _ShowGameOverMenu()
        {
            if (onPlayerDeathGameOver != null)
            {
                onPlayerDeathGameOver();
            }

            gameOverHandler.Show();
        }

        private void _WaitForParticles()
        {
            StartCoroutine(WaitForParticles());
        }

        private IEnumerator WaitForParticles()
        {
            yield return new WaitWhile(() => ParticleFactory.instance.ParticlesRemaining());

            yield return new WaitForSeconds(1f);

            m_RoundIndex = (m_RoundIndex + 1) % roundPrefabs.Length;

            if (m_RoundIndex == 0)
            {
                m_RoundStorage.SetRound(roundPrefabs.Length);
                OnRoundsComplete();
            }
            else
            { 
                OpenShop();

                m_RoundStorage.SetRound(m_RoundIndex);

                m_PlayerShootController.ResumeAll();
            }
        }

        private void OpenShop()
        {
            if(onRoundOver != null)
            {
                onRoundOver();
            }

            player.transform.position = new Vector2(0f, -3f);
            player.SetActive(false);
            m_ShopController.OpenShop();
        }

        public void CloseShop()
        {
            m_ShopController.CloseShop();
            player.SetActive(true);

            StartNextRound();
        }

        private void StartNextRound()
        {
            m_BGMAudioPlayer.SetVolume(1f, 0.7f);

            m_CurrentRound = GetNextRound();

            // Disbled for release.
            //debugText.text = m_CurrentRound.gameObject.name.Replace("(Clone)", "");

            if (m_CurrentRound.roundType == Round.RoundType.Wave)
            {
                m_RoundText.SetRoundNumber(m_RoundIndex + 1);
            }
            else if (m_CurrentRound.roundType == Round.RoundType.Challenge)
            {
                m_RoundText.SetChallengeWaveStartText();
            }
            else if (m_CurrentRound.roundType == Round.RoundType.Boss)
            {
                m_RoundText.SetBossWaveStartText();
            }

            m_RoundText.ShowForSeconds(2f, 1f, BeginCurrentRound);

            if (onRoundStart != null)
            {
                onRoundStart();
            }
        }

        private Round GetNextRound()
        {
            var roundObj = (GameObject)Instantiate(roundPrefabs[m_RoundIndex]);
            roundObj.transform.SetParent(transform);

            return roundObj.GetComponent<Round>();
        }

        private void BeginCurrentRound()
        {
            pause.EnableButton(1.1f);
            m_CurrentRound.StartRound();
            Invoke("BeginPlayerShooting", 1f);
        }

        private void BeginPlayerShooting()
        {
            m_PlayerShootController.BeginShooting();

            IsPlaying = true;
        }
    }
}