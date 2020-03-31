using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorColor : MonoBehaviour {

    [SerializeField] Color StartColor = Color.cyan, EndColor = Color.grey;

    float colorAlpha;
    float lerpRate = 0.05f;

    bool isIncreasing;

    PlayerHealth Health;

    float DamageFlashTime;

    float DamageFlashRate = 0.125f;

    [SerializeField] float TimeToFlashDamage = 0.5f;

    [SerializeField] Color DamageFlashOn = Color.white, DamageFlashOff = Color.red;

    bool isDamageFlashOn;

    bool isFlashingDamage;

    MeshRenderer Floor;

	// Use this for initialization
	void Start ()
    {
        if (!Health) Health = FindObjectOfType<PlayerHealth>();

        if (!Floor) Floor = GetComponent<MeshRenderer>();

        Health.OnDamaged += Damaged;

        InvokeRepeating("LerpColor", lerpRate, lerpRate);
    }

    void LerpColor()
    {
        colorAlpha = isIncreasing ? colorAlpha += lerpRate : colorAlpha -= lerpRate;

        Floor.material.color = Color.Lerp(StartColor, EndColor, colorAlpha);

        if (isIncreasing)
        {
            if(colorAlpha >= 1)
                isIncreasing = false;
        }
        else
        {
            if(colorAlpha <= 0)
                isIncreasing = true;
        }
    }
    //We don't care about the params
    void Damaged(int Damage, int CurrentHealth, GameObject Instigator)
    {
        CancelInvoke("LerpColor");

        if (isFlashingDamage) return;

        DamageFlashTime = 0;

        InvokeRepeating("DamageFlash", DamageFlashRate, DamageFlashRate);
    }

    void DamageFlash()
    {
        isFlashingDamage = true;

        isDamageFlashOn = !isDamageFlashOn;

        Floor.material.color = isDamageFlashOn ? DamageFlashOn : DamageFlashOff;

        DamageFlashTime += DamageFlashRate;

        //Debug.Log(DamageFlashTime);

//        Debug.Log(TimeToFlashDamage);

        if(DamageFlashTime >= TimeToFlashDamage)
        {
            isFlashingDamage = false;

            CancelInvoke("DamageFlash");
            InvokeRepeating("LerpColor", lerpRate, lerpRate);
        }
    }

}
