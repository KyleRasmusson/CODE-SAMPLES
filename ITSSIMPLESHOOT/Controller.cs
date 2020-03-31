using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Movement))]
[RequireComponent(typeof(WeaponComp))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(PowerupComp))]
[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(CameraMovement))]

public class Controller : MonoBehaviour {

    public EventKillAdded OnKillAdded;

    [SerializeField] PlayerHealth Health;
    [SerializeField] Movement Move;
    [SerializeField] CameraMovement CameraMove;
    [SerializeField] WeaponComp Weapon;
    [SerializeField] AudioSource SFX;
    [SerializeField] PowerupComp Powerup;

    public PlayerHealth GetHealth { get { return Health; } }
    public Movement GetMovement { get { return Move; } }
    public WeaponComp GetWeaponCom { get { return Weapon; } }
    public PowerupComp GetPowerup { get { return Powerup; } }

    int NumberKills;
    public int GetNumKills { get { return  NumberKills; } }

    [SerializeField]
    int[] NumKillsBeforeLife;
    int killIndex;

    int KillBeforeLifeTracker;
    public int GetNumKillsTillNewLife { get { return NumKillsBeforeLife[killIndex] - KillBeforeLifeTracker; } }

    [SerializeField]
    GameObject Mesh;

    [SerializeField] GameObject UIManager;

    bool HiddenPlayer;

	// Use this for initialization
	void Awake ()
    {
        Health = GetComponent<PlayerHealth>();
        if (Health == null)
        {
            Health = gameObject.AddComponent<PlayerHealth>();
            //Debug.LogError(name + " HealthComponent added with DEFAULT VALUES");
        }

        Move = GetComponent<Movement>();
        if (Move == null)
        {
            Move = gameObject.AddComponent<Movement>();
            //Debug.LogError(name + " Movement component added with DEFAULT VALUES");
        }

        CameraMove = GetComponent<CameraMovement>();
        if(CameraMove == null)
        {
            //Debug.LogError(name + " Camera follow script is NULL");
        }

        Weapon = GetComponent<WeaponComp>();
        if (Weapon == null)
        {
            //Debug.LogError(name + " Weapon Component script is NULL");
        }

        SFX = GetComponent<AudioSource>();
        if (SFX == null)
        {
            //Debug.LogError(name + " AudioSource is NULL");
        }
    }

    void Start()
    {
        Health.OnDamaged += Damaged;
        Health.OnDeath += StartHiding;
        Health.OnRespawned += StopHiding;

        GameObject _interface = Instantiate(UIManager, Vector3.zero, Quaternion.identity);
        MenuManager _manager = _interface.GetComponent<MenuManager>();
        if (_manager)
        {
            _manager.playerController = this;
        }
    }

    // Update is called once per frame
    void Update ()
    {
        if(Time.timeScale <= 0)
        {
            return;
        }

        DetectInput();
	}

    void FixedUpdate()
    {
        MovementInput();

        if (CameraMove != null)
        {
            CameraMove.MoveCamera();

            //Get raw float velocity of player
            float _velocity = Move.GetRigidBody.velocity.magnitude;

            float alpha = 0;

            //Check to make sure we don't divide by 0, also eliminate extra math operations
            if(_velocity > 0)
            {
                //product is velocity in a range from 0 - 1
                alpha = _velocity / Move.GetMoveSpeed;
            }

            CameraMove.ResizeCamera(alpha);
        }
    }

    void MovementInput()
    {
        if (Move != null)
        {
            Move.Move(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            Move.LookAtMouse();
        }
        else
        {
            //Debug.LogError(name + " Can Not Move: Movement Component is NULL");
        }
    }

    void DetectInput()
    {
        if (Weapon != null)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Weapon.FireWeapon();
            }

            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                Weapon.CeaseFire(true);
            }
            float MouseWheel = Input.GetAxis("Mouse ScrollWheel");

            if (MouseWheel > 0)
            {
                Weapon.SwitchWeapon(true);
            }

            if (MouseWheel < 0)
            {
                Weapon.SwitchWeapon(false);
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                Weapon.SetWeapon(0);
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                Weapon.SetWeapon(1);
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                Weapon.SetWeapon(2);
            }
        }

        if (Move != null)
        {
            if (Input.GetKeyDown(KeyCode.Mouse1))
                Move.LaunchPlayer();
        }
    }

        void Damaged(int Damage, int Health, GameObject Instigator)
    {
        //Cam.CameraShake.Shake();
        CameraShake.Shake(0.15f, 0.3f);

        SFX.volume = VolumeControl.GetVolume(EAudioType.EFFECT);
        SFX.Play();
    }

    void StartHiding(GameObject Instigator)
    {
        HiddenPlayer = false;
        InvokeRepeating("Hide", 0, 0.15f);
    }

    void StopHiding()
    {
        CancelInvoke("Hide");
        HiddenPlayer = false;
    }

    void Hide()
    {
        if (Mesh)
            Mesh.SetActive(HiddenPlayer);

        HiddenPlayer = !HiddenPlayer;
    }

    public void AddKills()
    {
        NumberKills++;
        KillBeforeLifeTracker++;

        if(KillBeforeLifeTracker >= NumKillsBeforeLife[killIndex])
        {
            Health.AddLives(1);
            KillBeforeLifeTracker = 0;

            if (killIndex < NumKillsBeforeLife.Length - 1)
            {
                killIndex++;
            }
        }

        if (OnKillAdded != null)
        {
            OnKillAdded();
        }

        GameManager.AddTotalKills();
    }
}