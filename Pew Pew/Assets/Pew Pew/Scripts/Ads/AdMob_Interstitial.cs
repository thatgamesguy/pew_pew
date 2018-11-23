using GoogleMobileAds.Api;
using UnityEngine;
using GameCore;
using System;

namespace Ads
{
    public class AdMob_Interstitial : MonoBehaviour
    {
        public bool loadOnRoundOver = false;
        public int numOfRoundsRequired = 1;

        public bool loadOnPlayerDeath = false;
        public int numOfDeathsRequired = 1;

        public bool loadOnGameOver = false;

        private InterstitialAd m_Interstitial;
        private GameManager m_GameManager;
        private PlayerHealth m_Player;
        private int m_RoundOverCount = 0;
        private int m_DeathCount = 0;

        void Awake()
        {
            m_GameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
            m_Player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>();
        }

        // Use this for initialization
        void Start()
        {
            if (loadOnRoundOver || loadOnGameOver || loadOnPlayerDeath)
            {
                CreateIntersitial(null, null);
            }

            if(loadOnRoundOver && numOfRoundsRequired < 1)
            {
                Debug.LogWarning("Number of rounds required to show ad should be equal to or greater than 1, setting to 1");
                numOfRoundsRequired = 1;
            }

            if (loadOnPlayerDeath && numOfDeathsRequired < 1)
            {
                Debug.LogWarning("Number of deaths required to show ad should be equal to or greater than 1, setting to 1");
                numOfDeathsRequired = 1;
            }
        }

        void OnEnable()
        {
            if (loadOnRoundOver)
            {
                m_GameManager.onRoundOver += CalculateRoundOverAD;
            }

            if(loadOnPlayerDeath)
            {
                m_Player.OnPlayerDeathPreSpawn += CalculatePlayerDeathAD;
            }

            if(loadOnGameOver)
            {
                m_GameManager.onPlayerDeathGameOver += ShowAd;
            }
        }


        void OnDisable()
        {
            if (loadOnRoundOver)
            {
                m_GameManager.onRoundOver -= CalculateRoundOverAD;
            }

            if (loadOnPlayerDeath)
            {
                m_Player.OnPlayerDeathPreSpawn -= CalculatePlayerDeathAD;
            }

            if (loadOnGameOver)
            {
                m_GameManager.onPlayerDeathGameOver -= ShowAd;
            }
        }


        private void CreateIntersitial(object sender, EventArgs e)
        {
            if(m_Interstitial != null)
            {
                m_Interstitial.OnAdClosed -= CreateIntersitial;
            }

            m_Interstitial = new InterstitialAd(ADMob_IDs.GetInterstitialAdID());
            AdRequest request = new AdRequest.Builder().Build();
            m_Interstitial.LoadAd(request);

            m_Interstitial.OnAdClosed += CreateIntersitial;
        }

        private void CalculateRoundOverAD()
        {
            if (++m_RoundOverCount % numOfRoundsRequired == 0)
            {
                ShowAd();
            }
        }

        private void CalculatePlayerDeathAD()
        {
            if (++m_DeathCount % numOfDeathsRequired == 0)
            {
                ShowAd();
            }
        }

        private void ShowAd()
        {
            if (m_Interstitial.IsLoaded())
            {
                m_Interstitial.Show();
            }
        }
    }
}