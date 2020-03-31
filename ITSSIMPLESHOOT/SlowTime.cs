using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowTime : MonoBehaviour {

    [SerializeField] float TimeMultiplier = 0.25f;
    public float GetTimeMultipler { get { return TimeMultiplier; } }

    enum ESlowTimeStatus
    {
        Refilling,
        Draining,
        ADDMORE
    }

    public void ModifyTime(bool ApplyMultiplier)
    {
        if(Time.timeScale <= 0 || GameManager.IsGameEnding())
        {
            return;
        }

        Time.timeScale = ApplyMultiplier ? TimeMultiplier : 1;
    }
}