using UnityEngine;
using UnityEngine.UI;

namespace GameCore
{
    /// <summary>
    /// Responsible for opening and closing shop, and updating whether items can be purchased.
    /// </summary>
    public class ShopController : MonoBehaviour
    {
        /// <summary>
        /// The audio to play on shop close.
        /// </summary>
        public AudioClip audioOnShopClose;

        /// <summary>
        /// THe button responsible for closing the shop. This button is disabled whilst the shop opening animation plays.
        /// </summary>
        public Button closeShopButton;

        /// <summary>
        /// The animation controller responsible for animating the shop buttons.
        /// </summary>
        public ButtonAnimationController animationController;

        private AudioPlayer m_AudioSource;
        private ShopPurchaseAction m_PurchaseAction;
        private ShopPurchaseActionable[] m_Purchasables;


        void Awake()
        {
            m_AudioSource = Camera.main.GetComponent<AudioPlayer>();
            m_PurchaseAction = GetComponent<ShopPurchaseAction>();
            m_Purchasables = GetComponentsInChildren<ShopPurchaseActionable>();
        }

        void Start()
        {
            CloseShop();
        }

        void OnEnable()
        {
            m_PurchaseAction.OnPuchase += UpdatePurchases;
            animationController.OnAnimationComplete += EnableCloseButton;
        }

        void OnDisable()
        {
            m_PurchaseAction.OnPuchase -= UpdatePurchases;
            animationController.OnAnimationComplete -= EnableCloseButton;
        }

        /// <summary>
        /// Opens the shop and updates purchases.
        /// </summary>
        public void OpenShop()
        {
            closeShopButton.interactable = false;
            UpdatePurchases();
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Closes the shop and plays audioclip.
        /// </summary>
        public void CloseShop()
        {
            m_AudioSource.PlayInstance(audioOnShopClose);
            gameObject.SetActive(false);
        }

        private void UpdatePurchases()
        {
            foreach (var purchase in m_Purchasables)
            {
                purchase.CheckActionable();
                purchase.CheckComplete();
            }
        }

        private void EnableCloseButton()
        {
            closeShopButton.interactable = true;
        }
    }
}