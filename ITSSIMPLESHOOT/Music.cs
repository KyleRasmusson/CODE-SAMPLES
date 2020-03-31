using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Music : MonoBehaviour {

    [SerializeField] AudioClip[] Playlist;

    int MusicIndex;

    AudioSource source;

	void Start ()
    {
        source = GetComponent<AudioSource>();

        if (source == null) return;

        Invoke("PlayMusic", .1f);

        Invoke("adjustVolume", 0.1f);

        Invoke("BindMusic", .1f);
    }

    void BindMusic()
    {
        VolumeControl.musicChanged += adjustVolume;
    }

    void adjustVolume()
    {
        //Debug.Log("Adjust Volume Called");

        if(source)
            source.volume = VolumeControl.GetVolume(EAudioType.MUSIC);
        else
        {
            //Debug.Log("SOURCE NOT VALID");
        }
    }

    void PlayMusic()
    {
        int _nextSong = Random.Range(0, Playlist.Length - 1);

        if(_nextSong == MusicIndex)
        {
            _nextSong++;
            if(_nextSong >= Playlist.Length - 1)
            {
                _nextSong = 0;
            }
        }

        MusicIndex = _nextSong;

        source.PlayOneShot(Playlist[MusicIndex]);

        Invoke("PlayMusic", Playlist[MusicIndex].length);
    }
	
}
