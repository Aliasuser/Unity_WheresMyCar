using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

/// <summary>
/// "Watch Ad" button for the Hanoi level: after a rewarded ad is watched to the end,
/// the level timer is reduced by <see cref="secondsRemoved"/> seconds.
///
/// This is separate from the existing RewardedAds script on purpose: that one is tied to the
/// flying objects of the city level and is hooked up through the "RewardedAdButton" tag.
/// This button deliberately has NO such tag, so AdManager leaves it alone.
/// It also initialises Unity Ads itself, because the Hanoi scene can be opened straight from
/// the main menu (where no ad objects exist).
/// </summary>
public class HanoiRewardedAd : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [Header("Ads (same ids as CityScene)")]
    [SerializeField] string androidGameId = "800388721";
    [SerializeField] bool testMode = true;
    [SerializeField] string androidAdUnitId = "BP_Rewarded_Android";

    [Header("Reward")]
    [Tooltip("How many seconds are taken off the timer for one watched ad.")]
    public float secondsRemoved = 20f;
    [Tooltip("How many ads can be watched in one attempt (resets on Restart).")]
    public int maxUsesPerRun = 3;

    [Header("UI")]
    public Button watchAdButton;
    public Text watchAdLabel;

    private bool adLoaded;
    private bool adShowing;
    private int usesLeft;
    private string adUnitId;

    private void Awake()
    {
        adUnitId = androidAdUnitId;
        usesLeft = maxUsesPerRun;
        if (watchAdButton != null)
        {
            watchAdButton.onClick.RemoveAllListeners();
            watchAdButton.onClick.AddListener(ShowAd);
            watchAdButton.interactable = false;
        }
        RefreshLabel();
    }

    private void Start()
    {
        if (Advertisement.isInitialized) LoadAd();
        else if (Advertisement.isSupported) Advertisement.Initialize(androidGameId, testMode, this);
        else Debug.Log("Hanoi: Unity Ads not supported on this platform, Watch Ad stays disabled.");
    }

    private void Update()
    {
        // The button is only usable when an ad is ready, the level is running and uses remain.
        if (watchAdButton == null) return;
        var mgr = HanoiGameManager.Instance;
        watchAdButton.interactable = adLoaded && !adShowing && usesLeft > 0 && mgr != null && mgr.CanInteract;
    }

    /// <summary>Called by HanoiGameManager when the level is restarted.</summary>
    public void ResetUses()
    {
        usesLeft = maxUsesPerRun;
        RefreshLabel();
    }

    private void RefreshLabel()
    {
        if (watchAdLabel != null)
            watchAdLabel.text = "Watch Ad  -" + Mathf.RoundToInt(secondsRemoved) + "s";
    }

    // ---------------------------------------------------------------- loading / showing

    public void LoadAd()
    {
        adLoaded = false;
        Advertisement.Load(adUnitId, this);
    }

    public void ShowAd()
    {
        var mgr = HanoiGameManager.Instance;
        if (!adLoaded || adShowing || usesLeft <= 0 || mgr == null || !mgr.CanInteract) return;

        adShowing = true;
        adLoaded = false;
        Advertisement.Show(adUnitId, this);
    }

    private IEnumerator ReloadAfter(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);   // realtime: timeScale may be 0 during an ad
        LoadAd();
    }

    // ---------------------------------------------------------------- IUnityAdsInitializationListener

    public void OnInitializationComplete() { LoadAd(); }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.LogWarning("Hanoi: Unity Ads init failed: " + error + " - " + message);
    }

    // ---------------------------------------------------------------- IUnityAdsLoadListener

    public void OnUnityAdsAdLoaded(string placementId)
    {
        if (placementId == adUnitId) adLoaded = true;
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogWarning("Hanoi: rewarded ad failed to load: " + error + " - " + message);
        StartCoroutine(ReloadAfter(10f));
    }

    // ---------------------------------------------------------------- IUnityAdsShowListener

    public void OnUnityAdsShowStart(string placementId) { Time.timeScale = 0f; }
    public void OnUnityAdsShowClick(string placementId) { }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.LogWarning("Hanoi: rewarded ad failed to show: " + error + " - " + message);
        adShowing = false;
        Time.timeScale = 1f;
        StartCoroutine(ReloadAfter(10f));
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState state)
    {
        adShowing = false;
        Time.timeScale = 1f;

        // Only a fully watched ad gives the reward (skipping gives nothing).
        if (placementId == adUnitId && state == UnityAdsShowCompletionState.COMPLETED)
        {
            usesLeft--;
            if (HanoiGameManager.Instance != null)
                HanoiGameManager.Instance.ReduceTime(secondsRemoved);
        }
        StartCoroutine(ReloadAfter(3f));
    }
}
