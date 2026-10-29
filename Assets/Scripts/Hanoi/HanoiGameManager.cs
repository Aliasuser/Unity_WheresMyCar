using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controls the whole Tower of Hanoi level: spawns the blocks, counts steps, runs the
/// timer, decides the star rating and shows the result screen.
/// Landscape only. Add to a GameObject in HanoiScene (the builder in Assets/Hanoi/Editor does it for you).
/// </summary>
public class HanoiGameManager : MonoBehaviour
{
    public static HanoiGameManager Instance { get; private set; }

    [Header("Setup")]
    [Tooltip("Block sprites ordered from SMALLEST (0block) to BIGGEST (5block).")]
    public Sprite[] blockSprites;
    [Tooltip("How many blocks to use (takes the first N sprites). 6 = all of 0block..5block.")]
    [Range(2, 6)] public int blockCount = 6;
    [Tooltip("Rods, left to right.")]
    public HanoiRod[] rods;
    public int startRodIndex = 0;
    [Tooltip("Index of the rod the blocks must end up on: 0 = left, 1 = middle, 2 = right.")]
    public int targetRodIndex = 2;
    [Tooltip("How close (world units) the dragged block's centre must be to a rod to snap onto it.")]
    public float snapDistance = 2.0f;
    public SpriteRenderer background;

    [Header("Stars (seconds)")]
    [Tooltip("Finish at or under this time = 3 stars.")]
    public float threeStarTime = 150f;
    [Tooltip("Finish at or under this time = 2 stars. Slower than this = 1 star.")]
    public float twoStarTime = 240f;

    [Header("HUD")]
    public Text stepsText;
    public Text timeText;
    public Button menuButton;
    public Button restartButton;
    [Tooltip("Optional: the Watch Ad button that takes seconds off the timer.")]
    public HanoiRewardedAd rewardedAd;
    [Tooltip("If true the clock starts on the first grabbed block, otherwise when the scene starts.")]
    public bool startTimerOnFirstGrab = false;

    [Header("Result screen")]
    public GameObject winPanel;
    public Image[] winStars;
    public Text winTitleText;
    public Text winTimeText;
    public Text winStepsText;
    public Button winRetryButton;
    public Button winMenuButton;
    public Color starOn = Color.white;
    public Color starOff = new Color(0.15f, 0.15f, 0.15f, 0.8f);

    [Header("Sound (all optional)")]
    public AudioSource audioSource;
    public AudioClip pickUpSound;
    public AudioClip dropSound;
    public AudioClip invalidSound;
    public AudioClip winSound;

    [Header("Navigation")]
    public string mainMenuScene = "MainMenu";

    public bool CanInteract => !finished;
    public int Steps { get; private set; }
    public float Elapsed { get; private set; }

    private bool timerRunning;
    private bool finished;
    private bool clockStarted;
    private readonly System.Collections.Generic.List<HanoiBlock> blocks = new System.Collections.Generic.List<HanoiBlock>();
    private int lastScreenW, lastScreenH;

    // orientation we must restore when leaving the level
    private ScreenOrientation prevOrientation;
    private bool prevPortrait, prevPortraitUD, prevLandL, prevLandR;

    // ------------------------------------------------------------ lifecycle

    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        ForceLandscape();

        if (Camera.main != null && Camera.main.GetComponent<Physics2DRaycaster>() == null)
            Camera.main.gameObject.AddComponent<Physics2DRaycaster>();   // lets blocks receive touch/mouse drags

        if (menuButton) menuButton.onClick.AddListener(GoToMainMenu);
        if (winMenuButton) winMenuButton.onClick.AddListener(GoToMainMenu);
        if (restartButton) restartButton.onClick.AddListener(Restart);
        if (winRetryButton) winRetryButton.onClick.AddListener(Restart);

        FitCamera();
    }

    private void Start()
    {
        BuildLevel();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        RestoreOrientation();
    }

    private void Update()
    {
        if (Screen.width != lastScreenW || Screen.height != lastScreenH) FitCamera();

        if (timerRunning)
        {
            Elapsed += Time.deltaTime;
            UpdateHud();
        }
    }

    // ---------------------------------------------------------------- setup

    private void ForceLandscape()
    {
        prevOrientation = Screen.orientation;
        prevPortrait = Screen.autorotateToPortrait;
        prevPortraitUD = Screen.autorotateToPortraitUpsideDown;
        prevLandL = Screen.autorotateToLandscapeLeft;
        prevLandR = Screen.autorotateToLandscapeRight;

        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }

    private void RestoreOrientation()
    {
        Screen.autorotateToPortrait = prevPortrait;
        Screen.autorotateToPortraitUpsideDown = prevPortraitUD;
        Screen.autorotateToLandscapeLeft = prevLandL;
        Screen.autorotateToLandscapeRight = prevLandR;
        Screen.orientation = prevOrientation;
    }

    /// <summary>Shows the whole background; extra height (wide-short screens) is added above, as sky.
    /// The banner ad simply overlays the bottom of this picture.</summary>
    private void FitCamera()
    {
        lastScreenW = Screen.width; lastScreenH = Screen.height;
        Camera cam = Camera.main;
        if (cam == null || background == null) return;

        float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);
        Bounds b = background.bounds;
        float size = Mathf.Max(b.size.y * 0.5f, b.size.x / aspect * 0.5f);
        cam.orthographic = true;
        cam.orthographicSize = size;
        cam.transform.position = new Vector3(b.center.x, b.min.y + size, -10f);
    }

    private void BuildLevel()
    {
        foreach (var b in blocks) if (b) Destroy(b.gameObject);
        blocks.Clear();
        foreach (var r in rods) r.Clear();

        HanoiRod start = rods[startRodIndex];
        int n = Mathf.Min(blockCount, blockSprites.Length);

        // biggest first so it ends up at the bottom of the stack
        for (int s = n - 1; s >= 0; s--)
        {
            var go = new GameObject("Block" + s);
            go.transform.SetParent(transform, false);
            go.AddComponent<SpriteRenderer>();
            var block = go.AddComponent<HanoiBlock>();
            block.Init(s, blockSprites[s], 10 + (n - s));
            block.PlaceInstantly(start);
            blocks.Add(block);
        }

        Steps = 0;
        Elapsed = 0f;
        finished = false;
        clockStarted = !startTimerOnFirstGrab;
        timerRunning = clockStarted;
        if (winPanel) winPanel.SetActive(false);
        UpdateHud();
    }

    public void Restart()
    {
        StopAllCoroutines();
        BuildLevel();
        if (rewardedAd != null) rewardedAd.ResetUses();
    }

    /// <summary>Called after the entry ad: if the player hasn't moved yet, the time spent on the ad doesn't count.</summary>
    public void ResetClockIfNoMoves()
    {
        if (Steps == 0 && !finished) { Elapsed = 0f; UpdateHud(); }
    }

    /// <summary>Takes seconds off the timer (Watch Ad reward). The time never goes below zero.</summary>
    public void ReduceTime(float seconds)
    {
        if (finished) return;   // too late once the level is complete
        Elapsed = Mathf.Max(0f, Elapsed - seconds);
        UpdateHud();
        if (timeText != null) StartCoroutine(FlashTimeText());
    }

    private IEnumerator FlashTimeText()
    {
        Color original = timeText.color;
        timeText.color = new Color(0.1f, 0.65f, 0.2f);
        yield return new WaitForSeconds(1.2f);
        timeText.color = original;
    }

    // ----------------------------------------------------------------- rules

    /// <summary>The rod whose x is closest to <paramref name="worldX"/>, or null if none is within snapDistance.</summary>
    public HanoiRod NearestRod(float worldX)
    {
        HanoiRod best = null;
        float bestDist = snapDistance;
        foreach (var r in rods)
        {
            float d = Mathf.Abs(r.transform.position.x - worldX);
            if (d <= bestDist) { bestDist = d; best = r; }
        }
        return best;
    }

    public void NotifyFirstGrab()
    {
        if (!clockStarted) { clockStarted = true; timerRunning = true; }
    }

    /// <summary>Called by a block when a legal move has been dropped. Updates stacks and the step counter.</summary>
    public void MoveBlock(HanoiBlock block, HanoiRod from, HanoiRod to)
    {
        from.Remove(block);
        to.Push(block);
        Steps++;
        UpdateHud();

        // Won? every block is on the target rod. (The size rule guarantees the order is correct.)
        if (rods[targetRodIndex].Count == Mathf.Min(blockCount, blockSprites.Length))
        {
            finished = true;          // stop the clock + input right away
            timerRunning = false;
        }
    }

    /// <summary>Called when a block has finished sliding into place.</summary>
    public void OnBlockSettled()
    {
        if (finished && winPanel != null && !winPanel.activeSelf)
            StartCoroutine(ShowWinAfterDelay(0.35f));
    }

    public int CalculateStars(float seconds)
    {
        if (seconds <= threeStarTime) return 3;
        if (seconds <= twoStarTime) return 2;
        return 1;
    }

    private IEnumerator ShowWinAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        int stars = CalculateStars(Elapsed);
        if (winStars != null)
            for (int i = 0; i < winStars.Length; i++)
                winStars[i].color = i < stars ? starOn : starOff;

        if (winTitleText) winTitleText.text = "Level Complete!";
        if (winTimeText) winTimeText.text = "Time: " + FormatTime(Elapsed);
        if (winStepsText) winStepsText.text = "Steps: " + Steps;
        winPanel.SetActive(true);
        winPanel.transform.SetAsLastSibling();

        if (audioSource && winSound) audioSource.PlayOneShot(winSound);

        // remember the best result (handy for a level-select screen later)
        int bestStars = PlayerPrefs.GetInt("Hanoi_BestStars", 0);
        if (stars > bestStars) PlayerPrefs.SetInt("Hanoi_BestStars", stars);
        float bestTime = PlayerPrefs.GetFloat("Hanoi_BestTime", float.MaxValue);
        if (Elapsed < bestTime) PlayerPrefs.SetFloat("Hanoi_BestTime", Elapsed);
        PlayerPrefs.Save();
    }

    // -------------------------------------------------------------- UI / misc

    private void UpdateHud()
    {
        if (stepsText) stepsText.text = "Steps: " + Steps;
        if (timeText) timeText.text = "Time: " + FormatTime(Elapsed);
    }

    private static string FormatTime(float t)
    {
        int total = Mathf.FloorToInt(t);
        return string.Format("{0:00}:{1:00}", total / 60, total % 60);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        GameObjectsScript.isGameEnded = false;   // existing flag used by the other level
        SceneManager.LoadScene(mainMenuScene, LoadSceneMode.Single);
    }

    public void PlayPickUp() { if (audioSource && pickUpSound) audioSource.PlayOneShot(pickUpSound); }
    public void PlayDrop() { if (audioSource && dropSound) audioSource.PlayOneShot(dropSound); }
    public void PlayInvalid() { if (audioSource && invalidSound) audioSource.PlayOneShot(invalidSound); }
}
