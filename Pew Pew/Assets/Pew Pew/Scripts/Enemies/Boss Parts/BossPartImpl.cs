using UnityEngine;
using System.Collections;
using System;

namespace GameCore
{
    /// <summary>
    /// Contract for all boss parts.
    /// </summary>
    public interface BossPart
    {
        void Activate();
    }

    /// <summary>
    /// Abstract concrete implementation of BossPart.
    /// </summary>
    [RequireComponent(typeof(EnemyHealth))]
    public abstract class BossPartImpl : MonoBehaviour, BossPart, EnemyMove
    {
        /// <summary>
        /// The next boss part. When this boss part dies, the next part is activated (if not null).
        /// </summary>
        public BossPartImpl next;

        /// <summary>
        /// Signifies if this instance is the first in the boss queue.
        /// </summary>
        public bool isFirst = false;

        protected bool m_HasBeenActivated = false;

        private static PlayerHealth PLAYER_HEALTH;

        private HitDeathInvoker m_Health;
        private SpriteRenderer m_SpriteRenderer;
        private float m_StartTime;
        private Collider2D[] m_Colliders;
        private bool m_WaitingForPlayerToSpawn = false;

        protected virtual void Awake()
        {
            m_Health = GetComponent<HitDeathInvoker>();
            m_SpriteRenderer = GetComponent<SpriteRenderer>();
            m_Colliders = GetComponents<Collider2D>();

            if (PLAYER_HEALTH == null)
            {
                PLAYER_HEALTH = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>();
            }
        }

        protected virtual void Start()
        {
            foreach (var c in m_Colliders)
            {
                c.enabled = false;
            }

            m_SpriteRenderer.color = m_SpriteRenderer.color.WithAlpha(0f);

            if (GetComponent<EnemyMoveRegister>() == null)
            {
                gameObject.AddComponent<EnemyMoveRegister>();
            }
        }

        void OnEnable()
        {
            m_Health.onDeath += OnDeath;
            m_Health.onHit += OnHit;

            PLAYER_HEALTH.OnDeath += PlayerStartedSpawning;
            PLAYER_HEALTH.OnSpawn += PlayerFinishedSpawning;
        }

        void OnDisable()
        {
            m_Health.onDeath -= OnDeath;
            m_Health.onHit -= OnHit;

            PLAYER_HEALTH.OnDeath -= PlayerStartedSpawning;
            PLAYER_HEALTH.OnSpawn -= PlayerFinishedSpawning;
        }

        /// <summary>
        /// Begin this instance.
        /// </summary>
        public void Begin()
        {
            m_StartTime = Time.time;
            StartCoroutine(FadeIn());
        }

        /// <summary>
        /// Pause this instance.
        /// </summary>
        public abstract void Pause();

        /// <summary>
        /// Resume this instance.
        /// </summary>
        public abstract void Resume();

        /// <summary>
        /// Activate this instance. Waits for player to spawn, flashes sprite, and then calls
        /// abstract DoActivation method (which is implementd in sub classes).
        /// </summary>
        public void Activate()
        {
            StartCoroutine(_Activate(DoActivation));
        }

        protected abstract void DoActivation();

        private IEnumerator WaitForBaseActivation(Action callbackOnActivation)
        {
            yield return new WaitUntil(() => m_HasBeenActivated);

            callbackOnActivation();
        }

        private void PlayerFinishedSpawning()
        {
            m_WaitingForPlayerToSpawn = false;
        }

        private void PlayerStartedSpawning()
        {
            m_WaitingForPlayerToSpawn = true;
        }

        private void OnDeath()
        {
            if (next != null)
            {
                next.Activate();
            }
        }

        private void OnHit()
        {
            m_SpriteRenderer.DoDamageFlash();
        }

        private IEnumerator _Activate(Action callbackOnComplete)
        {
            yield return new WaitForSeconds(.6f);

            if (m_WaitingForPlayerToSpawn)
            {
                yield return new WaitUntil(() => !m_WaitingForPlayerToSpawn);
            }

            const float waitTime = .09f;

            float activationDelay = 1f;

            while (activationDelay > 0f)
            {
                activationDelay -= waitTime;

                m_SpriteRenderer.enabled = !m_SpriteRenderer.enabled;

                yield return new WaitForSeconds(waitTime);
            }

            m_SpriteRenderer.enabled = true;

            foreach (var c in m_Colliders)
            {
                c.enabled = true;
            }

            m_HasBeenActivated = true;

            callbackOnComplete();
        }

        private IEnumerator FadeIn()
        {
            float t = 0f;

            while (t < 1f)
            {
                t = (Time.time - m_StartTime) / GameManager.ROUND_BEGIN_TIME;
                m_SpriteRenderer.color = m_SpriteRenderer.color.WithAlpha(Mathf.SmoothStep(0f, 1f, t));
                yield return new WaitForEndOfFrame();
            }

            if (isFirst)
            {
                Activate();
            }
        }
    }
}