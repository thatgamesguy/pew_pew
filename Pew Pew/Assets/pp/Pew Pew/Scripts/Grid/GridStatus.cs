using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using WarpGrid;

namespace GameCore
{
    /// <summary>
    /// Stores persistent status of grid. Data is stored in PlayerPrefs. When a user disables/enables the grid, it is stored and loaded next time they play.
    /// As object is persistent, the grid status is carried from main menu scene to game scene.
    /// </summary>
    public class GridStatus : MonoBehaviour
    {
        /// <summary>
        /// The toggle used to enable/disable grid.
        /// </summary>
        public Toggle toggle;

        private static readonly string GRID_KEY = "GridStatus";

        private bool m_Enabled = true;

        void Awake()
        {
            m_Enabled = PlayerPrefs.GetInt(GRID_KEY, 1) != 0;

            if (toggle != null)
            {
                toggle.isOn = m_Enabled;
            }
        }

        void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        /// <summary>
        /// Sets the grid status based on toggle status.
        /// </summary>
        /// <param name="ignored">Included to link to Unity toggle. Not used. Toggle status is queried directly from toggle.</param>
        public void SetGridEnabled(bool ignored)
        {
            m_Enabled = toggle.isOn;

            PlayerPrefs.SetInt(GRID_KEY, m_Enabled ? 1 : 0);

            UpdateGrid();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            UpdateGrid();
        }

        private void UpdateGrid()
        {
            if (!m_Enabled)
            {
                WarpGrid.WarpingGrid.Instance.DisableGrid();
            }
            else
            {
                WarpGrid.WarpingGrid.Instance.CreateGrid();
            }
        }
    }
}