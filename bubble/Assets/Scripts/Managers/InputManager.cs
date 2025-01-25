using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private bool m_paused;
    
    [SerializeField]
    private GameObject pauseMenu;

    private static InputManager Instance;

    [SerializeField] private float camSpeed;
    [SerializeField] private float camAccel;
    [SerializeField] private float camDecayPersistence;
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (pauseMenu != null) pauseMenu.SetActive(m_paused);
        AudioManager.SetPaused(m_paused);
    }

    /** Main input handling summary */
    private void Update()
    {
        HandlePause();
        HandleCameraMovement();
    }

    /** Input handling components begin */
    
    
    private Vector3 mvImpulse = Vector3.zero;
    // floaty cam
    void HandleCameraMovement()
    {
        Vector3 mvDir = Vector2.zero;
        if (Input.GetKey(KeyCode.A))
        {
            mvDir.x = -1;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            mvDir.x = 1;
        }
        
        if (Input.GetKey(KeyCode.W))
        {
            mvDir.y = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            mvDir.y = -1;
        }

        if (mvDir.sqrMagnitude > 0)
        {
            mvImpulse += mvDir.normalized * (camAccel * Time.deltaTime);
            if (mvImpulse.sqrMagnitude > 1)
            {
                mvImpulse.Normalize();
            }
        }
        else
        {
            mvImpulse = LerpSmooth(mvImpulse, Vector3.zero, Time.deltaTime, camDecayPersistence);
            if (mvImpulse.sqrMagnitude < 0.0001f)
            {
                mvImpulse = Vector3.zero;
            }
        }

        if (mvImpulse.sqrMagnitude > 0)
        {
            Camera.main!.transform.position += mvImpulse * camSpeed * Time.deltaTime;
        }
    }

    void HandlePause()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }
    
    /** Input handling components end */

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
        if (pauseMenu != null) pauseMenu.SetActive(m_paused);
        AudioManager.SetPaused(m_paused);
    }

    Vector3 LerpSmooth(Vector3 a, Vector3 b, float dt, float h)
    {
        // https://x.com/FreyaHolmer/status/1757836988495847568?lang=en
        return b + (a - b) * Mathf.Pow(2, -dt / h);
    }
}
