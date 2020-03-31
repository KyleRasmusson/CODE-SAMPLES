using UnityEngine;

public interface IDamageable
{
    void ApplyDamage(int Damage, EDamageType Type, GameObject Instigator);

    void ApplyHealth(int Healing, GameObject Healer);

    EDamageType GetVulnerability();
}

public interface IWeapon
{
    void StartFire();

    void CeaseFire();

    bool CanUseWeapon();

    string GetWeaponName();
}

public interface IEnemy
{
    void SetSpawnReferences(GameManager GM, GameObject Player);
}