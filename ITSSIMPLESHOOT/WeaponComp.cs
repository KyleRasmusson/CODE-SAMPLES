using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponComp : MonoBehaviour
{
    Weapon CurrentWeapon;

    [SerializeField] List<GameObject> StartingWeapons;

    [SerializeField] List<Weapon> Weapons;

    public List<Weapon> GetCurrentWeapons { get { return Weapons; } }

    WeaponCooldown[] Cooldowns;

    [SerializeField] Transform WeaponAttachPoint;
    public Transform GetWeaponAttachPoint() { return WeaponAttachPoint; }

    int WeaponIndex;

    bool isFirePressed = false;

    void Awake()
    {
        Invoke("DelayStart", .25f);
    }

    void DelayStart()
    {
        //Don't update UI if not owned by a player
        Controller Player = GetComponent<Controller>();
        if (Player)
            Cooldowns = FindObjectsOfType<WeaponCooldown>();

        SpawnInitialWeapons();
    }

    void SpawnInitialWeapons()
    {
        foreach (GameObject NewWeapon in StartingWeapons)
        {
            GameObject Gun = Instantiate(NewWeapon, transform.position, transform.rotation);
            AddWeapon(Gun, null);
        }
        //Clearing array of initial weapons, we simply don't need to store those references after this point
        StartingWeapons.Clear();
    }

    public void AddWeapon(GameObject WeaponToAdd, WeaponPickup Pickup)
    {
        Weapon NewWeapon = WeaponToAdd.GetComponent<Weapon>();

        //If what we're picking up does not actually contain a weapon component, stop functionality
        if (NewWeapon == null)
        {
            if (Pickup != null)
            {
                Pickup.CleanupPickup(false);
            }
            return;
        }

        //Get Weapon Name
        string WeaponName = NewWeapon.GetWeaponName();

        //Checks to see if we're trying to pick up a weapon we already own
        foreach (Weapon WeaponCheck in Weapons)
        {
            if (WeaponCheck.GetWeaponName() == WeaponName)
            {
                if (Pickup != null)
                {
                    Pickup.CleanupPickup(false);
                }

                return;
            }
        }

        //Assigning weapon owner, used for determining who dealt damage
        NewWeapon.Owner = gameObject;

        //adding to array of owned weapons, childing weapon to owner
        Weapons.Add(NewWeapon);
        WeaponToAdd.transform.parent = transform;

        //reseting position and rotation
        WeaponToAdd.transform.rotation = new Quaternion(0, 0, 0, 0);
        WeaponToAdd.transform.position = WeaponAttachPoint.position;

        //If picked up from weapon pickup, say pickup was successful
        if (Pickup != null)
            Pickup.CleanupPickup(true);

        //If this is the first weapon we're picking up, switch to it
        if (Weapons.Count == 1)
            SwitchWeapon(true);

        UpdateCooldown(Weapons.Count - 1, NewWeapon);
    }

    //Start firing the weapon, called from the owning controller
    public void FireWeapon()
    {
        if (CurrentWeapon != null) CurrentWeapon.StartFire();

        isFirePressed = true;
    }

    //Stop firing weapon, setting input switch bool so that you can switch weapons and still fire after the switch has occurred
    public void CeaseFire(bool inputSwitch)
    {
        if (CurrentWeapon != null)
            CurrentWeapon.CeaseFire();

        if (inputSwitch)
            isFirePressed = false;
    }

    public void SwitchWeapon(bool increase)
    {
        //If we don't have any weapons to use, don't switch
        if (Weapons == null) return;

        //Stop firing while we switch our weapon
        CeaseFire(false);

        //Start reloading weapon
        if (CurrentWeapon)
            CurrentWeapon.InitiateCooldown(true);

        //Math for which weapon index we want to switch to
        WeaponIndex = increase ? WeaponIndex -= 1 : WeaponIndex += 1;

        if (WeaponIndex > Weapons.Count - 1) WeaponIndex = 0;

        if (WeaponIndex < 0) WeaponIndex = Weapons.Count - 1;

        //Updating information for our current weapon
        if(Weapons[WeaponIndex] != null)
        {
            CurrentWeapon = Weapons[WeaponIndex];
        }

        UpdateActiveWeapon();

        //If we're currently pressing the fire button, keep firing
        if (isFirePressed) FireWeapon();
    }

    public void SetWeapon(int newIndex)
    {
        //If we don't have any weapons to use, don't switch
        if (Weapons == null || newIndex == WeaponIndex) return;

        //Stop firing while we switch our weapon
        CeaseFire(false);

        //Start reloading weapon
        if (CurrentWeapon)
            CurrentWeapon.InitiateCooldown(true);

        if (Weapons[newIndex] == null)
        {
            return;
        }

        WeaponIndex = newIndex;

        if (Weapons[WeaponIndex] != null)
        {
            CurrentWeapon = Weapons[WeaponIndex];
        }

        UpdateActiveWeapon();
    }

    public void SwitchWeapon(string weaponName)
    {
        //If we don't have any weapons to use, don't switch
        if (Weapons == null) return;

        //if (weaponName == "null") return;

        //Stop firing while we switch our weapon
        CeaseFire(false);

        //Start reloading weapon
        if (CurrentWeapon)
            CurrentWeapon.InitiateCooldown(true);

        for (int index = 0; index < Weapons.Count; index++)
        {
            if (Weapons[index].name == weaponName)
            {
                WeaponIndex = index;
                break;
            }
        }

        //Updating information for our current weapon
        CurrentWeapon = Weapons[WeaponIndex];
        UpdateActiveWeapon();

        //If we're currently pressing the fire button, keep firing
        if (isFirePressed) FireWeapon();
    }

    //Updating references for UI, making sure that they have the correct weapon
    void UpdateCooldown(int weaponInt, Weapon Gun)
    {
        if (Cooldowns == null) return;

        foreach (WeaponCooldown Cool in Cooldowns)
        {
            Cool.InitializeWeapon(this, weaponInt);
        }
    }

    //Updates which image should have transparency set to full or set to null
    void UpdateActiveWeapon()
    {
        if (Cooldowns == null) return;

        foreach (WeaponCooldown Cool in Cooldowns)
        {
            Cool.ImageTransparency(WeaponIndex);
        }
    }

    //Setting infinite ammo active in every weapon, and initiate cool-down
    public void SetInfiniteAmmo(bool isInfinite)
    {
        foreach (Weapon WeaponCheck in Weapons)
        {
            WeaponCheck.hasInfiniteAmmo = isInfinite;
            WeaponCheck.InitiateCooldown(true);
        }
    }
}