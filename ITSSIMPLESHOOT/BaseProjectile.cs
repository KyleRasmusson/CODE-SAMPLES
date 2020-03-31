using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(ParticleSystem))]
[RequireComponent(typeof(BoxCollider))]

public class BaseProjectile : MonoBehaviour {

    [SerializeField]
    float Speed = 20;

    [SerializeField]
    protected int DamageAmount = 1;

    [SerializeField]
    protected EDamageType DamageType = EDamageType.ALL;

    [SerializeField]
    float LifeTime = 2.0f;

    Rigidbody PhysBody;

    [HideInInspector]
    public GameObject Instigator;

    protected AudioSource SFX;
    protected ParticleSystem VFX;

    BoxCollider col;

    [SerializeField]
    protected GameObject HitFX;

    void Start()
    {
        PhysBody = GetComponent<Rigidbody>();
        if(PhysBody == null)
        {
            PhysBody = gameObject.AddComponent<Rigidbody>();
        }

        PhysBody.useGravity = false;

        PhysBody.AddForce(transform.forward * Speed, ForceMode.VelocityChange);

        col = GetComponent<BoxCollider>();

        SFX = GetComponent<AudioSource>();
        VFX = GetComponent<ParticleSystem>();

        Invoke("ClearBullet", LifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        OnHit(other.gameObject);

        if(SFX)
            SFX.volume = VolumeControl.GetVolume(EAudioType.EFFECT);
    }

    protected virtual void OnHit(GameObject hitObject)
    {
        //Debug.Log(hitObject.name);
        //Don't detect owner
        if (hitObject == Instigator)
        {
            return;
        }

        IDamageable Health = hitObject.GetComponent<IDamageable>();

        if (Health == null)
        {
            Destroy(gameObject);
            return;
        }

        Health.ApplyDamage(DamageAmount, DamageType, Instigator);

        SpawnEffects();

        DisableBullet();
    }

    public void SpawnEffects()
    {

        if (HitFX)
            Instantiate(HitFX, transform.position, transform.rotation);

        if(SFX)
        {
            if(SFX.enabled)
                SFX.Play();
        }

    }

    protected void DisableBullet()
    {
        if(PhysBody)
            PhysBody.velocity = Vector3.zero;

        if(col)
            col.enabled = false;

        Destroy(gameObject, .05f);
    }

    void OnBecameInvisible()
    {
        Destroy(gameObject, 0.5f);
    }

    void ClearBullet()
    {
        SpawnEffects();
        DisableBullet();
    }
}
