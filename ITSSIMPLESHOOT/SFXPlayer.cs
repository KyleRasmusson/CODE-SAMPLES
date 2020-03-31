using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Author:
Purpose:
Resources:
*/
[RequireComponent(typeof(AudioSource))]
public class SFXPlayer : MonoBehaviour 
{
    [SerializeField] EAudioType audioType;
    [SerializeField] AudioClip clip;

    AudioSource source;

    void Awake()
    {
        if(clip == null)
        {
            Destroy(gameObject);
            return;
        }

        source = GetComponent<AudioSource>();

        float _volume = 1;

        if (VolumeControl.instance)
        {
            _volume = VolumeControl.GetVolume(audioType);
        }

        source.volume = _volume;

        source.PlayOneShot(clip);

        StartCoroutine(DestroySound());
        //Destroy(gameObject, clip.length);
    }

    IEnumerator DestroySound()
    {
        float delaytime = clip.length;
        yield return new WaitForSecondsRealtime(delaytime);

        Destroy(gameObject);
    }

}