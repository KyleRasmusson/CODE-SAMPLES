using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupComp : MonoBehaviour
{
    bool infiniteAmmo;
    bool invulnerability;
    bool boost;

    [SerializeField] float infiniteAmmoDuration = 2.0f;
    float ammoTime;

    [SerializeField] float invulnerabilityDuration = 3.0f;
    float invulnerableTime;

    [SerializeField] float boostDuration = 1.5f;
    float boostTime;

    [SerializeField] PlayerHealth Health;
    [SerializeField] WeaponComp Weapon;
    [SerializeField] Movement Move;
    //[SerializeField] Controller controller;

    [SerializeField] GameObject PowerupFX;
    [SerializeField] GameObject PowerDownFX;

    public EventPowerupUpdated UpdatePowerup;
    public EventPowerupAdded OnPowerupAdded;

    public bool GetPowerupActive
    {
        get
        {
            if (infiniteAmmo || invulnerability || boost)
                return true;

            else { return false; }
        }
    }

    public float GetInfiniteAmmoTime { get { return infiniteAmmoDuration; } }
    public float GetInvulnerabilityTime { get { return invulnerabilityDuration; } }
    public float GetSpeedBoostTime { get { return boostDuration; } }

    void Awake()
    {
        if (!Health)
            Health = GetComponent<PlayerHealth>();

        if (!Weapon)
            Weapon = GetComponent<WeaponComp>();

        if (!Move)
            Move = GetComponent<Movement>();

        //if (!controller) controller = GetComponent<Controller>();
    }

    public void EnableInfiniteAmmo()
    {
        if (infiniteAmmo) return;

        infiniteAmmo = true;

        if (OnPowerupAdded != null) OnPowerupAdded("INFINITE AMMO!");

        if (Weapon)
            Weapon.SetInfiniteAmmo(true);

        InvokeRepeating("DisableInfiniteAmmo", 0, .01f);

        SpawnEffect(PowerupFX);
    }

    public void EnableInvulnerability()
    {
        if (invulnerability) return;

        invulnerability = true;

        if (OnPowerupAdded != null) OnPowerupAdded("INVINCIBILITY!");

        if (Health)
            Health.EnableInvulnerability();

        InvokeRepeating("DisableInvulnerability", 0, .01f);

        SpawnEffect(PowerupFX);
    }

    public void EnableBoost()
    {
        if (boost) return;

        boost = true;

        if (OnPowerupAdded != null) OnPowerupAdded("BOOST!");

        //Debug.Log("Player Detected!");

        if (Move)
        {
            Move.isBoosting = true;
            //Debug.Log("BOOOOST!");
        }

        InvokeRepeating("DisableBoost", 0, .01f);

        SpawnEffect(PowerupFX);
    }

    void DisableInfiniteAmmo()
    {
        ammoTime += .01f;

        if (UpdatePowerup != null)
            UpdatePowerup(ammoTime, infiniteAmmoDuration);

        if (ammoTime >= infiniteAmmoDuration)
        {
            if (Weapon)
                Weapon.SetInfiniteAmmo(false);

            if (UpdatePowerup != null)
                UpdatePowerup(0, 1);

            infiniteAmmo = false;

            ammoTime = 0;

            CancelInvoke("DisableInfiniteAmmo");

            SpawnEffect(PowerDownFX);
        }
    }

    void DisableInvulnerability()
    {
        invulnerableTime += .01f;

        if (UpdatePowerup != null)
            UpdatePowerup(invulnerableTime, invulnerabilityDuration);

        if (invulnerableTime >= invulnerabilityDuration)
        {
            if (Health)
                Health.powerupInvulnerability = false;

            if (UpdatePowerup != null)
                UpdatePowerup(0, 1);

            invulnerability = false;

            invulnerableTime = 0;

            CancelInvoke("DisableInvulnerability");

            SpawnEffect(PowerDownFX);
        }
    }

    void DisableBoost()
    {
        boostTime += .01f;

        if (UpdatePowerup != null)
            UpdatePowerup(boostTime, boostDuration);

        if (boostTime >= boostDuration)
        {
            if (Move)
                Move.isBoosting = false;

            if (UpdatePowerup != null)
                UpdatePowerup(0, 1);

            boost = false;

            boostTime = 0;

            CancelInvoke("DisableBoost");

            SpawnEffect(PowerDownFX);
        }
    }

    void SpawnEffect(GameObject effect)
    {
        if (effect == null)
        {
            return;
        }

        Vector3 _position = transform.position;
        Quaternion _rotation = Quaternion.identity;

        Instantiate(effect, _position, _rotation);
    }

}
