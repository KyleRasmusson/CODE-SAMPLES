using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Spawner : MonoBehaviour
{
    [SerializeField] GameObject EnemyToSpawn;

    [SerializeField] Transform[] SpawnPoints;

    [SerializeField] float initialDelay = 5.0f;

    [SerializeField] SpawnData[] data;

    int dataIndex = -1;

    int currentEnemyCount;

    /* Use this for initialization */
    void Start()
    {
        Invoke("UpdateData", initialDelay);
    }

    public void DecreaseEnemyCount()
    {
        currentEnemyCount--;
    }

    void SpawnEnemy()
    {
        if (currentEnemyCount >= data[dataIndex].maxEnemies)
        {
            return;
        }

        GameObject SpawnedEnemy;
        SpawnedEnemy = Instantiate(EnemyToSpawn, GetSpawnPoint(), Quaternion.identity, null);

        currentEnemyCount++;

        //set ref to spawner
        AI_Base Enemy = SpawnedEnemy.GetComponent<AI_Base>();
        if (Enemy != null)
        {
            Enemy.SetSpawner(this);
        }
    }

    Vector3 GetSpawnPoint()
    {
        float[] Distances = new float[SpawnPoints.Length - 1];

        //Get all distances between player and spawn point
        for (int i = 0; i < SpawnPoints.Length - 1; i++)
        {
            float _distance = GameManager.GetDistanceToPlayer(SpawnPoints[i].gameObject);

            Distances.SetValue(_distance, i);
        }

        float _currentDistance = 0;
        int _spawnIndex = 0;

        //Find point that is max distance between player
        for (int i = 0; i < Distances.Length - 1; i++)
        {
            float _distance = GameManager.GetDistanceToPlayer(SpawnPoints[i].gameObject);

            if(_distance > _currentDistance)
            {
                _currentDistance = _distance;
                _spawnIndex = i;
            }
        }

        return SpawnPoints[_spawnIndex].position;
    }

    void UpdateData()
    {
        if (dataIndex >= data.Length - 1)
        {
            CancelInvoke("UpdateData");
            return;
        }

        CancelInvoke("SpawnEnemy");

        dataIndex++;

        Invoke("UpdateData", data[dataIndex].timeTillUpdate);
        InvokeRepeating("SpawnEnemy", data[dataIndex].spawnRate, data[dataIndex].spawnRate);
    }
}