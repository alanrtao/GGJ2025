using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private Scrollbar volumeMusic;
    [SerializeField] private Scrollbar volumeSfx;
    
    private void OnEnable()
    {
        volumeMusic.value = AudioManager.GetVolumeMusic();
        volumeSfx.value = AudioManager.GetVolumeSfx();
    }

    public void Unpause()
    {
        InputManager.Unpause();
    }

    public void Exit()
    {
        SceneManager.LoadScene(0);
    }

    public void VolumeMusic(float val)
    {
        AudioManager.SetVolumeMusic(val);
    }

    public void VolumeSfx(float val)
    {
        AudioManager.SetVolumeSfx(val);
    }
}
