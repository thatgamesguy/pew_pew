using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Spawns bomb within set bounds when player taps on screen.
    /// </summary>
    public class BombManager : MonoBehaviour
    {
        /// <summary>
        /// The number of bombs the player starts with.
        /// </summary>
        public int initialBombCount = 1;

        /// <summary>
        /// The bomb prefab to spawn.
        /// </summary>
        public GameObject bombPrefab;

        private ScreenBounds m_Bounds;
        private GameManager m_GameManager;
        private PlayerItemUI m_BombUI;
        private int m_CurrentBombCount;
        private PlayerHealth m_Health;
        private PauseHandler m_GameState;
        private bool m_CanPlaceBombs = true;

        void Awake()
        {
            m_Bounds = GameObject.FindGameObjectWithTag("Bounds").GetComponent<ScreenBounds>();
            m_GameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
            m_BombUI = GameObject.FindGameObjectWithTag("BombUI").GetComponentInChildren<PlayerItemUI>();

            m_GameState = GameObject.FindGameObjectWithTag("UI").GetComponent<PauseHandler>();

            m_Health = FindObjectOfType<PlayerHealth>();
        }

        void Start()
        {
            m_CurrentBombCount = initialBombCount;
            m_BombUI.SetItemCount(m_CurrentBombCount);
        }

        void OnEnable()
        {
            m_Health.OnDeath += DisableBombPlacement;
            m_Health.OnSpawn += EnableBombPlacement;
        }

        void OnDisable()
        {
            m_Health.OnDeath -= DisableBombPlacement;
            m_Health.OnSpawn -= EnableBombPlacement;
        }

        /// <summary>
        /// Increments the bomb count.
        /// </summary>
        public void IncrementBombCount()
        {
            m_CurrentBombCount++;
            m_BombUI.SetItemCount(m_CurrentBombCount);
        }

        private void DecrementBombCount()
        {
            m_CurrentBombCount--;

            if (m_CurrentBombCount < 0)
            {
                Debug.LogWarning("Used bomb when there was 0 bomb count");
                m_CurrentBombCount = 0;
            }

            m_BombUI.SetItemCount(m_CurrentBombCount);
        }

        void Update()
        {
            if (m_GameState.isPaused || !m_CanPlaceBombs)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0) &&
                m_CurrentBombCount > 0 && m_GameManager.IsPlaying)
            {
                var viewportPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);

                if (viewportPos.y <= 0.82f && viewportPos.y >= 0.26f
                    && m_Bounds.IsWithinBounds(viewportPos))
                {
                    DecrementBombCount();

                    var spawnPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    spawnPos.z = 0f;

                    Instantiate(bombPrefab, spawnPos, Quaternion.identity);
                }
            }
        }

        private void EnableBombPlacement()
        {
            m_CanPlaceBombs = true;
        }

        private void DisableBombPlacement()
        {
            m_CanPlaceBombs = false;
        }
    }
}