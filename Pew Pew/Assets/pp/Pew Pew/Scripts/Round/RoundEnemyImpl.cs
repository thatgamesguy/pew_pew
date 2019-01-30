using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Contract for an enemy that is part of a pround.
    /// </summary>
    public interface RoundEnemy
    {
        /// <summary>
        /// Gets entities transform.
        /// </summary>
        /// <value>Transform.</value>
        Transform myTransform { get; }

        /// <summary>
        /// Registers the round owner. Used to let round owner know when it has been removed from the round.
        /// </summary>
        /// <param name="round">Round.</param>
        void RegisterRoundOwner(RoundOwner round);

        /// <summary>
        /// Signifies that the enemy has escaped the round.
        /// </summary>
        void EscapedWave();
    }

    /// <summary>
    /// Round enemy implementation. Informs roundowner when entity has been killed or has escaped round.
    /// </summary>
    public class RoundEnemyImpl : MonoBehaviour, RoundEnemy
    {
        /// <summary>
        /// Gets entities transform.
        /// </summary>
        /// <value>Transform.</value>
        public Transform myTransform { get { return transform; } }

        private RoundOwner m_Round;
        private HitDeathInvoker m_HitDeathInvoker;
        private ChallengeMovement m_ChallengeMovement;

        void Awake()
        {
            m_HitDeathInvoker = GetComponent<HitDeathInvoker>();
            m_ChallengeMovement = GetComponent<ChallengeMovement>();
        }

        void OnEnable()
        {
            m_HitDeathInvoker.onDeath += RemoveFromRound;

            if (m_ChallengeMovement)
            {
                m_ChallengeMovement.onEscapedWave += EscapedWave;
            }
        }

        void OnDisable()
        {
            m_HitDeathInvoker.onDeath -= RemoveFromRound;

            if (m_ChallengeMovement)
            {
                m_ChallengeMovement.onEscapedWave -= EscapedWave;
            }
        }

        /// <summary>
        /// Registers the round owner. Used to let round owner know when it has been removed from the round.
        /// </summary>
        /// <param name="round">Round.</param>
        public void RegisterRoundOwner(RoundOwner round)
        {
            m_Round = round;
        }

        /// <summary>
        /// Signifies that the enemy has escaped the round.
        /// </summary>
        public void EscapedWave()
        {
            m_Round.EnemyEscapedRound(this);
        }

        private void RemoveFromRound()
        {
            m_Round.RemoveEnemyFromRound(this);
        }
    }
}