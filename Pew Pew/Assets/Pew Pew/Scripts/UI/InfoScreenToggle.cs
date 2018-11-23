using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore
{
    /// <summary>
    /// Shows info screen and handles ui requests from that screen.
    /// </summary>
    public class InfoScreenToggle : MonoBehaviour
    {
        /// <summary>
        /// The button sprite to use when the info screen is closed.
        /// </summary>
        public Sprite openInfoImage;

        /// <summary>
        /// The button sprite to use when the info screen is open.
        /// </summary>
        public Sprite closeInfoImage;

        /// <summary>
        /// The parent object of the info screen.
        /// </summary>
        public GameObject infoScreen;

        /// <summary>
        /// The objects to hide before showing the info screen.
        /// </summary>
        public GameObject[] objectsToHide;

        private Button m_ToggleButton;

        void Awake()
        {
            m_ToggleButton = GetComponent<Button>();
        }

        void Start()
        {
            infoScreen.SetActive(false);
            SetImage();
        }

        /// <summary>
        /// If info screen is open, it is closed and vise versa.
        /// </summary>
        public void Toggle()
        {
            infoScreen.SetActive(!infoScreen.activeSelf);

            foreach (var obj in objectsToHide)
            {
                obj.SetActive(!infoScreen.activeSelf);
            }

            SetImage();
        }

        private void SetImage()
        {
            m_ToggleButton.image.sprite = infoScreen.activeSelf ? closeInfoImage : openInfoImage;
        }
    }
}