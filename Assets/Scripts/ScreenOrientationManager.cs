using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// One place that decides the screen orientation per scene.
///   HanoiScene           -> landscape only (either landscape side)
///   everything else      -> follows the device: portrait by default, landscape when you rotate the phone
/// Creates itself when the game starts and survives scene changes, so no scene setup is needed.
/// </summary>
public class ScreenOrientationManager : MonoBehaviour
{
    const string LandscapeScene = "HanoiScene";

    private static ScreenOrientationManager instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { instance = null; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        if (instance != null) return;
        var go = new GameObject("ScreenOrientationManager");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<ScreenOrientationManager>();
    }

    private void Awake() { SceneManager.sceneLoaded += OnSceneLoaded; }
    private void Start() { Apply(SceneManager.GetActiveScene()); }   // first scene loaded before this object existed
    private void OnDestroy() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) { Apply(scene); }

    private void Apply(Scene scene)
    {
        StopAllCoroutines();

        if (scene.name == LandscapeScene)
        {
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.orientation = ScreenOrientation.LandscapeLeft;   // snap to landscape right away
            StartCoroutine(EnableAutoRotationNextFrame());          // then allow flipping between the two landscapes
        }
        else
        {
            // Auto-rotate between portrait and landscape; the phone's current position decides.
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.orientation = ScreenOrientation.AutoRotation;
        }
    }

    private System.Collections.IEnumerator EnableAutoRotationNextFrame()
    {
        yield return null;
        yield return null;
        Screen.orientation = ScreenOrientation.AutoRotation;   // only the landscape flags are on, so it stays landscape
    }
}