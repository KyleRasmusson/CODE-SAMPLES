using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PL_GrenadeRound : BaseProjectile {

    [SerializeField] float explosionRadius = 2.0f;

    [SerializeField] float explosionForce = 5.0f;

    protected override void OnHit(GameObject hitObject)
    {
        //base.OnHit(hitObject);

        Collider[] hitObjects = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach(Collider hit in hitObjects)
        {
            IDamageable Health = hit.gameObject.GetComponent<IDamageable>();

            if(Health != null)
            {

                Health.ApplyDamage(DamageAmount, DamageType, Instigator);

                if (Health.GetVulnerability() == DamageType || Health.GetVulnerability() == EDamageType.ALL)
                {
                }
                else
                {
                    AI_Base Enemy = hit.GetComponent<AI_Base>();

                    if(Enemy != null)
                    {
                        Enemy.LaunchEnemy(hit.transform.position - transform.position, explosionForce);
                    }
                }
            }
        }

        if (HitFX != null)
            Instantiate(HitFX, transform.position, transform.rotation);

        SFX.Play();

        CameraShake.Shake(.1f, .4f);

        DisableBullet();
    }
}
