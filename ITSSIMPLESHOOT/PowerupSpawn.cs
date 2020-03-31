using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupSpawn : MonoBehaviour {

    HealthComp Health;

    float Chance = 0.20f;

    [SerializeField] GameObject[] Powerups;

    [SerializeField] float InvincibilityChance = 0.5f, InfiniteAmmoChance = 0.8f, speedBoostChance = 0.65f;

    private void Start()
    {
        if (Powerups.Length <= 0) {
            return;
        }

        Health = GetComponent<HealthComp>();

        Health.OnDeath += SpawnPickup;
    }

    void SpawnPickup(GameObject Instigator)
    {
        if (GameManager.CanSpawnPowerup() == false) return;

        float SpawnChance = Random.Range(0.0f, 1.0f);
//        Debug.Log(SpawnChance);
        if (Chance > SpawnChance) return;

        //int PowerupIndex = Random.Range(0, 2);

        Quaternion SpawnRot = Quaternion.Euler(0, 0, 0);

        Instantiate(Powerups[GetSpawnIndex()], transform.position, SpawnRot);

        GameManager.UpdatePowerupCount(true);
    }

    int GetSpawnIndex()
    {
        float invulChance = Random.Range(0, InvincibilityChance);
        float infiAmmChance = Random.Range(0, InfiniteAmmoChance);
        float speedChance = Random.Range(0, speedBoostChance);

//        Debug.Log("Invuln Chance = " + invulChance + " InfAmm chance = " + infiAmmChance + " Speed chance =  " + speedChance);

        if (invulChance > infiAmmChance && invulChance > speedChance) return 0;

        if (infiAmmChance > invulChance && infiAmmChance > speedChance) return 1;

        return 2;
    }
}