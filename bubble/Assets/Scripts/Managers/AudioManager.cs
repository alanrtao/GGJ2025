using System;
using FMOD.Studio;
using FMODUnity;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    [SerializeField] private EventReference title;
    private EventInstance m_Title;
    [SerializeField] private EventReference inGame;
    private EventInstance m_InGame;

    public EventReference Footsteps;
    public EventReference HurtBubble;
    public EventReference BubblePlacement;
    public EventReference SmallBubblePop;
    public EventReference BigBubblePop;
    public EventReference BubbleLoopTemplate;

    public enum Asset
    {
        Footsteps,
        HurtBubble,
        BubblePlacement,
        SmallBubblePop,
        BigBubblePop,
        BubbleLoopTemplate
    }
    
    private void Awake()
    {
        Instance = this;
        //DontDestroyOnLoad(Instance);
    }

    public static void PlaySound(Asset sound)
    {
        Debug.Log($"PLAYING SOUND??? {sound.HumanName()}");
        FMODUnity.RuntimeManager.PlayOneShot(sound switch
        {
            Asset.Footsteps => Instance.Footsteps,
            Asset.HurtBubble => Instance.HurtBubble,
            Asset.BubblePlacement => Instance.BubblePlacement,
            Asset.SmallBubblePop => Instance.SmallBubblePop,
            Asset.BigBubblePop => Instance.BigBubblePop,
            Asset.BubbleLoopTemplate => Instance.BubbleLoopTemplate,
            _ => throw new Exception("pawoeifjaow")
        });
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_Title = FMODUnity.RuntimeManager.CreateInstance(title);
        m_InGame = FMODUnity.RuntimeManager.CreateInstance(inGame);
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(m_Title, Camera.main!.gameObject); // lol. wtf I'm going to cry :sob:
        
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            m_Title.start();
        } else
        {
            m_InGame.start();
        }

    }

    private void OnDestroy()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            m_Title.stop(STOP_MODE.ALLOWFADEOUT);
            m_Title.release();
        }
        else
        {
            m_InGame.stop(STOP_MODE.ALLOWFADEOUT);
            m_InGame.release();
        }
    }

    public static void StartGameMusic()
    {
        Instance.m_Title.stop(STOP_MODE.ALLOWFADEOUT);
        Instance.m_Title.release();
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
