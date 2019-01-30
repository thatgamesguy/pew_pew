using UnityEngine;
using UnityEngine.UI;

namespace GameCore
{
    /// <summary>
    /// Handles enabling and disabling of points images (used to signify how many instances of an item have been purchased).
    /// </summary>
    public class PointsImages : MonoBehaviour
    {
        /// <summary>
        /// The points images to enable/disable.
        /// </summary>
        public Image[] images;

        private int m_CurrentImage;
        private bool m_AllEnabled;

        void Awake()
        {
            m_CurrentImage = 0;

            foreach (var sprite in images)
            {
                sprite.enabled = false;
            }
        }

        /// <summary>
        /// Gets the number of enabled points images.
        /// </summary>
        /// <returns>The number enabled.</returns>
        public int GetNumberEnabled()
        {
            int enabled = 0;

            foreach (var i in images)
            {
                if (i.enabled)
                {
                    enabled++;
                }
            }

            return enabled;
        }

        /// <summary>
        /// Enables the next point image.
        /// </summary>
        public void EnableNextPointImage()
        {
            if (!m_AllEnabled)
            {
                images[m_CurrentImage].enabled = true;

                m_CurrentImage = (m_CurrentImage + 1) % images.Length;

                m_AllEnabled = m_CurrentImage == 0;
            }
        }

        /// <summary>
        /// Disables the number of images. First checks if that many images are enabled. If not an error is output.
        /// </summary>
        /// <param name="numToDisable">Number to disable.</param>
        public void DisableImages(int numToDisable)
        {
            if (numToDisable <= 0)
            {
                return;
            }

            int enabled = GetNumberEnabled();

            if (numToDisable > GetNumberEnabled())
            {
                Debug.LogWarning("Cannot disable " + numToDisable + " images, as only " + enabled + " are activated");

                return;
            }

            int index = m_CurrentImage - 1;

            if (index < 0)
            {
                index = images.Length - 1;
            }


            for (int i = 0; i < numToDisable; i++)
            {
                images[index].enabled = false;
                index--;
            }

            m_CurrentImage = index + 1;
            m_AllEnabled = false;
        }
    }
}