using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Provides functionality to add new shoot modules to player (when purchased through the store).
    /// </summary>
    public class PlayerShootModules : MonoBehaviour
    {
        /// <summary>
        /// The shoot modules the can be enabled in game.
        /// </summary>
        public GameObject[] shootModules;

        private int m_CurrentModuleIndex = 0;

        void Start()
        {
            foreach (var module in shootModules)
            {
                module.SetActive(false);
            }
        }

        /// <summary>
        /// Determines whether a new module can be enabled.
        /// </summary>
        /// <returns><c>true</c> if this instance is actionable; otherwise, <c>false</c>.</returns>
        public bool IsActionable()
        {
            return GetNumberOfActionableModules() > 0;
        }

        /// <summary>
        /// Gets the number of modules that can be activated.
        /// </summary>
        /// <returns>The number of modules that can be activated.</returns>
        public int GetNumberOfActionableModules()
        {
            int actionable = 0;

            foreach (var module in shootModules)
            {
                if (!module.activeSelf)
                {
                    actionable++;
                }
            }

            return actionable;
        }

        /// <summary>
        /// Enables a new module.
        /// </summary>
        public void EnableNewModule()
        {
            int attempts = shootModules.Length;

            for (int i = m_CurrentModuleIndex; attempts >= 0; i = (i + 1) % shootModules.Length)
            {
                if (!shootModules[i].activeSelf)
                {
                    m_CurrentModuleIndex = i;
                    break;
                }
                attempts--;
            }

            if (attempts >= 0)
            {
                shootModules[m_CurrentModuleIndex].SetActive(true);
            }
        }

    }
}