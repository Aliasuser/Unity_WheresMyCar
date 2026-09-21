using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneChangeScript : MonoBehaviour
{
    [Header("Skaņa")]
    [Tooltip("Klikšķa skaņa, kas atskan, nospiežot jebkuru pogu, kas izsauc šo skriptu.")]
    public AudioClip clickSound;

    public void ToLevel1() // Aizved lietotāju uz pirmo līmeni
    {
        StartCoroutine(PlaySoundThenDo(() => SceneManager.LoadScene("CityScene", LoadSceneMode.Single)));
    }
    /*
    public void ToLevel2() // Aizved lietotāju uz otro līmeni
    {
        SceneManager.LoadScene("Level2", LoadSceneMode.Single);
    }
    */
    public void ToMainMenu() // Aizved lietotāju uz sākuma ekrānu
    {
        StartCoroutine(PlaySoundThenDo(() => SceneManager.LoadScene("MainMenu", LoadSceneMode.Single)));
    }

    public void Quit() //Aizver spēli
    {
        StartCoroutine(PlaySoundThenDo(() => Application.Quit()));
    }

    private IEnumerator PlaySoundThenDo(System.Action action)
    {
        float wait = 0f;
        if (clickSound != null)
        {
            AudioSource.PlayClipAtPoint(clickSound, Camera.main != null ? Camera.main.transform.position : Vector3.zero);
            wait = clickSound.length;
        }

        if (wait > 0f)
        {
            yield return new WaitForSeconds(wait);
        }

        action();
    }

}


