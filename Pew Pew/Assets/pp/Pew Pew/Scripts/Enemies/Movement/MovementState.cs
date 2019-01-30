using UnityEngine;
using System;
using System.Collections;

namespace GameCore
{
    /// <summary>
    /// Contract for a directional movement state.
    /// </summary>
    public interface MovementState
    {
        /// <summary>
        /// Enter the specified state.
        /// </summary>
        /// <param name="owner">Owner of state.</param>
        void Enter(Transform owner);

        /// <summary>
        /// Returns the next movement.
        /// </summary>
        /// <returns>The move to perform.</returns>
        Vector2 NextMove();

        /// <summary>
        /// Returns true if object has completed the movement state.
        /// </summary>
        /// <returns><c>true</c>, if move was completeded, <c>false</c> otherwise.</returns>
        /// <param name="owner">Owner.</param>
        bool CompletedMove(Transform owner);
    }

    /// <summary>
    /// Move right movement state.
    /// </summary>
    public class MoveRight : MovementState
    {
        private ScreenBounds m_Bounds;

        /// <summary>
        /// Initializes a new instance of the <see cref="MoveRight"/> class.
        /// </summary>
        /// <param name="bounds">Bounds.</param>
        public MoveRight(ScreenBounds bounds)
        {
            m_Bounds = bounds;
        }

        /// <summary>
        /// Enter the specified state.
        /// </summary>
        /// <param name="owner">Owner of state.</param>
        public void Enter(Transform owner)
        {

        }

        /// <summary>
        /// Returns the next movement.
        /// </summary>
        /// <returns>The move to perform.</returns>
        public Vector2 NextMove()
        {
            return Vector2.right;
        }

        /// <summary>
        /// Returns true if object has completed the movement state.
        /// </summary>
        /// <returns>true</returns>
        /// <c>false</c>
        /// <param name="owner">Owner.</param>
        public bool CompletedMove(Transform owner)
        {
            return owner.position.x >= m_Bounds.GetHorizontalBounds().y;
        }
    }

    /// <summary>
    /// Move left movement state.
    /// </summary>
    public class MoveLeft : MovementState
    {
        private ScreenBounds m_Bounds;

        /// <summary>
        /// Initializes a new instance of the <see cref="MoveLeft"/> class.
        /// </summary>
        /// <param name="bounds">Bounds.</param>
        public MoveLeft(ScreenBounds bounds)
        {
            m_Bounds = bounds;
        }

        /// <summary>
        /// Enter the specified state.
        /// </summary>
        /// <param name="owner">Owner of state.</param>
        public void Enter(Transform owner)
        {

        }

        /// <summary>
        /// Returns the next movement.
        /// </summary>
        /// <returns>The move to perform.</returns>
        public Vector2 NextMove()
        {
            return -Vector2.right;
        }

        /// <summary>
        /// Returns true if object has completed the movement state.
        /// </summary>
        /// <returns>true</returns>
        /// <c>false</c>
        /// <param name="owner">Owner.</param>
        public bool CompletedMove(Transform owner)
        {
            return owner.position.x <= m_Bounds.GetHorizontalBounds().x;
        }
    }

    /// <summary>
    /// Move up movement state.
    /// </summary>
    public class MoveUp : MovementState
    {
        /// <summary>
        /// Enter the specified state.
        /// </summary>
        /// <param name="owner">Owner of state.</param>
        public void Enter(Transform owner)
        {

        }

        /// <summary>
        /// Returns the next movement.
        /// </summary>
        /// <returns>The move to perform.</returns>
        public Vector2 NextMove()
        {
            return Vector2.up;
        }

        /// <summary>
        /// Returns true if object has completed the movement state.
        /// </summary>
        /// <returns>true</returns>
        /// <c>false</c>
        /// <param name="owner">Owner.</param>
        public bool CompletedMove(Transform owner)
        {
            return true;
        }
    }

    /// <summary>
    /// Move down movement state.
    /// </summary>
    public class MoveDown : MovementState
    {
        private Vector2 m_TargetPos;
        private Vector2 m_TargetOffset;

        /// <summary>
        /// Initializes a new instance of the <see cref="MoveDown"/> class.
        /// </summary>
        /// <param name="targetOffset">Target offset.</param>
        public MoveDown(Vector2 targetOffset)
        {
            m_TargetOffset = targetOffset;
        }

        /// <summary>
        /// Enter the specified state.
        /// </summary>
        /// <param name="owner">Owner of state.</param>
        public void Enter(Transform owner)
        {
            m_TargetPos = (Vector2)owner.position + m_TargetOffset;
        }

        /// <summary>
        /// Returns the next movement.
        /// </summary>
        /// <returns>The move to perform.</returns>
        public Vector2 NextMove()
        {
            return -Vector2.up;
        }

        /// <summary>
        /// Returns true if object has completed the movement state.
        /// </summary>
        /// <returns>true</returns>
        /// <c>false</c>
        /// <param name="owner">Owner.</param>
        public bool CompletedMove(Transform owner)
        {
            return owner.position.y <= m_TargetPos.y;
        }
    }
}