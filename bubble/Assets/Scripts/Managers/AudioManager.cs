using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField]
    private EventReference song;
    private EventInstance m_song;
    
    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_song = FMODUnity.RuntimeManager.CreateInstance(song);
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(m_song, Camera.main!.gameObject); // lol
        m_song.start();
    }

    private void OnDestroy()
    {
        m_song.stop(STOP_MODE.ALLOWFADEOUT);
        m_song.release();
    }

    public static void SetPaused(bool paused)
    {
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Paused", paused ? 1 : 0);
    }

    public static float GetVolumeMusic()
    {
        FMODUnity.RuntimeManager.StudioSystem.getBus("bus:/Music", out var bus);
        bus.getVolume(out var vol);
        return (vol - 0.5f) / 1.5f;
    }
    
    public static float GetVolumeSfx()
    {
        FMODUnity.RuntimeManager.StudioSystem.getBus("bus:/Sfx", out var bus);
        bus.getVolume(out var vol);
        return (vol - 0.5f) / 1.5f;;
    }

    public static void SetVolumeMusic(float val)
    {
        FMODUnity.RuntimeManager.StudioSystem.getBus("bus:/Music", out var bus);
        bus.setVolume(val * 1.5f + 0.5f);
    }
    
    public static void SetVolumeSfx(float val)
    {
        FMODUnity.RuntimeManager.StudioSystem.getBus("bus:/Sfx", out var bus);
        bus.setVolume(val * 1.5f + 0.5f);
    }
}
