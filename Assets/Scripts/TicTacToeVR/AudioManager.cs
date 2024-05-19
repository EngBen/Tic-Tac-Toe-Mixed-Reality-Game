using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; internal set; }     //Or private set

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _buttonPressAudioClip;
    [SerializeField] private AudioClip _XOrO_PrefabSpawnAudioClip;
    [SerializeField] private AudioClip _exitGameButtonAudioClip;
    [SerializeField] private float _buttonPressVolume = 0.6f;
    [SerializeField] private float _XOrO_PrefabSpawnVolume = 0.6f;

    private void Awake()
    {
        if (Instance != null)
        {
            // As long as you aren't creating multiple NetworkManager instances, throw an exception.
            // (***the current position of the callstack will stop here***)
            throw new Exception($"Detected more than one instance of {nameof(AudioManager)} on {nameof(gameObject)}!");
        }
        Instance = this;
    }
    
    void Start()
    {
        if (Instance != this){
            return; // so things don't get even more broken if this is a duplicate
        }
    }

    public void PlayButtonPressAudioClip()
    {
        PlayAudioClip(_buttonPressAudioClip, _buttonPressVolume, false);
    }
    
    public void PlayExitGameButtonPressAudioClip()
    {
        PlayAudioClip(_exitGameButtonAudioClip, _buttonPressVolume, false);
    }
    
    public void PlayXOrO_PrefabSpawnAudioClip()
    {
        PlayAudioClip(_XOrO_PrefabSpawnAudioClip, _XOrO_PrefabSpawnVolume, false);
    }


    public void PlayAudioClip(AudioClip clip, float audioVolume, bool loopAudioClip)
    {
        if(!_audioSource || !clip) return;
        _audioSource.Stop();
        _audioSource.clip = clip;
        _audioSource.volume = audioVolume;
        _audioSource.loop = loopAudioClip;
        _audioSource.Play();
        log("Playing AudioClip "+clip.name);
    }

    public void StopPlayingAudio()
    {
        _audioSource.Stop();
    }


    void Update()
    {
        
    }
    
    private void log(string logText){
        string className = this.GetType().Name;
        Debug.Log("["+className+"]  " +logText);
    }
}
