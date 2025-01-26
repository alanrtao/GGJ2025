using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject credits;
    
    public void StartGame()
    {
        AudioManager.StartGameMusic();
        SceneManager.LoadScene(1);  //PLZ MAKE SURE THIS IS SET RIGHT IN THE BUILD PROFILE SETTINGS OR ELSE I CRI AAAAAAAA
    }

    public void Credits()
    {
        credits.SetActive(true);
    }

    public void HideCredits()
    {
        credits.SetActive(false);
    }

    public void Exit()
    {
        Application.Quit();
    }
}
