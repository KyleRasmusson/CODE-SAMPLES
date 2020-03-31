using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]

public class WeaponPickup : MonoBehaviour {

    [SerializeField]
    protected GameObject WeaponToAdd;

    [SerializeField] protected bool switchToNewWeapon;

    void OnTriggerEnter(Collider other)
    {
        AddWeaponToPlayer(other.gameObject);
    }

    public void CleanupPickup(bool SuccessfullPickup)
    {
        if(SuccessfullPickup == false)
            Destroy(WeaponToAdd);

        Destroy(gameObject);
    }

    public void AddWeaponToPlayer(GameObject Player)
    {
        WeaponComp WeaponComponent = Player.GetComponent<WeaponComp>();

        if (WeaponComponent == null)
        {
            return;
        }

        WeaponComponent.AddWeapon(WeaponToAdd, this);

        if (switchToNewWeapon)
            WeaponComponent.SwitchWeapon(true);
    }
}
