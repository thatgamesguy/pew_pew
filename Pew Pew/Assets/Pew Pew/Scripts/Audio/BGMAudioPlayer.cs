using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameCore
{
    /// <summary>
    /// Handles playing background music. Can fade between clips and queue pitch and volume lerp.
    /// </summary>
    public class BGMAudioPlayer : MonoBehaviour
    {
        /// <summary>
        /// The AudioClip to play at main menu.
        /// </summary>
        public AudioClip menuAudio;

        /// <summary>
        /// The AudioClips to play when gameplay scene is loaded. If more than one, a random clip is selected.
        /// </summary>
        public AudioClip[] gameAudio;

        /// <summary>
        /// The AudioClip to play on game over.
        /// </summary>
        public AudioClip gameoverClip;

        /// <summary>
        /// The time in seconds the fade between audio tracks lasts.
        /// </summary>
        public float switchAudioTrackLerpSecs = 2f;

        /// <summary>
        /// The maximum audio pitch
        /// </summary>
        public float maxBGMPitch = 1.1f;

        /// <summary>
        /// The max background volume.
        /// </summary>
        public float maxBGMVolume = 0.7f;

        /// <summary>
        /// Whether this instance of BGMAudioPlayer is muted.
        /// </summary>
        public bool muted { get { return !m_ShouldPlay; } }

        private static readonly string AUDIO_KEY = "BGM_AUDIO_TOGGLE";

        private AudioSource m_AudioSource;
        private bool m_ShouldPlay = true;
        private Queue<QueuedAudioAdjustment> m_VolQueue = new Queue<QueuedAudioAdjustment>();
        private Queue<QueuedAudioAdjustment> m_PitchQueue = new Queue<QueuedAudioAdjustment>();

        void Awake()
        {
            m_AudioSource = GetComponent<AudioSource>();
            m_AudioSource.loop = true;

            m_ShouldPlay = PlayerPrefs.GetInt(AUDIO_KEY, 1) == 1 ? true : false;
        }

        void Start()
        {
            DontDestroyOnLoad(gameObject);

            m_AudioSource.volume = maxBGMVolume;

            StartCoroutine(LerpPitch());
            StartCoroutine(LerpVolume());
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnLevelFinishedLoading;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        }

        /// <summary>
        /// Toggles audio playing. Stores setting in PlayerPrefs and is loaded at game start.
        /// </summary>
        public void ToggleAudio()
        {
            m_ShouldPlay = !m_ShouldPlay;

            PlayerPrefs.SetInt(AUDIO_KEY, m_ShouldPlay ? 1 : 0);

            if (m_ShouldPlay)
            {
                m_AudioSource.Play();
            }
            else
            {
                m_AudioSource.Stop();
            }
        }

        /// <summary>
        /// Plays the game over AudioClip.
        /// </summary>
        public void PlayGameOverBGM()
        {
            if (m_ShouldPlay)
            {
                SwitchClips(gameoverClip, 2.2f);
            }
        }

        /// <summary>
        /// Enques a volume change. This volume change is lerped to by seconds.
        /// </summary>
        /// <param name="vol">Target Volume.</param>
        /// <param name="seconds">Seconds to spend lerping to volume.</param>
        /// <param name="clipToChangeTo">Clip to change to. If null the current clip is continued.</param>
        public void SetVolume(float vol, float seconds, AudioClip clipToChangeTo = null)
        {
            vol = Mathf.Clamp(vol, 0f, maxBGMVolume);

            m_VolQueue.Enqueue(new QueuedAudioAdjustment(vol, seconds, clipToChangeTo));
        }

        /// <summary>
        /// Increases the pitch by amount. Capped by maxBGMPitch.
        /// </summary>
        /// <param name="amount">Amount.</param>
        public void IncreasePitch(float amount)
        {
            m_AudioSource.pitch += amount;

            if (m_AudioSource.pitch > maxBGMPitch)
            {
                m_AudioSource.pitch = maxBGMPitch;
            }
        }

        /// <summary>
        /// Enques a pitch change. This pitch change is lerped to by seconds.
        /// </summary>
        /// <param name="pitch">Target pitch.</param>
        /// <param name="seconds">Seconds to spend lerping to pitch.</param>
        /// <param name="clipToChangeTo">Clip to change to. If null the current clip is continued.</param>
        public void SetPitch(float pitch, float seconds, AudioClip clipToChangeTo = null)
        {
            m_PitchQueue.Enqueue(new QueuedAudioAdjustment(pitch, seconds, clipToChangeTo));
        }

        /// <summary>
        /// Fade between the currently play clip and this clip.
        /// </summary>
        /// <param name="to">The clip to fade to.</param>
        /// <param name="seconds">The time in seconds for the fade.</param>
        public void SwitchClips(AudioClip to, float seconds)
        {
            SetVolume(0f, seconds * 0.5f, to);

            SetVolume(maxBGMVolume, seconds * 0.5f);
        }

        private IEnumerator LerpVolume()
        {
            while (true)
            {
                if (m_VolQueue.Count > 0)
                {
                    var data = m_VolQueue.Dequeue();

                    float updateDuration = data.seconds * 1000f;

                    float initialVol = m_AudioSource.volume;

                    float timeParam = 0f;

                    float lastTime = System.Environment.TickCount;

                    while (timeParam < 1f)
                    {
                        var currentTick = System.Environment.TickCount;

                        timeParam += (currentTick - lastTime) / updateDuration;

                        m_AudioSource.volume = Mathf.Lerp(initialVol, data.adjustment, timeParam);

                        lastTime = currentTick;

                        yield return null;
                    }

                    if (data.clipToChangeTo != null)
                    {
                        m_AudioSource.clip = data.clipToChangeTo;
                        m_AudioSource.Play();
                    }

                }

                yield return new WaitUntil(() => m_VolQueue.Count > 0);
            }
        }

        private IEnumerator LerpPitch()
        {
            while (true)
            {
                if (m_PitchQueue.Count > 0)
                {
                    var data = m_PitchQueue.Dequeue();

                    float updateDuration = data.seconds * 1000f;

                    float initialPitch = m_AudioSource.pitch;

                    float timeParam = 0f;

                    float lastTime = System.Environment.TickCount;

                    while (timeParam < 1f)
                    {
                        var currentTick = System.Environment.TickCount;

                        timeParam += (currentTick - lastTime) / updateDuration;

                        m_AudioSource.pitch = Mathf.Lerp(initialPitch, data.adjustment, timeParam);

                        lastTime = currentTick;

                        yield return null;
                    }

                    if (data.clipToChangeTo != null)
                    {
                        m_AudioSource.clip = data.clipToChangeTo;
                        m_AudioSource.Play();
                    }
                }

                yield return new WaitUntil(() => m_PitchQueue.Count > 0);
            }
        }

        private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
        {

            int sceneIndex = scene.buildIndex;

            if (sceneIndex == 0)
            {
                m_AudioSource.clip = menuAudio;

                if (m_ShouldPlay)
                {
                    m_AudioSource.Play();
                    m_AudioSource.volume = 0f;
                    SetVolume(1f, 2.5f);
                }
            }
            else if (m_ShouldPlay)
            {
                if (gameAudio != null && gameAudio.Length > 0)
                {
                    SwitchClips(gameAudio[UnityEngine.Random.Range(0, gameAudio.Length)], switchAudioTrackLerpSecs);
                }
            }
        }

        /// <summary>
        /// Holds data related to an audio adjustment (pitch or volume).
        /// </summary>
        private struct QueuedAudioAdjustment
        {
            public float adjustment;
            public float seconds;
            public AudioClip clipToChangeTo;

            /// <summary>
            /// Initializes a new instance of the <see cref="BGMAudioPlayer+QueuedAudioAdjustment"/> struct.
            /// </summary>
            /// <param name="adjustment">The adjustment to make.</param>
            /// <param name="seconds">Seconds.</param>
            /// <param name="clipToChangeTo">Clip to change to.</param>
            public QueuedAudioAdjustment(float adjustment, float seconds, AudioClip clipToChangeTo)
            {
                this.adjustment = adjustment;
                this.seconds = seconds;
                this.clipToChangeTo = clipToChangeTo;
            }
        }
    }
}