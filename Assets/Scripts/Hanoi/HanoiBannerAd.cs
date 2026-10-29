using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;

/// <summary>
/// Shows a banner ad at the bottom of the Hanoi level automatically (no button needed).
/// Separate from the city level's BannerAd, which is toggled by a button and the AdManager.
/// The banner overlays the bottom of the game picture; the game keeps its full size.
/// </summary>
public class HanoiBannerAd : MonoBehaviour
{
    [Header("Ads (same ids as CityScene)")]
    [SerializeField] string androidGameId = "800388721";
    [SerializeField] bool testMode = true;
    [SerializeField] string androidAdUnitId = "BP_Banner_Android";
    [SerializeField] BannerPosition bannerPosition = BannerPosition.BOTTOM_CENTER;

    private bool shown;

    private void Start()
    {
        HanoiAdsInit.Ensure(androidGameId, testMode, LoadBanner);
    }

    private void LoadBanner()
    {
        if (this == null) return;   // scene was left while ads were initialising
        Advertisement.Banner.SetPosition(bannerPosition);
        Advertisement.Banner.Load(androidAdUnitId, new BannerLoadOptions
        {
            loadCallback = OnBannerLoaded,
            errorCallback = OnBannerError
        });
    }

    private void OnBannerLoaded()
    {
        if (this == null) return;
        Advertisement.Banner.Show(androidAdUnitId, new BannerOptions
        {
            showCallback = () => shown = true,
            hideCallback = () => shown = false
        });
    }

    private void OnBannerError(string message)
    {
        Debug.LogWarning("Hanoi: banner failed to load: " + message);
        if (this != null) StartCoroutine(RetryAfter(15f));
    }

    private IEnumerator RetryAfter(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        LoadBanner();
    }

    private void OnDestroy()
    {
        // Don't leave the banner on screen when going back to the menu.
        if (Advertisement.isInitialized) Advertisement.Banner.Hide();
    }
}
