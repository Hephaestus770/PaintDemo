using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundMixerManager : MonoBehaviour
{
    [SerializeField] AudioMixer AudioMixer;


    public void SetMasterVolume(float Value)
    {
        AudioMixer.SetFloat("MasterVolume", Mathf.Log10(Value) * 20f);
    }

    public void SetMusicVolume(float Value) 
    {
        AudioMixer.SetFloat("MusicVolume", Mathf.Log10(Value) * 20f);

    }

    public void SetSoundFX(float Value)
    {
        AudioMixer.SetFloat("SoundFXVolume", Mathf.Log10(Value) * 20f);

    }
}
