using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Adjusts enemies movement speed near round end.
    /// </summary>
    public class EnemyMoveSpeedAdjuster : MonoBehaviour
    {
        [Range(0f, 100f)]
        /// <summary>
        /// The percentage of enemies remaining to trigger speed increase.
        /// </summary>
        public int enemeisRemainingPercentInc = 20;

        /// <summary>
        /// The amount to increase background audio on round over.
        /// </summary>
        public float bgmPitchIncreaseOnRoundOver = 0.01f;

        private AdjustableMoveSpeed[] m_AdjustableMoveSpeeds;
        private AdjustableShootSpeed[] m_AdjustableShootSpeeds;
        private float m_MaxEnemies;
        private float m_EnemyCount;
        private Round m_Round;
        private BGMAudioPlayer m_BGMAudioPlayer;

        void Awake()
        {
            m_BGMAudioPlayer = FindObjectOfType<BGMAudioPlayer>();

            m_AdjustableMoveSpeeds = GetComponentsInChildren<AdjustableMoveSpeed>();
            m_AdjustableShootSpeeds = GetComponentsInChildren<AdjustableShootSpeed>();

            m_Round = GetComponent<Round>();
        }

        void Start()
        {
            m_EnemyCount = m_Round.maxEnemies;
            m_MaxEnemies = m_Round.maxEnemies;
        }

        void OnEnable()
        {
            m_Round.onEnemyRemoved += CheckForSpeedIncrement;
        }

        void Disable()
        {
            m_Round.onEnemyRemoved += CheckForSpeedIncrement;
        }

        private void CheckForSpeedIncrement()
        {
            m_EnemyCount--;

            if (m_EnemyCount == 0)
            {
                return;
            }

            if (m_EnemyCount == 1)
            {

                DoIncrement();
                return;
            }
            else
            {
                float percent = (m_EnemyCount / m_MaxEnemies) * 100f;
                if (percent <= enemeisRemainingPercentInc)
                {
                    DoIncrement();
                    enemeisRemainingPercentInc = 0;
                }
            }

        }

        private void DoIncrement()
        {
            IncrementMoveSpeed();
            IncrementShootSpeed();

            m_BGMAudioPlayer.IncreasePitch(bgmPitchIncreaseOnRoundOver);
        }

        private void IncrementMoveSpeed()
        {
            foreach (var adjSpeed in m_AdjustableMoveSpeeds)
            {
                if (adjSpeed != null)
                {
                    adjSpeed.IncrementSpeed();
                }
            }
        }

        private void IncrementShootSpeed()
        {
            foreach (var adjShoot in m_AdjustableShootSpeeds)
            {
                if (adjShoot != null)
                {
                    adjShoot.IncrementSpeed();
                }
            }
        }
    }
}