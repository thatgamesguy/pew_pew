using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ads
{
    public class ADMob_IDs : MonoBehaviour
    {
        /// <summary>
        /// Android
        /// </summary>
        public static readonly string ANDROID_BANNER_ID = "ca-app-pub-3940256099942544/6300978111";
        public static readonly string ANDROID_INTERSTITIAL_ID = "ca-app-pub-3940256099942544/1033173712";

        /// <summary>
        /// iOS
        /// </summary>
        public static readonly string IOS_BANNER_ID = "ca-app-pub-3940256099942544/6300978111";
        public static readonly string IOS_INTERSTITIAL_ID = "ca-app-pub-3940256099942544/1033173712";

        public static string GetBannerAdID()
        {
#if UNITY_EDITOR
            string adUnitId = "unused";
#elif UNITY_ANDROID
            string adUnitId = ADMob_IDs.ANDROID_BANNER_ID;
#elif UNITY_IPHONE
            string adUnitId = ADMob_IDs.IOS_BANNER_ID;
#else
            string adUnitId = "unexpected_platform";
#endif

            return adUnitId;
        }

        public static string GetInterstitialAdID()
        {
#if UNITY_EDITOR
            string adUnitId = "unused";
#elif UNITY_ANDROID
            string adUnitId = ADMob_IDs.ANDROID_INTERSTITIAL_ID;
#elif UNITY_IPHONE
            string adUnitId = ADMob_IDs.IOS_INTERSTITIAL_ID;
#else
            string adUnitId = "unexpected_platform";
#endif

            return adUnitId;
        }
    }
}
