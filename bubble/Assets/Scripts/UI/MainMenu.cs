using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject credits;
    
    public void StartGame()
    {
        SceneManager.LoadScene(1);
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
