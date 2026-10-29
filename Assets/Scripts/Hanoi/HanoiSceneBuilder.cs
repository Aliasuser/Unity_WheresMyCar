#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// One-click builder for the Tower of Hanoi level.
/// Menu: Hanoi > Build Hanoi Scene
/// Opens Assets/Scenes/HanoiScene.unity, creates the background, rods, HUD, result screen,
/// wires everything to HanoiGameManager and adds the scene to the Build Settings.
/// Safe to run again: it rebuilds everything it created before.
/// </summary>
public static class HanoiSceneBuilder
{
    const string ScenePath = "Assets/Scenes/HanoiScene.unity";

    // Rod centres measured from towerOfHanoi.png (1280x600, 100 px per unit, centred on the origin).
    static readonly float[] RodX = { -3.935f, 0.315f, 4.56f };
    const float FloorY = -2.55f;

    static readonly Color Ink = new Color(0.17f, 0.12f, 0.10f);
    static readonly Color Maroon = new Color(0.35f, 0.07f, 0.15f);
    static readonly Color Wood = new Color(0.25f, 0.16f, 0.09f, 0.97f);

    [MenuItem("Hanoi/Build Hanoi Scene")]
    public static void Build()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        // remove what a previous run created (keep the Main Camera)
        foreach (var root in scene.GetRootGameObjects())
            if (root.name == "HanoiRoot" || root.name == "Canvas" || root.name == "EventSystem")
                Object.DestroyImmediate(root);

        // ---- camera
        Camera cam = Camera.main;
        if (cam == null)
        {
            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camGo.tag = "MainCamera";
            cam = camGo.GetComponent<Camera>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 3f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color32(239, 239, 239, 255);   // same grey-white as the sky in the background image
        cam.transform.position = new Vector3(0, 0, -10);
        if (cam.GetComponent<Physics2DRaycaster>() == null) cam.gameObject.AddComponent<Physics2DRaycaster>();

        // ---- world root
        var rootGo = new GameObject("HanoiRoot");

        var bgGo = new GameObject("Background");
        bgGo.transform.SetParent(rootGo.transform, false);
        var bg = bgGo.AddComponent<SpriteRenderer>();
        bg.sprite = LoadSprite("Assets/Sprites/towerOfHanoi.png");
        bg.sortingOrder = -10;

        var rods = new HanoiRod[3];
        for (int i = 0; i < 3; i++)
        {
            var r = new GameObject("Rod" + i);
            r.transform.SetParent(rootGo.transform, false);
            r.transform.position = new Vector3(RodX[i], 0f, 0f);
            rods[i] = r.AddComponent<HanoiRod>();
            rods[i].floorY = FloorY;
        }

        // ---- manager
        var mgr = rootGo.AddComponent<HanoiGameManager>();
        mgr.background = bg;
        mgr.rods = rods;
        mgr.startRodIndex = 0;
        mgr.targetRodIndex = 2;   // goal = right-most rod
        mgr.blockSprites = Enumerable.Range(0, 6).Select(i => LoadSprite("Assets/Sprites/" + i + "block.png")).ToArray();
        mgr.blockCount = 6;

        var audio = rootGo.AddComponent<AudioSource>();
        audio.playOnAwake = false;
        mgr.audioSource = audio;
        mgr.pickUpSound = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/click.wav");
        mgr.dropSound = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/drop.wav");
        mgr.winSound = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/Winning.mp3");

        // ---- UI
        var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        mgr.stepsText = MakeText(canvasGo.transform, "StepsText", "Steps: 0", 60, TextAnchor.MiddleRight,
            new Vector2(0.5f, 1f), new Vector2(-30, -60), new Vector2(420, 90), Ink);
        mgr.timeText = MakeText(canvasGo.transform, "TimeText", "Time: 00:00", 60, TextAnchor.MiddleLeft,
            new Vector2(0.5f, 1f), new Vector2(30, -60), new Vector2(520, 90), Ink);
        // pivots so the two texts sit either side of the centre
        mgr.stepsText.rectTransform.pivot = new Vector2(1f, 0.5f);
        mgr.timeText.rectTransform.pivot = new Vector2(0f, 0.5f);

        mgr.menuButton = MakeTextButton(canvasGo.transform, "MenuButton", "Menu", Maroon,
            new Vector2(0, 1), new Vector2(30, -30), new Vector2(240, 100));
        mgr.restartButton = MakeTextButton(canvasGo.transform, "RestartButton", "Restart", Maroon,
            new Vector2(1, 1), new Vector2(-30, -30), new Vector2(280, 100));

        // Watch Ad: green button under Menu. No "RewardedAdButton" tag on purpose (see HanoiRewardedAd).
        var adBtn = MakeTextButton(canvasGo.transform, "WatchAdButton", "Watch Ad", new Color(0.12f, 0.45f, 0.18f),
            new Vector2(0, 1), new Vector2(30, -150), new Vector2(400, 100));
        var adScript = rootGo.AddComponent<HanoiRewardedAd>();
        adScript.watchAdButton = adBtn;
        adScript.watchAdLabel = adBtn.GetComponentInChildren<Text>();
        adBtn.GetComponentInChildren<Text>().fontSize = 46;
        mgr.rewardedAd = adScript;

        BuildWinPanel(canvasGo.transform, mgr);

        // ---- event system (needed for dragging)
        var es = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
        es.AddComponent<StandaloneInputModule>();
#endif

        // ---- build settings
        var list = EditorBuildSettings.scenes.ToList();
        if (!list.Any(s => s.path == ScenePath))
        {
            list.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = list.ToArray();
        }

        EditorUtility.SetDirty(mgr);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = rootGo;
        Debug.Log("Hanoi scene built and saved. Press Play in HanoiScene to test.");
    }

    // ------------------------------------------------------------------ result screen

    static void BuildWinPanel(Transform canvas, HanoiGameManager mgr)
    {
        var overlay = MakeImage(canvas, "WinPanel", null, new Color(0, 0, 0, 0.65f));
        Stretch(overlay.rectTransform);
        mgr.winPanel = overlay.gameObject;

        var panel = MakeImage(overlay.transform, "Panel", null, Wood);
        Center(panel.rectTransform, new Vector2(1000, 760), Vector2.zero);

        mgr.winTitleText = MakeText(panel.transform, "Title", "Level Complete!", 76, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 1f), new Vector2(0, -70), new Vector2(900, 110), Color.white);

        Sprite star = LoadSprite("Assets/Sprites/OneStar.png");
        mgr.winStars = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            var s = MakeImage(panel.transform, "Star" + (i + 1), star, Color.white);
            s.preserveAspect = true;
            s.rectTransform.anchorMin = s.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            s.rectTransform.anchoredPosition = new Vector2((i - 1) * 260, 110);
            s.rectTransform.sizeDelta = new Vector2(220, 230);
            mgr.winStars[i] = s;
        }

        mgr.winTimeText = MakeText(panel.transform, "TimeResult", "Time: 00:00", 56, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(-220, -75), new Vector2(430, 80), Color.white);
        mgr.winStepsText = MakeText(panel.transform, "StepsResult", "Steps: 0", 56, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(220, -75), new Vector2(430, 80), Color.white);

        mgr.winRetryButton = MakeIconButton(panel.transform, "RetryButton", "Assets/Sprites/RetryButton.png",
            new Vector2(-170, -250));
        mgr.winMenuButton = MakeIconButton(panel.transform, "MenuButton", "Assets/Sprites/Return.png",
            new Vector2(170, -250));

        overlay.gameObject.SetActive(false);
    }

    // ------------------------------------------------------------------ small UI helpers

    static Font UiFont()
    {
        var f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }

    static Text MakeText(Transform parent, string name, string text, int size, TextAnchor align,
        Vector2 anchor, Vector2 pos, Vector2 dim, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = dim;
        var t = go.GetComponent<Text>();
        t.font = UiFont();
        t.text = text;
        t.fontSize = size;
        t.fontStyle = FontStyle.Bold;
        t.alignment = align;
        t.color = color;
        t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        return t;
    }

    static Image MakeImage(Transform parent, string name, Sprite sprite, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        return img;
    }

    static Button MakeTextButton(Transform parent, string name, string label, Color bg,
        Vector2 corner, Vector2 pos, Vector2 dim)
    {
        var img = MakeImage(parent, name, null, bg);
        var rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = corner;
        rt.pivot = corner;
        rt.anchoredPosition = pos;
        rt.sizeDelta = dim;
        var btn = img.gameObject.AddComponent<Button>();
        var t = MakeText(img.transform, "Label", label, 52, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), Vector2.zero, dim, Color.white);
        Stretch(t.rectTransform);
        return btn;
    }

    static Button MakeIconButton(Transform parent, string name, string spritePath, Vector2 pos)
    {
        // The Retry/Return icons are black glyphs, so they sit on a light button to stay readable.
        var bg = MakeImage(parent, name, null, new Color(0.97f, 0.86f, 0.55f));
        Center(bg.rectTransform, new Vector2(280, 170), pos);
        var btn = bg.gameObject.AddComponent<Button>();
        var icon = MakeImage(bg.transform, "Icon", LoadSprite(spritePath), Color.white);
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        Center(icon.rectTransform, new Vector2(120, 120), Vector2.zero);
        return btn;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static void Center(RectTransform rt, Vector2 size, Vector2 pos)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
    }

    static Sprite LoadSprite(string path)
    {
        var s = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
        if (s == null) Debug.LogWarning("Hanoi builder: sprite not found or not imported as Sprite: " + path);
        return s;
    }
}
#endif