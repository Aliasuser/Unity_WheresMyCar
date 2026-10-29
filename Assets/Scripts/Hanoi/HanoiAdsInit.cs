using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

/// <summary>
/// Initialises Unity Ads once for the Hanoi scene, however many ad scripts ask for it
/// (the rewarded button and the banner both need it, and the scene can be opened straight
/// from the main menu where nothing else initialises ads).
/// </summary>
public sealed class HanoiAdsInit : IUnityAdsInitializationListener
{
    private static readonly HanoiAdsInit instance = new HanoiAdsInit();
    private static readonly List<Action> waiting = new List<Action>();
    private static bool initializing;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { waiting.Clear(); initializing = false; }

    /// <summary>Calls <paramref name="onReady"/> as soon as Unity Ads is initialised (immediately if it already is).</summary>
    public static void Ensure(string gameId, bool testMode, Action onReady)
    {
        if (Advertisement.isInitialized) { onReady(); return; }
        if (!Advertisement.isSupported)
        {
            Debug.Log("Hanoi: Unity Ads is not supported on this platform.");
            return;
        }

        waiting.Add(onReady);
        if (initializing) return;
        initializing = true;
        Advertisement.Initialize(gameId, testMode, instance);
    }

    public void OnInitializationComplete()
    {
        initializing = false;
        var callbacks = waiting.ToArray();
        waiting.Clear();
        foreach (var cb in callbacks) cb();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        initializing = false;
        waiting.Clear();
        Debug.LogWarning("Hanoi: Unity Ads init failed: " + error + " - " + message);
    }
}
