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
public class HanoiRewardedAd : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [Header("Ads (same ids as CityScene)")]
    [SerializeField] string androidGameId = "800388721";
    [SerializeField] bool testMode = true;
    [SerializeField] string androidAdUnitId = "BP_Rewarded_Android";

    [Header("Reward")]
    [Tooltip("How many seconds are taken off the timer for one watched ad.")]
    public float secondsRemoved = 40f;
    [Tooltip("How many ads can be watched in one attempt (resets on Restart).")]
    public int maxUsesPerRun = 3;

    [Header("UI")]
    public Button watchAdButton;
    public Text watchAdLabel;
    [Tooltip("Shows how many ads were watched out of the maximum, e.g. 0/3. Created under the button automatically if left empty.")]
    public Text usesText;
    [Tooltip("Colour of the counter while ads can still be watched.")]
    public Color usesAvailableColor = new Color(0.10f, 0.80f, 0.20f);   // bright green
    [Tooltip("Colour of the counter when no ads are left (3/3).")]
    public Color usesEmptyColor = new Color(0.90f, 0.15f, 0.15f);

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
            if (usesText == null) usesText = CreateUsesText();
        }
        RefreshLabel();
    }

    private void Start()
    {
        HanoiAdsInit.Ensure(androidGameId, testMode, () => { if (this != null) LoadAd(); });
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

        // "watched / allowed", e.g. 0/3 at the start of an attempt, 3/3 when no ads are left.
        if (usesText != null)
        {
            int watched = maxUsesPerRun - usesLeft;
            usesText.text = "Ads: " + watched + "/" + maxUsesPerRun;
            usesText.color = usesLeft > 0 ? usesAvailableColor : usesEmptyColor;
        }
    }

    /// <summary>Small text directly under the Watch Ad button.</summary>
    private Text CreateUsesText()
    {
        var go = new GameObject("UsesText", typeof(RectTransform), typeof(Text));
        go.transform.SetParent(watchAdButton.transform, false);

        var rt = (RectTransform)go.transform;
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -6f);
        rt.sizeDelta = new Vector2(400f, 60f);

        var t = go.GetComponent<Text>();
        if (watchAdLabel != null) t.font = watchAdLabel.font;
        if (t.font == null) t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 44;
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.raycastTarget = false;

        // dark outline so the green stays readable on the light background
        var outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
        outline.effectDistance = new Vector2(2f, -2f);
        return t;
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
            RefreshLabel();
            if (HanoiGameManager.Instance != null)
                HanoiGameManager.Instance.ReduceTime(secondsRemoved);
        }
        StartCoroutine(ReloadAfter(3f));
    }
}