using UnityEngine;
using UnityEngine.SceneManagement;
using GoogleMobileAds.Api;

namespace Ads
{
    public class ADMob_Banner : MonoBehaviour
    {
        public bool displayADMenu = true;
        public bool displayADGame = true;

        private BannerView m_Bannerview;

        void Start()
        {
            DontDestroyOnLoad(gameObject);

            if(!displayADMenu)
            {
                return;
            }

            if (displayADMenu || displayADGame)
            {
                CreateBanner();
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        private void OnSceneUnloaded(Scene scene)
        {
            if (scene.buildIndex == 0) // Are we unloading the main menu scene?
            {
                if (displayADMenu && !displayADGame)
                {
                    m_Bannerview.Destroy();     // Banner is currently being displayed so we remove it.
                    m_Bannerview = null;
                }
                else if (!displayADMenu && displayADGame)
                {
                    CreateBanner();             // Banner hasn't been created yet so we create it.
                }
            }
        }

        private void CreateBanner()
        {
            m_Bannerview = new BannerView(ADMob_IDs.GetBannerAdID(), AdSize.SmartBanner, AdPosition.Bottom);

            AdRequest request = new AdRequest.Builder().Build();

            m_Bannerview.LoadAd(request);
        }


    }
}
