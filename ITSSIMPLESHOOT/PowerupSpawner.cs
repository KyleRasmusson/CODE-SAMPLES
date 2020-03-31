using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupSpawner : MonoBehaviour {

    [SerializeField] float TimeBetweenPowerupSpawns = 5.0f;
    [SerializeField] float initialPowerupDelay = 10.0f;

    [SerializeField] GameObject[] Powerups;

    [SerializeField] Transform[] PowerupSpawnPoints;


    GameObject LastPickup;

    int lastIndex;

	// Use this for initialization
	void Start ()
    {
        //Don't run code if we don't have anything to spawn, or anywhere to spawn
        if (Powerups.Length == 0 || PowerupSpawnPoints.Length == 0)
            return;

        InvokeRepeating("SpawnPowerup", initialPowerupDelay, TimeBetweenPowerupSpawns);
	}

    void SpawnPowerup()
    {
        //Destroy last pickup, make sure only one on map
        if (LastPickup)
            Destroy(LastPickup);

        //Get Spawn position
        int SpawnIndex = Random.Range(0, PowerupSpawnPoints.Length - 1);
        Transform SpawnPoint = PowerupSpawnPoints[SpawnIndex].transform;

        //Should probably be done in the instantiate, but I want to separate the code more
        int powerupIndex = GetPowerupIndex();

        //Instantiate and assign to be destroyed later
        LastPickup = Instantiate(Powerups[powerupIndex], SpawnPoint);
    }

    int GetPowerupIndex()
    {
        //Get Random int within array bounds
        int index = Random.Range(0, Powerups.Length - 1);

        //Make sure we don't spawn the same pickup every time
        if(index == lastIndex)
            index++;

        //Make sure we stay within array bounds if we make alterations based on last index
        if (index >= Powerups.Length)
            index = 0;

        lastIndex = index;

        return index;
    }
}
