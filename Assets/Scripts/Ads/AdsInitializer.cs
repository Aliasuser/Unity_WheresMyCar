using System;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdsInitializer : MonoBehaviour, IUnityAdsInitializationListener
{
    [SerializeField] string _androidGameId;
    [SerializeField] bool _testMode = true;
    private string _gameId;
    public event Action OnAdsInitialized;
    private bool notified;

    void Awake()
    {
        InitializeAds();
    }

    void Start()
    {
        // The menu/Hanoi flow may already have initialised Unity Ads, in which case Initialize() below is
        // skipped and OnInitializationComplete never fires - tell AdManager anyway so the car level's ads still load.
        if (Advertisement.isInitialized && !notified)
        {
            notified = true;
            OnAdsInitialized?.Invoke();
        }
    }

    public void InitializeAds()
    {
#if UNITY_ANDROID || UNITY_EDITOR
        _gameId = _androidGameId;
#endif

        if (!Advertisement.isInitialized && Advertisement.isSupported)
        {
            Advertisement.Initialize(_gameId, _testMode, this);
        }
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
        if (notified) return;
        notified = true;
        OnAdsInitialized?.Invoke();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.LogError($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
    }
}