using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class WinnerScript : MonoBehaviour
{
    //Sagatavo mainīgos
    public GameObject winnerBackground;
    public GameObject winnerLogo;
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

    private float startTime;
    private bool timerRunning = false;
    private float endTime;

    public void Start()
    {
        //Paslēpj uzvaras logu sākumā
        winnerBackground.SetActive(false);
        winnerLogo.SetActive(false);
        timeVictoryText.gameObject.SetActive(false);
        star1.SetActive(false);
        star2.SetActive(false);
        star3.SetActive(false);
        Texts.gameObject.SetActive(false);
        Back.gameObject.SetActive(false);
        Retry.gameObject.SetActive(false);

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
                ShowWinScreen();
            }
        }
    }

    public bool VehicleChecker() //Salīdzina mašīnas un mašīnas pozīcijas ar pareizām vietām
    {
        for (int i = 0; i < carObjects.Count; i++)
        {
            if (carObjects[i] == null || targetPos[i] == null)
                return false;

            if (Vector3.Distance(carObjects[i].transform.position, targetPos[i].transform.position) > 1f)
                return false;
        }
        return true;
    }

    void ShowWinScreen() //Parāda uzvaras logu
    {
        winnerBackground.SetActive(true);
        winnerLogo.SetActive(true);
        timeVictoryText.gameObject.SetActive(true);
        Texts.gameObject.SetActive(true);
        Back.gameObject.SetActive(true);
        Retry.gameObject.SetActive(true);

        

        if (endTime <= 60f) //Pārbauda kāds ir laiks un cik zvaigznes lietotājs iegūst
        {
            star3.SetActive(true);
        }
        else if (endTime <= 90)
        {
            star2.SetActive(true);
        }
        else
        {
            star1.SetActive(true);
        }

        audioSource.Play(); //Atskaņo uzvaras audio

    }

    string FormatTime(float secondsTime) //Formatē taimera laiku redzāmākā veidā
    {
        int totalTime = Mathf.FloorToInt(secondsTime);
        int hours = totalTime / 3600;
        int min = (totalTime % 3600)/60;
        int sec = totalTime % 60;
        return string.Format("{0:00}:{1:00}:{2:00}",hours, min, sec);
    }
}