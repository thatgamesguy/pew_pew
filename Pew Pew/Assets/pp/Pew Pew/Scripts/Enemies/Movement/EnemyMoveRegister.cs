using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Adds attached EnemyMove to GameManager::EnemyMoves.
    /// </summary>
    public class EnemyMoveRegister : MonoBehaviour
    {
        private EnemyMove m_Move;

        void Awake()
        {
            m_Move = GetComponent<EnemyMove>();
        }

        void OnEnable()
        {
            if (m_Move != null)
            {
                GameManager.EnemyMoves.Add(m_Move);
            }
        }

        void OnDisable()
        {
            if (m_Move != null)
            {
                GameManager.EnemyMoves.Remove(m_Move);
            }
        }
    }
}