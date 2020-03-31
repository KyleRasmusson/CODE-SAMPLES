using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : HealthComp
{
    [SerializeField]
    GameObject DeathFX;

    [SerializeField]
    MeshRenderer Shield;

    [SerializeField] GameObject NoEffect;

    protected override void OnInitialized()
    {
        //Shield.
        if (Shield)
        {
            Color newColor = Shield.material.color;
            newColor.a = 0;
            Shield.material.color = newColor;
        }

    }

    public override void Death(GameObject Instigator)
    {
        SpawnEffects();

        //Debug.Log("Death!");

        base.Death(Instigator);

        if (Instigator)
        {
            Controller Player = Instigator.GetComponent<Controller>();

            if (Player)
                Player.AddKills();
        }


        Destroy(gameObject);
    }

    public void SpawnEffects()
    {
        if(DeathFX)
            Instantiate(DeathFX, transform.position, transform.rotation);
    }

    protected override void DamageIgnored()
    {
        if (isDead) return;

        //Debug.Log("DAMAGE IGNORED");
        if (Shield)
        {
            Shield.enabled = true;
            Color newColor = Shield.material.color;
            newColor.a = 1;
            Shield.material.color = newColor;
            InvokeRepeating("ShieldAlpha", .25f, .05f);
        }

        if (NoEffect)
        {
            Vector3 _position = transform.position + new Vector3(0, .25f, 0);
            Quaternion _rotation = Quaternion.Euler(Vector3.zero);
            Instantiate(NoEffect, _position, _rotation);
        }
    }

    void HideShield()
    {

    }

    void ShieldAlpha()
    {
        if (Shield == null) return;

        Color newColor = Shield.material.color;
        newColor.a -= .1f;
        Shield.material.color = newColor;

        if (newColor.a <= 0)
        {
            CancelInvoke("ShieldAlhpa");
            Shield.enabled = false;
        }
    }
}
