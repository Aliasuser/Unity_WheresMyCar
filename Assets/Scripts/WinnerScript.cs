using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class WinnerScript : MonoBehaviour
{
    //Sagatavo mainīgos
    public GameObject winnerBackground;
    public GameObject winnerLogo;
    [Tooltip("Attēls/logo, kas tiek parādīts, kad spēlētājs ZAUDĒ (nevis uzvaras logo). Ja atstāj tukšu, tiks izmantots winnerLogo.")]
    public GameObject gameOverLogo;
    public Text timeVictoryText;
    public Text timeGameText;
    public GameObject star1;
    public GameObject star2;
    public GameObject star3;
    public Text Texts;
    public List<GameObject> carObjects = new List<GameObject>();
    public List<GameObject> targetPos = new List<GameObject>();
    public GameObject Back;
    public GameObject Retry;
    public AudioSource audioSource;

    [Header("Dzīvības")]
    [Tooltip("Cik reizes spēlētājs drīkst pazaudēt mašīnu sadursmē ar lidojošu objektu, pirms tiek parādīts zaudējuma ekrāns.")]
    public int maxLives = 3;
    private int livesLeft;

    [Tooltip("Teksta lauks HUD-ā, kas rāda atlikušās dzīvības (piem. spēles ekrāna augšā, blakus taimerim).")]
    public Text livesText;

    [Header("Zaudējuma ekrāna teksts un krāsas")]
    public string victoryMessage = "Tu uzvarēji!";
    public string gameOverMessage = "Spēle beigusies!";
    public Color victoryColor = Color.white;
    public Color gameOverColor = new Color(0.8f, 0.15f, 0.15f); // sarkanīga

    [Tooltip("Uzvaras loga fona attēls (Image komponents uz winnerBackground), kuram tiks mainīta krāsa zaudējuma gadījumā.")]
    public Image winnerBackgroundImage;

    [Tooltip("Skaņa, kas atskan, kad spēlētājs zaudē (nav obligāta).")]
    public AudioClip gameOverSound;
    [Tooltip("Skaņa, kas atskan, kad spēlētājs uzvar (nav obligāta).")]
    public AudioClip WinningSound;

    private float startTime;
    private bool timerRunning = false;
    private float endTime;
    private bool gameEnded = false;

    public void Start()
    {
        Time.timeScale = 1f; //Drošības pēc atjauno normālu laika gaitu, ja iepriekšējā scēnā tā bija sasaldēta

        //Paslēpj uzvaras logu sākumā
        winnerBackground.SetActive(false);
        winnerLogo.SetActive(false);
        if (gameOverLogo != null)
            gameOverLogo.SetActive(false);
        timeVictoryText.gameObject.SetActive(false);
        star1.SetActive(false);
        star2.SetActive(false);
        star3.SetActive(false);
        Texts.gameObject.SetActive(false);
        Back.gameObject.SetActive(false);
        Retry.gameObject.SetActive(false);

        livesLeft = maxLives; //Uzstāda sākuma dzīvību skaitu
        UpdateLivesText();

        startTime = Time.time; //Uzstāda sākuma laiku
        timerRunning = true; // Uzsāk taimeri
    }

    public void Update()
    {
        //Pārbauda vai taimeris vēl iet, tad aprēķina laiku
        if (timerRunning)
        {
            endTime = Time.time - startTime;
            timeVictoryText.text = FormatTime(endTime);
            timeGameText.text = FormatTime(endTime);
            if (VehicleChecker()) //Pārbauda vai mašīnas ir saliktas pareizās vietās, apstādinat taimeri un parāda uzvaras logu
            {
                timerRunning = false;
                ShowEndScreen(true);
            }
        }
    }

    public bool VehicleChecker() //Salīdzina mašīnas un mašīnas pozīcijas ar pareizām vietām
    {
        if (carObjects.Count == 0 || targetPos.Count == 0)
            return false; //Drošības pārbaude: ja saraksti nav aizpildīti, spēle nekad neuzskata šo par uzvaru

        for (int i = 0; i < carObjects.Count; i++)
        {
            if (carObjects[i] == null || targetPos[i] == null)
                return false;

            if (Vector3.Distance(carObjects[i].transform.position, targetPos[i].transform.position) > 1f)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Izsauc FlyingObjectControllerScript, kad mašīna tiek iznīcināta sadursmē ar lidojošu objektu.
    /// Atņem vienu dzīvību; kad dzīvības beidzas, parāda zaudējuma ekrānu.
    /// </summary>
    public void LoseLife()
    {
        if (gameEnded || !timerRunning)
            return; //Spēle jau beigusies, dzīvības vairs neskaita

        livesLeft--;
        Debug.Log("Mašīna zaudēta! Atlikušās dzīvības: " + livesLeft);
        UpdateLivesText();

        if (livesLeft <= 0)
        {
            timerRunning = false;
            ShowEndScreen(false);
        }
    }

    void ShowEndScreen(bool isVictory) //Parāda uzvaras vai zaudējuma logu
    {
        gameEnded = true;

        Time.timeScale = 0f; //Sasaldē fonu (lidojošos objektus, animācijas u.c.), kamēr rādās uzvaras/zaudējuma ekrāns

        winnerBackground.SetActive(true);

        //Parāda pareizo logo atkarībā no iznākuma
        if (isVictory)
        {
            winnerLogo.SetActive(true);
            if (gameOverLogo != null)
                gameOverLogo.SetActive(false);
        }
        else
        {
            winnerLogo.SetActive(false);
            //Ja atsevišķs zaudējuma logo nav piesaistīts, rāda uzvaras logo kā rezerves variantu
            if (gameOverLogo != null)
                gameOverLogo.SetActive(true);
            else
                winnerLogo.SetActive(true);
        }

        timeVictoryText.gameObject.SetActive(true);
        Texts.gameObject.SetActive(true);
        Back.gameObject.SetActive(true);
        Retry.gameObject.SetActive(true);

        //Iestata tekstu un krāsu atkarībā no iznākuma
        Texts.text = isVictory ? victoryMessage : gameOverMessage;
        if (winnerBackgroundImage != null)
        {
            winnerBackgroundImage.color = isVictory ? victoryColor : gameOverColor;
        }

        if (isVictory)
        {
            //Pārbauda kāds ir laiks un cik zvaigznes lietotājs iegūst
            if (endTime <= 120f) //līdz 2 minūtēm ieskaitot - 3 zvaigznes
            {
                star1.SetActive(true);
                star2.SetActive(true);
                star3.SetActive(true);
            }
            else if (endTime <= 300f) //līdz 5 minūtēm ieskaitot - 2 zvaigznes
            {
                star1.SetActive(true);
                star2.SetActive(true);
            }
            else //vairāk par 5 minūtēm - 1 zvaigzne
            {
                star1.SetActive(true);
            }

            if (audioSource != null)
                audioSource.PlayOneShot(WinningSound); //Atskaņo uzvaras audio
        }
        else
        {
            //Zaudējuma gadījumā zvaigznes paliek paslēptas (0 zvaigznes)
            if (gameOverSound != null && audioSource != null)
                audioSource.PlayOneShot(gameOverSound);
        }
    }

    void UpdateLivesText() //Atjauno dzīvību skaita tekstu HUD-ā
    {
        if (livesText != null)
            livesText.text = "Dzīvības: " + livesLeft;
    }

    string FormatTime(float secondsTime) //Formatē taimera laiku redzāmākā veidā
    {
        int totalTime = Mathf.FloorToInt(secondsTime);
        int hours = totalTime / 3600;
        int min = (totalTime % 3600) / 60;
        int sec = totalTime % 60;
        return string.Format("{0:00}:{1:00}:{2:00}", hours, min, sec);
    }
}