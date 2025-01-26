using System;
using FMOD.Studio;
using FMODUnity;
using Unity.Properties;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField]
    private EventReference title;
    private EventInstance m_Title;
    private EventReference inGame;
    private EventInstance m_InGame;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(Instance);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_Title = FMODUnity.RuntimeManager.CreateInstance(title);
        m_InGame = FMODUnity.RuntimeManager.CreateInstance(inGame);
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(m_Title, Camera.main!.gameObject); // lol. wtf I'm going to cry :sob:
        m_Title.start();
    }

    private void OnDestroy()
    {
        m_Title.stop(STOP_MODE.ALLOWFADEOUT);
        m_Title.release();
    }

    public static void StartGameMusic()
    {
        Instance.m_Title.stop(STOP_MODE.ALLOWFADEOUT);
        Instance.m_Title.release();

        Instance.m_InGame.start();
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
