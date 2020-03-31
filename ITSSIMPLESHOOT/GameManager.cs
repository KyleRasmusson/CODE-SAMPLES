using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    [SerializeField]
    GameObject Player;

    PlayerHealth Health;

    public EventPlayerDeath Death;
    public EventPlayerRespawn Respawn;
    public EventGameOver OnGameOver; // Should be used for LOSS
    public EventGameOver OnGameEnded;
    public EventKillAdded OnKillAdded;

    public bool isGameOver { get { return Health.GetLives < 0; } }

    [SerializeField]
    float maxDistanceToPlayer = 1.0f;

    public float GetMaxDistanceToPlayer { get { return maxDistanceToPlayer; } }

    [SerializeField]
    int maxEnemies = 50;

    int EnemyCount;

    public bool canSpawnEnemies { get { return EnemyCount < maxEnemies; } }

    int enemiesKilled = 0;
    public int GetEnemiesKilled {  get { return enemiesKilled; } }

    public static GameManager instance;

    [SerializeField] int maxNumberPowerups = 15;

    int currentNumberPowerups;

    float PowerupSpawnCooldown = 5.0f;

    bool isOnPowerupCooldown;

    float TimeAtMaxSpeed = 400.0f; //   400/60 = 6.66 minutes, lovely

    protected bool GameIsEnding;

    void Awake()
    {
        if (instance) { Destroy(instance); }

        instance = this;
    }

    // Use this for initialization
    void Start()
    {
        if (Player == null)
        {
            Player = FindObjectOfType<Controller>().gameObject;
        }

        Health = Player.GetComponent<PlayerHealth>();

        if (Health != null)
        {
            Health.OnDeath += PlayerHasDied;
            Health.OnRespawned += PlayerRespawn;
            Health.FullDeath += PlayerTotalDeath;
        }
    }

    void PlayerHasDied(GameObject instigator)
    {
        if (Death != null)
        {
            Death();
        }
    }

    void PlayerRespawn()
    {
        if (Respawn != null)
        {
            Respawn();
        }
    }

    void PlayerTotalDeath()
    {
        if (OnGameOver != null) OnGameOver();

        if (OnGameEnded != null) OnGameEnded();

        StartCoroutine(LoadFailureScene());
    }

    public void EnemyCheck(bool increase)
    {
        if (increase)
        {
            EnemyCount++;
        }
        else
        {
            EnemyCount--;
        }
    }

    public static void AddTotalKills()
    {
        instance.enemiesKilled++;

        Data.UpdatePlayerScore(instance.enemiesKilled);

        //        Debug.Log(instance.enemiesKilled);

        instance.EnemiesChecked(instance.enemiesKilled);

        if (instance.OnKillAdded != null)
            instance.OnKillAdded();
    }

    protected virtual void EnemiesChecked(int CurrentEnemies) { /* Meant for overriding */}

    public static bool CanSpawnPowerup()
    {
        if (instance.isOnPowerupCooldown)
            return false;

        return instance.currentNumberPowerups < instance.maxNumberPowerups;
    }

    public static void UpdatePowerupCount(bool increase)
    {
        if (increase)
        {
            instance.currentNumberPowerups++;

            if (instance.isOnPowerupCooldown == false)
            {
                instance.Invoke("HaltPowerupCooldown", instance.PowerupSpawnCooldown);
                instance.isOnPowerupCooldown = true;
            }

        }
        else
        {
            instance.currentNumberPowerups--;
        }

       /* Debug.Log("Current Powerup count " + instance.currentNumberPowerups +
            "\n max power-up count " + instance.maxNumberPowerups +
            "\n can spawn power-up? " + GameManager.CanSpawnPowerup());*/
    }

    public static float GetDistanceToPlayer(GameObject objectToCheck)
    {
        return Vector3.Distance(objectToCheck.transform.position, instance.Player.transform.position);
    }

    public static float GetMinDistanceToPlayer() { return instance.maxDistanceToPlayer; }

    public static float GetSpeedModifier()
    {
        return Mathf.Clamp(Time.time / instance.TimeAtMaxSpeed, 0, 1);
    }

    public static bool IsGameEnding()
    {
        return instance.isGameOver || instance.GameIsEnding;
    }

    public static GameObject GetPlayer()
    {
        return instance.Player;
    }

    void HaltPowerupCooldown()
    {
        isOnPowerupCooldown = false;
    }

    IEnumerator LoadFailureScene()
    {
        float TimeValue = Time.timeScale;

        while (Time.timeScale > 0)
        {
            yield return new WaitForSecondsRealtime(0.05f);

            TimeValue -= .05f;

            if (TimeValue <= 0) TimeValue = 0;

            Time.timeScale = TimeValue;
            //Debug.Log(Time.timeScale);

            if (Time.timeScale <= 0)
            {

                Time.timeScale = 1;
                SceneLoader.LoadLoseScene();
            }
        }

    }
}
