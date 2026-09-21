using System.Collections;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class FlyingObjectControllerScript : MonoBehaviour
{
    [HideInInspector]
    public float speed = 1f;
    public float fadeDuration = 1.5f;
    public float waveAmplitude = 25f;
    public float waveFrequency = 1f;
    private GameObjectsScript gameObjectsScript;
    private ScreenBoundaryScript screenBoundaryScript;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private bool isFadingOut = false;
    private bool isExploding = false;
    private Image image;
    private Color originalColor;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if(canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        if (image != null)
            originalColor = image.color;

        gameObjectsScript = Object.FindFirstObjectByType<GameObjectsScript>();
        screenBoundaryScript = Object.FindFirstObjectByType<ScreenBoundaryScript>();
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float time = 0f;
        while(time < fadeDuration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, time / fadeDuration);
            yield return null;

        }
        canvasGroup.alpha = 1f;
    }

    // Update is called once per frame
    private void Update()
    {
        float waveOffset = Mathf.Sin(Time.time * waveFrequency) * waveAmplitude;
        rectTransform.anchoredPosition += new Vector2(-speed * Time.deltaTime, waveOffset * Time.deltaTime);
        if(speed > 0 && transform.position.x < screenBoundaryScript.minX + 80)
        {
            StartCoroutine(FadeOutAndDestroy());
            isFadingOut = true;
        }

        if (speed < 0 && transform.position.x > screenBoundaryScript.maxX - 80 && !isFadingOut)
        {
            StartCoroutine(FadeOutAndDestroy());
            isFadingOut = true;
        }


    }

    IEnumerator FadeOutAndDestroy()
    {
        float time = 0f;
        float startAlpha = canvasGroup.alpha;
        while(time < fadeDuration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, time / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        Destroy(gameObject);
    }
}
