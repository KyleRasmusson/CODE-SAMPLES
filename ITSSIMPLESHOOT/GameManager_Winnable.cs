using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager_Winnable : GameManager {

    [SerializeField] int KillThreshold = 500;
    public int GetKillThreshold { get { return KillThreshold; } }

    [SerializeField] int SceneIndex = 0;

//    float SceneDelay = 5.0f;

    protected override void EnemiesChecked(int CurrentEnemies)
    {
        if(CurrentEnemies > KillThreshold)
        {
            //Invoke("LoadNewScene", SceneDelay);
            GameIsEnding = true;

            StartCoroutine(LoadNextScene());

            if (OnGameEnded != null) OnGameEnded();

            Time.timeScale = 0;
        }
    }

    void LoadNewScene()
    {
        SceneManager.LoadScene(SceneIndex);
    }

    float TimeValue = 1;

    IEnumerator LoadNextScene()
    {
        while(Time.timeScale > 0)
        {
            yield return new WaitForSecondsRealtime(0.05f);

            TimeValue -= .05f;

            if (TimeValue <= 0) TimeValue = 0;

            Time.timeScale = TimeValue;
            //Debug.Log(Time.timeScale);

            if (Time.timeScale <= 0) {
                SceneLoader.LoadWinScene();
            }
        }

    }
}