using UnityEngine;

/// <summary>
/// Anim�cija main menu fonam: maš�na "brauc" pa ce�u, tuvojoties kamerai,
/// tad izzu�st un no jauna paras�s t�l�, atk�rtojot ciklu.
///
/// Novietošana:
/// 1. Main Menu scēn� (piem�ram, MainMenu.unity) izveido UI Image objektu
///    (Canvas -> UI -> Image) ar CarMenu.png sprite, tāpat kā MCQueen/MCQueenInverted.
/// 2. Uz tā paša objekta pievieno komponenti CanvasGroup (Add Component -> Canvas Group).
/// 3. Piekabini šo skriptu (CarDriveMenuScript) tam pašam objektam.
/// 4. Inspector logā pieregulē startPos/endPos/startScale/endScale vērtības,
///    skatoties Scene/Game view, lai kustība sakristu ar tavu ceļa perspektīvu.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class CarDriveMenuScript : MonoBehaviour
{
    [Header("Laiks")]
    [Tooltip("Cik sekundes ilgst viens pilns brauciens no tāluma līdz tuvumam.")]
    public float driveDuration = 3.5f;

    [Tooltip("Cik sekundes paliek necaurspīdīga pauze, pirms sākas fade-out gala punktā.")]
    public float pauseAtEnd = 0.15f;

    [Tooltip("Cik sekundes ilgst fade-out/fade-in pāreja starp cikliem.")]
    public float fadeDuration = 0.4f;

    [Header("Pozīcija (RectTransform anchoredPosition)")]
    [Tooltip("Sākuma pozīcija - tālu, pie ceļa horizonta.")]
    public Vector2 startPos = new Vector2(0f, 40f);

    [Tooltip("Beigu pozīcija - tuvu, apakšā ekrānā.")]
    public Vector2 endPos = new Vector2(0f, -380f);

    [Header("Izmērs (localScale)")]
    [Tooltip("Sākuma izmērs - maza mašīna tālumā.")]
    public float startScale = 0.15f;

    [Tooltip("Beigu izmērs - liela mašīna tuvumā.")]
    public float endScale = 1.4f;

    [Header("Neliela šūpošanās (pēc izvēles, imitē braukšanu)")]
    public bool enableSideSway = true;
    public float swayAmplitude = 8f;
    public float swayFrequency = 2.5f;

    private RectTransform rect;
    private CanvasGroup canvasGroup;

    private enum State { Driving, FadingOut, FadingIn }
    private State state = State.Driving;
    private float timer = 0f;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        ResetToStart();
    }

    void ResetToStart()
    {
        timer = 0f;
        fadeTimer = 0f;
        state = State.Driving;
        rect.anchoredPosition = startPos;
        rect.localScale = new Vector3(
            Mathf.Sign(rect.localScale.x == 0 ? 1 : rect.localScale.x) * startScale,
            startScale, 1f);
        canvasGroup.alpha = 1f;
    }

    void Update()
    {
        switch (state)
        {
            case State.Driving:
                DriveUpdate();
                break;

            case State.FadingOut:
                fadeTimer += Time.deltaTime;

                // vispirms pauze pilnā izmērā, tad fade uz 0
                if (fadeTimer <= pauseAtEnd)
                {
                    canvasGroup.alpha = 1f;
                }
                else
                {
                    float t = Mathf.Clamp01((fadeTimer - pauseAtEnd) / fadeDuration);
                    canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

                    if (t >= 1f)
                    {
                        // tagad, kamēr esam pilnībā neredzami, atiestatām pozīciju/izmēru
                        rect.anchoredPosition = startPos;
                        float sign = Mathf.Sign(rect.localScale.x == 0 ? 1 : rect.localScale.x);
                        rect.localScale = new Vector3(sign * startScale, startScale, 1f);

                        fadeTimer = 0f;
                        state = State.FadingIn;
                    }
                }
                break;

            case State.FadingIn:
                fadeTimer += Time.deltaTime;
                float tIn = Mathf.Clamp01(fadeTimer / fadeDuration);
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, tIn);

                if (tIn >= 1f)
                {
                    canvasGroup.alpha = 1f;
                    fadeTimer = 0f;
                    timer = 0f;
                    state = State.Driving;
                }
                break;
        }
    }

    private void DriveUpdate()
    {
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / driveDuration);
        float smoothT = Mathf.SmoothStep(0f, 1f, t);

        Vector2 pos = Vector2.Lerp(startPos, endPos, smoothT);

        if (enableSideSway)
        {
            pos.x += Mathf.Sin(timer * swayFrequency) * swayAmplitude * smoothT;
        }

        rect.anchoredPosition = pos;

        float scale = Mathf.Lerp(startScale, endScale, smoothT);
        float sign = Mathf.Sign(rect.localScale.x == 0 ? 1 : rect.localScale.x);
        rect.localScale = new Vector3(sign * scale, scale, 1f);

        if (t >= 1f)
        {
            // neliela pauze pilnā tuvplānā, tad sākas izgaišana
            fadeTimer = 0f;
            state = State.FadingOut;
        }
    }

    private float fadeTimer = 0f;

    /// <summary>
    /// Izsauc no Inspector konteksta izvēlnes vai cita skripta, lai uzreiz
    /// pārstartētu animāciju (noderīgi testēšanai).
    /// </summary>
    [ContextMenu("Restart Animation")]
    public void RestartAnimation()
    {
        ResetToStart();
    }
}
