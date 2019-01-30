using UnityEngine;
using System.Collections;

namespace GameCore
{
    /// <summary>
    /// Attach to an enemy object to receive moves from VerticalGroupMovement.
    /// </summary>
    public class EnemyMoveReceiver : MonoBehaviour
    {
        /// <summary>
        /// Moves position by amount.
        /// </summary>
        /// <param name="move">Amount to move.</param>
        public void DoMove(Vector2 move)
        {
            transform.position += (Vector3)move;
        }
    }
}