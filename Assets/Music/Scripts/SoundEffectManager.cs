using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectManager : MonoBehaviour
{
    public static SoundEffectManager Instance;

    [SerializeField] private AudioSource SoundFXObject;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;    
        }
    }

    public int RandomGen(AudioClip[] AudioClip)
    {
        int Rand = Random.Range(0, AudioClip.Length);
        return Rand;
    }

    public void PlaySoundFXClips(AudioClip[] AudioClip, Transform SpawnTransform, float Volume)
    {
        


        AudioSource AudioSource = Instantiate(SoundFXObject, SpawnTransform.position, Quaternion.identity);

        AudioSource.clip = AudioClip[RandomGen(AudioClip)];
        AudioSource.volume = Volume;
        AudioSource.Play();

        float ClipLength = AudioSource.clip.length;

        Destroy(AudioSource.gameObject, ClipLength);



    }




}
