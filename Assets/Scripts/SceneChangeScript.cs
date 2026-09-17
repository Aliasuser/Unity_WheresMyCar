using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneChangeScript : MonoBehaviour
{
    public void ToLevel1() // Aizved lietotāju uz pirmo līmeni
    {
        SceneManager.LoadScene("CityScene", LoadSceneMode.Single);
    }
    /*
    public void ToLevel2() // Aizved lietotāju uz otro līmeni
    {
        SceneManager.LoadScene("Level2", LoadSceneMode.Single);
    }
    */
    public void ToMainMenu() // Aizved lietotāju uz sākuma ekrānu
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    public void Quit() //Aizver spēli
    {
        Application.Quit();
    }



}
