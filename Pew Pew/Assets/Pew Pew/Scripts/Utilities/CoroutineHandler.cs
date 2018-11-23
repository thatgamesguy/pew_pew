using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Provides access to coroutines for instances that do not inherit form monobehaviour.
    /// </summary>
    public class CoroutineHandler : MonoBehaviour
    {
        /// <summary>
        /// Runs the coroutine.
        /// </summary>
        /// <param name="routine">Coroutine to execute.</param>
        public void RunCoroutine(IEnumerator routine)
        {
            StartCoroutine(routine);
        }
    }
}