using UnityEngine;
using UnityEngine.UI;


namespace GameCore
{
    /// <summary>
    /// Toggles game audio on button press.
    /// </summary>
    public class AudioToggle : MonoBehaviour
    {
        /// <summary>
        /// Image to display when audio is not muted.
        /// </summary>
        public Sprite audioNonMutedImage;

        /// <summary>
        /// Image to display when audio is muted.
        /// </summary>
        public Sprite audioMutedImage;

        private Button m_ToggleButton;
        private BGMAudioPlayer m_BGMAudioPlayer;

        void Awake()
        {
            m_ToggleButton = GetComponent<Button>();
        }

        void Start()
        {
            m_BGMAudioPlayer = GameObject.FindObjectOfType<BGMAudioPlayer>();

            SetImage();
        }

        /// <summary>
        /// Toggles audio playing and button image.
        /// </summary>
        public void Toggle()
        {
            m_BGMAudioPlayer.ToggleAudio();

            SetImage();
        }

        private void SetImage()
        {
            m_ToggleButton.image.sprite = m_BGMAudioPlayer.muted ? audioMutedImage : audioNonMutedImage;
        }
    }
}