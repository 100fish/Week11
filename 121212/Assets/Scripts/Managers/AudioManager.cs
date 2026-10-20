using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public List<AudioSource> AudioSources = new List<AudioSource>();

    static AudioManager instance;

    public void Awake()
    {
        instance = this;

        for (var i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).GetComponent<AudioSource>() == null) continue;
            AudioSources.Add(transform.GetChild(i).GetComponent<AudioSource>());
        }
    }

    [ContextMenu("Applause")]
    public void Play_Applause()
    {
        PlayMusic(MUSIC.Applause);
    }

    public static void PlayMusic(MUSIC sound, float _pitch_randomness = 0, bool _check_playing = false, bool _stop = false)
    {
        

        int _sound = (int)sound + 11;

        if (_stop)
        {
            instance.AudioSources[_sound].Stop();
            return;
        }

        if (_check_playing && instance.AudioSources[_sound].isPlaying) return;


        if (_pitch_randomness != 0)
            instance.AudioSources[_sound].pitch = 1 + Random.Range(-_pitch_randomness, _pitch_randomness);

        print("play_msuic: " + sound);
        print("play_msuic: " + _sound);
        print("play_msuic: " + Time.timeScale);

        instance.AudioSources[_sound].Play();
    }

    public static void PlaySound(SFX sound, float _pitch_randomness = 0, bool _check_playing = false, bool _stop = false)
    {
        int _sound = (int) sound;

        if(_stop)
        {
            instance.AudioSources[_sound].Stop();
            return;
        }

        if (_check_playing && instance.AudioSources[_sound].isPlaying) return;


        if (_pitch_randomness != 0) 
            instance.AudioSources[_sound].pitch = 1 + Random.Range(-_pitch_randomness, _pitch_randomness);

        instance.AudioSources[_sound].Play();
    }
}

public enum SFX
{
    ElevatorDing,
    MainMusic,
    GunShot,
    Jump,
    DoubleJump,
    SlideRoll,
    SlideCymbol,
    Shot,
    LockOn,
    StartSlide,
    EndSlide = 10
}

public enum MUSIC
{
    Applause = 0,
    EndMusic = 1,
}


