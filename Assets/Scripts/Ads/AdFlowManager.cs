using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.SceneManagement;

/// <summary>
/// Decides WHEN full-screen (interstitial) ads play outside the car level, and shows the
/// banner in the main menu. It creates itself when the game starts (no scene setup needed)
/// and survives scene changes.
///
///   Game launch (MainMenu)         -> interstitial + banner
///   Entering HanoiScene            -> interstitial   (banner is handled by HanoiBannerAd)
///   Back in MainMenu               -> interstitial + banner
///   CityScene (car level)          -> untouched, AdManager keeps doing its own thing;
///                                     this script only hides the menu banner there.
/// </summary>
public class AdFlowManager : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    // ---- scene names
    const string MainMenuScene = "MainMenu";
    const string HanoiScene = "HanoiScene";

    // ---- ad ids (same as CityScene)
    const string GameId = "800388721";
    const bool TestMode = true;
    const string InterstitialUnit = "BP_Interstitial_Android";
    const string BannerUnit = "BP_Banner_Android";

    /// <summary>If the ad isn't ready this many seconds after the scene opened, skip it (never pop up mid-game).</summary>
    const float MaxWaitForAd = 8f;

    private static AdFlowManager instance;

    private bool interstitialReady;
    private bool loading;
    private bool showing;
    private string pendingScene;        // scene that wants an interstitial right now
    private float pendingDeadline;
    private int lastHandledSceneHandle = -1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { instance = null; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        if (instance != null) return;
        var go = new GameObject("AdFlowManager");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<AdFlowManager>();
    }

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        HanoiAdsInit.Ensure(GameId, TestMode, () => { if (this != null) LoadInterstitial(); });
        // The very first scene was loaded before this object existed, so handle it by hand.
        HandleScene(SceneManager.GetActiveScene());
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HandleScene(scene);
    }

    // ------------------------------------------------------------------ scene logic

    private void HandleScene(Scene scene)
    {
        if (scene.handle == lastHandledSceneHandle) return;   // already handled
        lastHandledSceneHandle = scene.handle;

        if (scene.name == MainMenuScene) ShowMenuBanner();
        else HideBanner();

        if (scene.name == MainMenuScene || scene.name == HanoiScene)
        {
            pendingScene = scene.name;
            pendingDeadline = Time.realtimeSinceStartup + MaxWaitForAd;
            TryShowPending();
        }
        else
        {
            pendingScene = null;
        }
    }

    private void Update()
    {
        if (pendingScene == null) return;
        if (Time.realtimeSinceStartup > pendingDeadline) { pendingScene = null; return; }
        TryShowPending();
    }

    // ------------------------------------------------------------------ interstitial

    private void LoadInterstitial()
    {
        if (!Advertisement.isInitialized || loading || interstitialReady) return;
        loading = true;
        Advertisement.Load(InterstitialUnit, this);
    }

    private void TryShowPending()
    {
        if (pendingScene == null || showing || !interstitialReady) return;

        if (SceneManager.GetActiveScene().name != pendingScene) { pendingScene = null; return; }

        pendingScene = null;
        showing = true;
        interstitialReady = false;
        Advertisement.Show(InterstitialUnit, this);
    }

    private IEnumerator RetryLoadAfter(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        LoadInterstitial();
    }

    private void FinishShow()
    {
        showing = false;
        Time.timeScale = 1f;

        // Hanoi: don't let the ad/loading time count against the player's clock.
        if (HanoiGameManager.Instance != null) HanoiGameManager.Instance.ResetClockIfNoMoves();

        LoadInterstitial();   // preload the next one (e.g. for the trip back to the menu)
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        if (placementId != InterstitialUnit) return;
        loading = false;
        interstitialReady = true;
        TryShowPending();
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        loading = false;
        Debug.LogWarning("AdFlow: interstitial failed to load: " + error + " - " + message);
        StartCoroutine(RetryLoadAfter(10f));
    }

    public void OnUnityAdsShowStart(string placementId) { Time.timeScale = 0f; }
    public void OnUnityAdsShowClick(string placementId) { }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState state) { FinishShow(); }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.LogWarning("AdFlow: interstitial failed to show: " + error + " - " + message);
        FinishShow();
    }

    // ------------------------------------------------------------------ banner (main menu)

    private void ShowMenuBanner()
    {
        HanoiAdsInit.Ensure(GameId, TestMode, () =>
        {
            if (this == null || SceneManager.GetActiveScene().name != MainMenuScene) return;
            Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
            Advertisement.Banner.Load(BannerUnit, new BannerLoadOptions
            {
                loadCallback = () =>
                {
                    if (this != null && SceneManager.GetActiveScene().name == MainMenuScene)
                        Advertisement.Banner.Show(BannerUnit);
                },
                errorCallback = msg => Debug.LogWarning("AdFlow: menu banner failed: " + msg)
            });
        });
    }

    private void HideBanner()
    {
        if (Advertisement.isInitialized) Advertisement.Banner.Hide();
    }
}
