using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : HealthComp {

    public EventRespawned OnRespawned;
    public EventLivesAdded OnLivesAdded;
    public EventFullDeath FullDeath;

    public GameManager Manager;

    [SerializeField]
    int Lives = 5;

    public int GetLives { get { return Lives; } }

    int maxLives;

    //[SerializeField]
    //int Shield = 5;

    //int maxShield;

    [SerializeField] float invulnerableTime = 1.0f;

    bool Invulnerable;

    [HideInInspector] public bool powerupInvulnerability = false;

    [SerializeField] GameObject LifeLostFX;

    Shield shield;

    //bool DamageOnLifeLost = true;
    float DeathDamageRadius = 10.0f;
    

    protected override void OnInitialized()
    {
        //maxShield = Shield;
        //Start off with no shield
        //Shield = 0;

        maxLives = Lives;

        shield = GetComponent<Shield>();
    }

    public override void ApplyDamage(int Damage, EDamageType Type, GameObject Instigator)
    {
        //Don't allow damage if we're in a state where we shouldn't
        if (Invulnerable || powerupInvulnerability || isDead)
        {
            return;
        }

        //We don't want to receive quick consecutive hits
        Invulnerable = true;

        Invoke("AllowDamage", 1.0f);

        //Base call to receive damage
        base.ApplyDamage(Damage, Type, Instigator);

        /*
        if (Shield > 0)
        {
            Shield = Mathf.Clamp(Shield -= Damage, 0, maxHealth);

            if (OnDamaged != null)
                OnDamaged(Damage, Health, Instigator);

            return;
        }
        */
    }

    void AllowDamage()
    {
        Invulnerable = false;
    }

    //Deprecated, not used in game but too lazy to take out
    public void ApplyShield()
    {
        //Shield = maxShield;
    }

    public void AddLives(int lives)
    {
        //Constraining amount of lives
        Lives = Mathf.Clamp(Lives += lives, 0, maxLives);

        //Calling delegate for adding lives
        if (OnLivesAdded != null)
        {
            OnLivesAdded(Lives);
        }
    }

    public override void Death(GameObject Instigator)
    {
        if (isDead)
            return;

        base.Death(Instigator);

        //Shield = 0;

        //Stop execution if we have lives to add
        if(Lives <= 0 && Health <= 0)
        {
            if (FullDeath != null)
                FullDeath();

            return;
        }

        Invulnerable = true;
        Lives--;

        LifeLost();

        //Dealing radial damage to all enemies nearby
        GameplayStatics.DealRadialDamage(DeathDamageRadius, transform.position, 100, EDamageType.ALL, gameObject);

        //Destroying all projectiles
        Collider[] bullets = Physics.OverlapSphere(transform.position, 100.0f);//, 12);
        foreach(Collider bullet in bullets)
        {
            PL_Bullet bulletscript = bullet.gameObject.GetComponent<PL_Bullet>();

            if (bulletscript && bullet.gameObject.tag == "EnemyProjectile")
            {
                bulletscript.SpawnEffects();
                Destroy(bullet.gameObject);
            }
        }

        if(OnLivesAdded != null)
            OnLivesAdded(Lives);

        Invoke("ResetHealth", invulnerableTime);
    }

    public void ResetHealth()
    {
        isDead = false;
        Health = maxHealth;
        //Shield = 0;
        Invulnerable = false;

        if (OnRespawned != null)
            OnRespawned();
    }

    void LifeLost()
    {
        GameObject FX = Instantiate(LifeLostFX, transform.position, transform.rotation);
        FX.transform.parent = null;
    }


    public void EnableInvulnerability(float duration)
    {
        if (powerupInvulnerability) return;

        powerupInvulnerability = true;
        Invoke("disableInvulnerability", duration);
    }

    void disableInvulnerability()
    {
        powerupInvulnerability = false;
    }

    public void EnableInvulnerability()
    {
        powerupInvulnerability = true;

        shield.StartShield();
    }
}
