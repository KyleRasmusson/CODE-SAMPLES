using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PL_Bullet : BaseProjectile {

    protected override void OnHit(GameObject hitObject)
    {
        if (hitObject == Instigator) return;

        IDamageable Health = hitObject.GetComponent<IDamageable>();

        if (Health != null)
        {
            base.OnHit(hitObject);

            if (Health.GetVulnerability() == DamageType || Health.GetVulnerability() == EDamageType.ALL)
            {

                //Health.ApplyDamage(DamageAmount, DamageType, Instigator);
            }
            else
            {
                AI_Base Enemy = hitObject.GetComponent<AI_Base>();

                if (Enemy == null)
                    return;

                //Debug.Log("is Stunning Enemy");
                Enemy.MoveToStun();

                if (HitFX != null)
                    Instantiate(HitFX, transform.position, transform.rotation);

                //VFX.Play();
                if (SFX)
                {
                    if (SFX.enabled)
                    {
                        SFX.Play();
                    }
                }

                DisableBullet();
            }
        }
        else
        {
            //Debug.Log(hitObject.name);
            SpawnEffects();
            DisableBullet();
            //Destroy(gameObject);
        }


        //
    }
}
