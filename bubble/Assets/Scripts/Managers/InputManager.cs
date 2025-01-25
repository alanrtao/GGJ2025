using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private bool m_paused;
    
    [SerializeField]
    private GameObject pauseMenu;

    private static InputManager Instance;
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        pauseMenu.SetActive(m_paused);
        AudioManager.SetPaused(m_paused);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public static void Unpause()
    {
        if (Instance.m_paused)
        {
            Instance.TogglePause();
        }
    }

    public void TogglePause()
    {
        m_paused = !m_paused;
        Time.timeScale = m_paused ? 0 : 1;
        pauseMenu.SetActive(m_paused);
        AudioManager.SetPaused(m_paused);
    }
}
