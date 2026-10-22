using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.Advertisements.Advertisement;

public class AdManager : MonoBehaviour
{
    public AdsInitializer adsInitializer;
    public InterstitialAd interstitialAd;
    [SerializeField] bool turnOffInterstitialAds = false;
    private bool firstAdShown = false;

    public RewardedAds rewardedAds;
    [SerializeField] bool turnOffRewardedAds = false;


    public BannerAd bannerAd;
    [SerializeField] bool turnOffBannerAds = false;
    public static AdManager Instance { get; private set; }

    private bool firstSceneLoaded = false;


    private void Awake()
    {
        if (adsInitializer == null)
        {
            adsInitializer = FindFirstObjectByType<AdsInitializer>();
        }

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        adsInitializer.OnAdsInitialized += HandleAdsInitialized;
    }

    private void HandleAdsInitialized()
    {
        // Pārbauda null, jo piem. MainMenu scēnā reklāmu objektu (un pogu) nav
        if (!turnOffInterstitialAds && interstitialAd != null)
        {
            interstitialAd.OnInterstitialAdReady -= HandleInterstitialReady; // novērš dubultu abonēšanu
            interstitialAd.OnInterstitialAdReady += HandleInterstitialReady;
            interstitialAd.LoadAd();
        }

        if (!turnOffRewardedAds && rewardedAds != null)
        {
            rewardedAds.LoadAd();
        }

        if (!turnOffBannerAds && bannerAd != null)
        {
            bannerAd.LoadBanner();
        }
    }

    private void HandleInterstitialReady()
    {
        if (!firstAdShown)
        {
            Debug.Log("Showing interstitial ad for the first time!");
            interstitialAd.ShowAd();
            firstAdShown = true;

        }
        else
        {
            Debug.Log("Interstitial ad is ready to be shown again!");
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Atrod pogu pēc tag. Ja tādas scēnā nav (piem. MainMenu), atgriež null, nevis izmet kļūdu
    private Button FindButtonByTag(string tag)
    {
        GameObject obj = GameObject.FindGameObjectWithTag(tag);
        return obj != null ? obj.GetComponent<Button>() : null;
    }

    // Padara visas reklāmu pogas neaktīvas (izsauc WinnerScript, kad spēle beidzas)
    public void DisableAdButtons()
    {
        SetButtonInteractable("InterstitialAdButton", false);
        SetButtonInteractable("RewardedAdButton", false);
        SetButtonInteractable("BannerAdButton", false);
    }

    private void SetButtonInteractable(string tag, bool value)
    {
        Button button = FindButtonByTag(tag);
        if (button != null)
            button.interactable = value;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (interstitialAd == null)
            interstitialAd = FindFirstObjectByType<InterstitialAd>();

        Button interstitialButton = FindButtonByTag("InterstitialAdButton");
        if (interstitialAd != null && interstitialButton != null)
        {
            interstitialAd.SetButton(interstitialButton);
        }

        if (rewardedAds == null)
            rewardedAds = FindFirstObjectByType<RewardedAds>();

        Button rewardedAdButton = FindButtonByTag("RewardedAdButton");
        if (rewardedAds != null && rewardedAdButton != null)
        {
            rewardedAds.SetButton(rewardedAdButton);
        }

        if (bannerAd == null)
            bannerAd = FindFirstObjectByType<BannerAd>();

        Button bannerAdButton = FindButtonByTag("BannerAdButton");
        if (bannerAd != null && bannerAdButton != null)
        {
            bannerAd.SetButton(bannerAdButton);
        }

        if (!firstSceneLoaded)
        {
            firstSceneLoaded = true;
            Debug.Log("First scene loaded!");
            return;
        }

        Debug.Log("Scene relodaded!");
        HandleAdsInitialized();
    }

}