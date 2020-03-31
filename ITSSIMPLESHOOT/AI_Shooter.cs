using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Shooter : AI_Base {

    WeaponComp Guns;

    [SerializeField] float ShootingDistance = 5;

    [SerializeField] float shotdirectionVariation = 1.0f;

    protected override void Initialized()
    {
        Guns = GetComponent<WeaponComp>();
    }

    protected override void AttackState()
    {

        base.AttackState();

        //Debug.Log("FIRING WEAPON");

        float DistanceToPlayer = Vector3.Distance(gameObject.transform.position, Target.transform.position);

        if (DistanceToPlayer < ShootingDistance)
        {
            //Rotate weapon in direction of player
            Vector3 relativePos = Target.transform.position - transform.position;

            // the second argument, upwards, defaults to Vector3.up
            Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.forward);

            //Adjust rotation to player with a given offset
            Vector3 _rotAdjust = rotation.eulerAngles;//Quaternion.ToEulerAngles(rotation);
            //removing potential oddities of rotation values
            _rotAdjust.x = 0;
            _rotAdjust.z = 0;
            //Add rotation offset
            _rotAdjust.y += Random.Range(-shotdirectionVariation, shotdirectionVariation);

            //Quaternion finalRot = Quaternion.Euler(_rotAdjust);

            Guns.GetWeaponAttachPoint().rotation = rotation;

            Guns.FireWeapon();
        }
    }
}