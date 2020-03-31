using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(ParticleSystem))]
public class Weapon : MonoBehaviour, IWeapon
{

    #region VARIABLE DECLARATION
    public EventOnWeaponCooldown OnCooldown;
    public EventOnWeaponFired OnFired;

    [HideInInspector] public GameObject Owner;

    [SerializeField] string Name = "MyWeapon";

    [SerializeField] GameObject Projectile;

    [SerializeField] int numProjectiles = 1;

    [SerializeField] float AnglePerRound = 0.25f;

    [SerializeField] float fireRate = 600;

    float RealFireRate;
    float timeTillCanFireWeapon;

    [SerializeField] float cooldownTime = 1.0f;

    [SerializeField] float lowAmmoAutoReloadThreshold = 1;

    public float GetCooldownTime { get { return cooldownTime; } }

    float cooldownRate = .05f;

    float cooldownAmt;
    public float GetCooldownAmount { get { return cooldownAmt; } }

    bool isOnCooldown;

    [Range(0, 50)] [SerializeField] int Ammo = 10;

    int maxAmmo;

    [HideInInspector] public bool hasInfiniteAmmo = false;

    [SerializeField] Transform Muzzle;

    [SerializeField] AudioClip EmptyGunSound;

    ParticleSystem VFX;
    AudioSource SFX;
    #endregion

    void Start()
    {
        RealFireRate = 60 / fireRate;

        SFX = GetComponent<AudioSource>();
        VFX = GetComponent<ParticleSystem>();

        maxAmmo = Ammo;

        //SFX.volume = SFX.volume * VolumeControl.GetVolume(EAudioType.EFFECT);
    }

    #region WEAPON FIRING

    public void StartFire()
    {
        if (CanFire()) {
            InvokeRepeating("FireWeapon", 0, RealFireRate);
        }

        else
        {
            CancelInvoke("FireWeapon");

            if (EmptyGunSound)
                SFX.PlayOneShot(EmptyGunSound);
        }
    }

    public void CeaseFire()
    {
        CancelInvoke("FireWeapon");
    }

    void FireWeapon()
    {
        timeTillCanFireWeapon = Time.time + RealFireRate;

        if (!hasInfiniteAmmo) Ammo--;

        ProjectileLogic();

        if (OnFired != null) OnFired(Ammo, maxAmmo);

        PlayWeaponSound();
        VFX.Play();

        InitiateCooldown(false);
    }
    #endregion

    #region COOLDOWNS

    //Start weapon cool-down, basically reload sequence
    public void InitiateCooldown(bool wasWeaponSwitched)
    {
        if (isOnCooldown) return; //Don't do cool-down if already on cool-down

        float curAmmo = Ammo;
        float totalAmmo = maxAmmo;

        if (curAmmo == totalAmmo) return;

        //start cool-down from current amount of ammo instead of starting from zero
        if (wasWeaponSwitched) cooldownAmt = curAmmo / totalAmmo * cooldownTime;

        if (Ammo <= 0 || (wasWeaponSwitched && curAmmo / totalAmmo < lowAmmoAutoReloadThreshold))
        {
            if (OnCooldown != null) OnCooldown();

            isOnCooldown = true;
            CeaseFire();
            InvokeRepeating("Cooldown", cooldownRate, cooldownRate);
        }
    }

    //Called via invoke repeating, works on reloading the weapon after the cool-down time has elapsed
    void Cooldown()
    {
        if (cooldownAmt >= cooldownTime)
        {
            CancelInvoke("Cooldown");
            cooldownAmt = 0;
            isOnCooldown = false;
            Ammo = maxAmmo;
        }

        cooldownAmt += cooldownRate;
    }
    #endregion

    #region PROJECTILE

    protected virtual void ProjectileLogic()
    {
        if (numProjectiles <= 1)
        {
            InstantiateBullet(Muzzle.transform.rotation);
        }
        else
        {
            //is the number of projectiles odd or even
            bool even = numProjectiles % 2 == 0;
            //Calculate the spacing per round based on odd/even number of projectiles
            int adjustedProjNum = even ? numProjectiles / 2 - numProjectiles : -(numProjectiles / 2 - 1) - 1;

            for (int currentRound = 0; currentRound < numProjectiles; currentRound++)
            {
                Vector3 adjustedRot = Muzzle.transform.rotation.eulerAngles;
                int bulletMod = adjustedProjNum + currentRound;

                adjustedRot.y += bulletMod * AnglePerRound;

                //Additional math if the projectile is even, does not work in the adjusted projectile number line
                if (even)
                    adjustedRot.y += AnglePerRound / 2;

                InstantiateBullet(Quaternion.Euler(adjustedRot));
            }
        }
    }

    //actually spawns projectile, marked as virtual in case we want to use a ray-cast or some other "projectile" type instead
    protected virtual void InstantiateBullet(Quaternion rot)
    {
        if (Projectile == null) return;

        GameObject Bullet = Instantiate(Projectile, Muzzle.position, rot);

        BaseProjectile Proj = Bullet.GetComponent<BaseProjectile>();

        Proj.Instigator = Owner;
    }
    #endregion

    #region CONDITIONS

    bool CanFire()
    {
        //Don't fire if game is paused
        if (Time.timeScale <= 0) return false;

        if (Time.time < timeTillCanFireWeapon) return false;

        //If we have infinite ammo, we don't care about any of the other conditions as they are related to ammo count
        if (hasInfiniteAmmo) return true;

        if (isOnCooldown == true) return false;

        if (Ammo <= 0) return false;

        return true;
    }

    public bool CanUseWeapon() { return isOnCooldown; }

    public string GetWeaponName() { return Name; }
    #endregion

    void PlayWeaponSound()
    {
        if (SFX.enabled == false)
            return;

        SFX.volume = VolumeControl.GetVolume(EAudioType.EFFECT);

        float basePitch = SFX.pitch;

        SFX.pitch = Random.Range(SFX.pitch - .1f, SFX.pitch + .1f);
        SFX.Play();

        SFX.pitch = basePitch;
    }
}