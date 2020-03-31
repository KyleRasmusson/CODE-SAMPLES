using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthComp : MonoBehaviour, IDamageable {

    public EventDamaged OnDamaged;
    public EventHealed OnHealed;
    public EventDeath OnDeath;

    [SerializeField]
    EDamageType Vulnerability = EDamageType.ALL;

    public EDamageType GetVulnerability() { return Vulnerability; }

    [SerializeField] protected int Health = 5;
    public int GetCurrentHealth { get { return Health; } }

    protected int maxHealth;
    public int GetMaxHealth { get { return maxHealth; } }

    protected bool isDead;

    public bool GetIsDead { get { return isDead; } }

	// Use this for initialization
	void Awake ()
    {
        //Initialize values
        maxHealth = Health;

        //Once all other values are set, call initialize for child classes
        OnInitialized();
	}
    protected virtual void OnInitialized() { /*For Overriding Only*/ }

    public virtual void ApplyDamage(int Damage, EDamageType Type, GameObject Instigator)
    {
        //Debug.Log("Damage Checking");

        if(isDead == true || Damage <= 0 || !(Vulnerability == EDamageType.ALL || Type == Vulnerability || Type == EDamageType.ALL))
        {
            DamageIgnored();
            return;
        }

       // Debug.Log("Damage Dealing");

        Health = Mathf.Clamp(Health -= Damage, 0, maxHealth);

        if (OnDamaged != null)
            OnDamaged(Damage, Health, Instigator);

        if(Health <= 0)
        {
            Death(Instigator);
        }
    }

    protected virtual void DamageIgnored() { /*OVERRIDE ONLY, DAMAGE IGNORED*/}

    public void ApplyHealth(int Healing, GameObject Healer)
    {
        if (isDead == true || Healing <= 0 || Health == maxHealth)
        {
            return;
        }

        Health = Mathf.Clamp(Health += Health, 0, maxHealth);

        if (OnHealed != null)
            OnHealed(Healing, Health, Healer);
    }

    public virtual void Death(GameObject Instigator)
    {
        isDead = true;

        Health = 0;

        if (OnDeath != null)
            OnDeath(Instigator);
    }

}
