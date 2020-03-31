using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
[RequireComponent(typeof(AudioSource))]
public class Effects : MonoBehaviour {

    [SerializeField]
    float lifeTime = 0.5f;

    [SerializeField] Vector2 PitchRange = new Vector2(0.95f, 1.05f);

    ParticleSystem VFX;
    AudioSource FX;

	// Use this for initialization
	void Start ()
    {
        VFX = GetComponent<ParticleSystem>();
        FX = GetComponent<AudioSource>();

        FX.volume = FX.volume * VolumeControl.GetVolume(EAudioType.EFFECT);

        VFX.Play();

        FX.pitch = Random.Range(PitchRange.x, PitchRange.y);
        FX.Play();

        Destroy(gameObject, lifeTime);
	}
}
