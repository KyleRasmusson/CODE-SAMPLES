using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
Author:
Purpose:
Resources:
*/

public class Statics : MonoBehaviour 
{
   

}

public static class GameplayStatics
{
    public static void DealDamage(GameObject DamagedObject, int DamageAmount, EDamageType DamageType, GameObject Instigator)
    {
        HealthComp health = DamagedObject.GetComponent<HealthComp>();

        if (health)
        {
            health.ApplyDamage(DamageAmount, DamageType, Instigator);
        }
    }

    public static void DealRadialDamage(float Radius, Vector3 Position,  int Damage, EDamageType DamageType, GameObject Instigator)
    {
        Collider[] hits = Physics.OverlapSphere(Position, Radius);

        foreach(Collider hit in hits)
        {
            DealDamage(hit.gameObject, Damage, DamageType, Instigator);
        }
    }

}

public static class SceneLoader
{
    readonly static int MainMenuScene = 0;

    public readonly static int GameScene1 = 1;
    public readonly static int GameScene2 = 2;
    public readonly static int GameScene3 = 3;
    public readonly static int GameScene4 = 4;

    readonly static int WinScene = 5;
    readonly static int LoseScene = 6;

    public readonly static int TutorialScene = 7;

    public static void LoadMainMenu()
    {
        Time.timeScale = 1;
        Cursor.visible = true;
        SceneManager.LoadScene(MainMenuScene);
    }

    public static void LoadWinScene()
    {
        SceneManager.LoadScene(WinScene);
    }

    public static void LoadLoseScene()
    {
        SceneManager.LoadScene(LoseScene);
    }

    public static void LoadGameScene(int Scene)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(Scene);
    }
}

public enum EInputMode { Game, UI}
public enum EPauseMode { Pause, UnPause }
public static class InputMode
{
    public static void SetInputMode(EInputMode inputMode)
    {
        switch (inputMode)
        {
            case EInputMode.Game:
                Cursor.visible = false;
                break;

            case EInputMode.UI:
                Cursor.visible = true;
                break;
        }
    }

    public static void SetInputMode(EInputMode inputMode, EPauseMode pauseMode)
    {
        SetInputMode(inputMode);

        switch (pauseMode)
        {
            case EPauseMode.Pause:
                Time.timeScale = 0;
                break;

            case EPauseMode.UnPause:
                Time.timeScale = 1;
                break;
        }
    }
}

public static class SpawnObject
{
    public static void Spawn(GameObject objectToSpawn)
    {
        if(objectToSpawn == null)
        {
            return;
        }

        Object.Instantiate(objectToSpawn, Vector3.zero, Quaternion.identity);
    }
}

public static class Data
{
    private readonly static string saveKey = "saveKey";

    private static int HighScore = 0;

    private static int CurrentScore = 0;

    //Accessors
    public static int GetHighScore { get { return HighScore; } }
    public static int GetCurrentScore { get { return CurrentScore; } }

    public static void LoadPlayerScore()
    {
        if (PlayerPrefs.HasKey(saveKey))
        {
            HighScore = PlayerPrefs.GetInt(saveKey);
        }
    }

    public static void UpdatePlayerScore(int score)
    {
        CurrentScore = score;
        //Debug.Log(CurrentScore);

        if (!PlayerPrefs.HasKey(saveKey))
        {
            HighScore = score;
            PlayerPrefs.SetInt(saveKey, HighScore);
            return;
        }

        if(score > HighScore)
        {
            HighScore = score;
            PlayerPrefs.SetInt(saveKey, HighScore);
        }
        //Debug.Log(HighScore);
    }
}

