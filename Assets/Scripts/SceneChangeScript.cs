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
        Time.timeScale = 1f; //Atjauno normālu laika gaitu, ja spēle bija sasalusi (uzvaras/zaudējuma ekrāns)
        StartCoroutine(PlaySoundThenDo(() => { GameObjectsScript.isGameEnded = false; SceneManager.LoadScene("CityScene", LoadSceneMode.Single); }));
    }
    
    public void ToLevel2() // Aizved lietotāju uz otro līmeni
    {
        SceneManager.LoadScene("HanoiScene", LoadSceneMode.Single);
    }
    
    public void ToMainMenu() // Aizved lietotāju uz sākuma ekrānu
    {
        Time.timeScale = 1f; //Atjauno normālu laika gaitu, ja spēle bija sasalusi (uzvaras/zaudējuma ekrāns)
        StartCoroutine(PlaySoundThenDo(() => { GameObjectsScript.isGameEnded = false; SceneManager.LoadScene("MainMenu", LoadSceneMode.Single); }));
    }

    public void Quit() //Aizver spēli
    {
        Time.timeScale = 1f;
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
            //WaitForSecondsRealtime turpina skaitīt neatkarīgi no Time.timeScale,
            //tāpēc poga strādā arī tad, ja spēle bija sasalusi uzvaras/zaudējuma ekrānā
            yield return new WaitForSecondsRealtime(wait);
        }

        action();
    }

}