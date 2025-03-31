// Copyright 2025 Udfowner
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;
using GoogleMobileAds.Api;
using System;
using Assets.Scripts.utils.ad.exception;

namespace Assets.Scripts.utils.ad
{
    public class RxInterstitialAd
    {
        private static RxInterstitialAd instance;

        private InterstitialAd _interstitialAd;

        private string adUnitId;

        private Action<RxInterstitialAd, AdException> onFailedCallback;

        private Action<RxInterstitialAd> onAdClosedCallback;

        private Action<RxInterstitialAd> onAdClickedCallback;

        private RxInterstitialAd() { }

        public static RxInterstitialAd Init(string adUnitId)
        {

            instance ??= new();

            instance.adUnitId = adUnitId;

            return instance;
        }

        public static RxInterstitialAd InitWithTestAd()
        {

            instance ??= new();

#if UNITY_ANDROID
            instance.adUnitId = "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_IPHONE
            instance.adUnitId = "ca-app-pub-3940256099942544/4411468910";
#else
            instance.adUnitId = "unused";
#endif

            return instance;
        }

        public RxInterstitialAd SetOnFailedLoadCallback(Action<RxInterstitialAd, Exception> onFailedLoad)
        {
            onFailedCallback = onFailedLoad;

            return this;
        }

        public RxInterstitialAd SetOnAdClosedCallback(Action<RxInterstitialAd> callback)
        {
            onAdClosedCallback = callback;

            return this;
        }

        public RxInterstitialAd SetOnAdClickedCallback(Action<RxInterstitialAd> callback)
        {
            onAdClickedCallback = callback;

            return this;
        }

        public void Load(Action<RxInterstitialAd> onSuccessLoaded = null)
        {

#if UNITY_ANDROID || UNITY_IOS
            MobileAds.Initialize(initStatus =>
            {
                DestoryAdIfNeed();
                LoadAd(onSuccessLoaded);
            });
#else
            onFailedCallback?.Invoke(this, new("RxInterstitialAd.initialize: Platform is not supported!"));
#endif
        }

        private void LoadAd(Action<RxInterstitialAd> onSuccessLoaded)
        {
            var adRequest = new AdRequest();

            // send the request to load the ad.
            InterstitialAd.Load(adUnitId, adRequest,
                (ad, error) =>
                {

                    // if error is not null, the load request failed.
                    if (error != null || ad == null)
                    {
                        // Debug.LogError("interstitial ad failed to load an ad " +
                        //    "with error : " + error);

                        onFailedCallback?.Invoke(this, new(error));

                        return;
                    }

                    Debug.Log("Interstitial ad loaded with response : "
                              + ad.GetResponseInfo());

                    _interstitialAd = ad;

                    RegisterEventHandlers(ad);

                    onSuccessLoaded?.Invoke(this);
                });
        }

        /// <summary>
        /// Shows the interstitial ad.
        /// </summary>
        public void Show()
        {
            if (_interstitialAd != null && _interstitialAd.CanShowAd())
            {
                Debug.Log("Showing interstitial ad.");
                _interstitialAd.Show();
            }
            else
            {
                onFailedCallback?.Invoke(this, new("Interstitial ad is not ready yet."));
            }
        }

        public void Destroy() => DestoryAdIfNeed();

        private void DestoryAdIfNeed()
        {
            _interstitialAd?.Destroy();
            _interstitialAd = null;
        }

        private void RegisterEventHandlers(InterstitialAd interstitialAd)
        {
            // Raised when the ad is estimated to have earned money.
            interstitialAd.OnAdPaid += adValue =>
            {
                Debug.Log(String.Format("Interstitial ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            interstitialAd.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Interstitial ad recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            interstitialAd.OnAdClicked += () =>
            {
                Debug.Log("Interstitial ad was clicked.");

                onAdClickedCallback?.Invoke(this);
            };
            // Raised when an ad opened full screen content.
            interstitialAd.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Interstitial ad full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Interstitial ad full screen content closed.");
                onAdClosedCallback?.Invoke(this);
            };
            // Raised when the ad failed to open full screen content.
            interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
            {
                // Debug.LogError("Interstitial ad failed to open full screen content " +
                //    "with error : " + error);

                onFailedCallback?.Invoke(this, new("Interstitial ad failed to open full screen content " +
                        "with error : " + error));
            };
        }
    }
}