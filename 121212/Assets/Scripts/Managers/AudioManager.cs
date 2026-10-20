using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Range(1, 8)]
    public float StartLevelMusicTime;
    static float StartLevelMusicTime_s;
    public List<AudioSource> AudioSources = new List<AudioSource>();

    public static AudioManager instance;

    public void Awake()
    {
        instance = this;

        StartLevelMusicTime_s = StartLevelMusicTime;

        for (var i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).GetComponent<AudioSource>() == null) continue;
            AudioSources.Add(transform.GetChild(i).GetComponent<AudioSource>());
        }
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

    public static void Play_LevelMusic(int _level)
    {
        if (_waiting_to_start_level_music || _playing_level_sound) return;
        print("ffffAudio: 3");
        PlayMusic(MUSIC.Applause, 0, false, true);
        PlayMusic(MUSIC.EndMusic, 0, false, true);

        int _sounds = (_level + 1) * 2;

        _time_before_level_music = StartLevelMusicTime_s;
        _waiting_to_start_level_music = true;

        PlayMusic((MUSIC)_sounds);
        level_sound = _sounds + 1;
    }

    static bool _waiting_to_start_level_music = false;
    static float _time_before_level_music = 0;

    static bool _playing_level_sound = false;
    static int level_sound = 0;
    public static float Level_Sound_Volume = 0.0f;
    public static float Level_Sound_Volume_Divide = 1;
    static float current_Level_Sound_Volume_Divide = 1;

    public static void Start_Level_Music()
    {
        print("ffffAudio: start level music");
        Level_Sound_Volume = instance.AudioSources[level_sound].volume;
        _playing_level_sound = true;
        PlayMusic((MUSIC)level_sound);
    }

    public void Update()
    {
        print("ffffAudio: 1");
        if (!_waiting_to_start_level_music) return;
        print("ffffAudio: 2");
        _time_before_level_music -= Time.deltaTime;

        if (_time_before_level_music <= 0 && !_playing_level_sound) Start_Level_Music();

        if (!_playing_level_sound) return;

        current_Level_Sound_Volume_Divide = Mathf.Lerp(current_Level_Sound_Volume_Divide, Level_Sound_Volume_Divide, 0.5f);
        current_Level_Sound_Volume_Divide = Mathf.Round(current_Level_Sound_Volume_Divide * 100) / 100;

        
        instance.AudioSources[level_sound].volume = Level_Sound_Volume * current_Level_Sound_Volume_Divide;
        print("ffffAudio: volume divide " + current_Level_Sound_Volume_Divide + ", original volume: " + Level_Sound_Volume + ", new volume: " + instance.AudioSources[level_sound].volume);

        if (current_Level_Sound_Volume_Divide <= 0) Stop_LevelSound();
    }

    public void Set_LevelMusic_Divide(float _divide) => Level_Sound_Volume_Divide = _divide;

    public void Stop_LevelSound()
    {
        instance.AudioSources[level_sound].Stop();
        level_sound = 0;
        _waiting_to_start_level_music = false;
        _playing_level_sound = false;
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
    Start_FirstSong = 2,
    FirstSong = 3
}


