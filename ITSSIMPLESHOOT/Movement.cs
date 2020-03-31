using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]

public class Movement : MonoBehaviour {

    Rigidbody PhysBody;
    public Rigidbody GetRigidBody { get { return PhysBody; } }

    [Range(0.5f, 10.0f)] [SerializeField] float MoveSpeed;
    public float GetMoveSpeed { get { return MoveSpeed; } }

    [Range(1.0f, 40.0f)] [SerializeField] float boostSpeed;

    [SerializeField]
    float LaunchCooldownTime = 5;

    float LaunchCooldown;
    float launchCooldownRate = .01f;

    Vector3 launchEnd, launchStart;
    const float launchDistance = 12.0f;
    float launchRate;
    const float baseLaunchRate = .01f;
    const float launchRateMod = .03f;
    float launchAlpha;
    float distanceMod;

    bool isDashing = false;

    float invulnerabilityTime = 0.25f;

    public EventDashUsed OnDashed;

    [HideInInspector] public bool isBoosting = false;

    Controller PlayerController;

    AudioSource SFX;

    [SerializeField] AudioClip DashSound;

    void Awake()
    {
        PhysBody = GetComponent<Rigidbody>();
        if(PhysBody == null)
        {
            PhysBody = gameObject.AddComponent<Rigidbody>();
        }

        PhysBody.constraints =  RigidbodyConstraints.FreezeRotationX | 
                                RigidbodyConstraints.FreezeRotationZ |
                                RigidbodyConstraints.FreezePositionY;

        PlayerController = GetComponent<Controller>();

        SFX = GetComponent<AudioSource>();
    }

    public void Move(float x, float y)
    {
        if (isDashing) return;

        float _moveX = x * MoveSpeed;
        float _moveY = y * MoveSpeed;

        if (isBoosting)
        {
            _moveX = _moveX * boostSpeed;
            _moveY = _moveY * boostSpeed;
        }

        Vector3 _moveDirection = new Vector3(_moveX, 0, _moveY);

        Vector3 _velocity = _moveDirection;
        float maxSpeed = isBoosting == false ? MoveSpeed : boostSpeed;

        _velocity = Vector3.ClampMagnitude(_velocity, maxSpeed);

        PhysBody.velocity = _velocity;
    }

    public void LookAtMouse()
    {
        PhysBody.angularVelocity = Vector3.zero;

        Vector3 _mousePosition = Input.mousePosition;

        _mousePosition.z = transform.position.z - Camera.main.transform.position.z;
        _mousePosition = Camera.main.ScreenToWorldPoint(_mousePosition);
        _mousePosition.y = transform.position.y;

        if (_mousePosition == transform.position) return;

        Quaternion _lookRotation = Quaternion.FromToRotation(Vector3.forward, _mousePosition - transform.position);
        _lookRotation.x = 0;

        transform.rotation = _lookRotation;
    }

    public void LaunchPlayer()
    {
        if (LaunchCooldown > 0)
        {
            Debug.Log("Is on Launch Cooldown");
            return;
        }

        if (SFX)
        {
            SFX.volume = VolumeControl.GetVolume(EAudioType.EFFECT);
            SFX.PlayOneShot(DashSound);
        }

        /*
        //Launch player in direction of movement                   //Launch in direction of mouse cursor if not moving
        Vector3 launchVelocity = PhysBody.velocity.magnitude > 1 ? PhysBody.velocity.normalized * 500 : transform.forward * 500;


        PhysBody.AddForce(launchVelocity, ForceMode.Impulse);*/
        LaunchCooldown = LaunchCooldownTime;

        InvokeRepeating("AllowLaunch", launchCooldownRate, launchCooldownRate);

        PlayerController.GetHealth.EnableInvulnerability(invulnerabilityTime);

        launchStart = transform.position;
        GetLaunchPosition();
        GetLaunchRate();
        InvokeRepeating("Launch", 0, launchRate);
        isDashing = true;
    }

    void Launch()
    {
        launchAlpha += launchRate + launchRateMod;
        transform.position = Vector3.Lerp(launchStart, launchEnd, launchAlpha);

        if(launchAlpha >= 1)
        {
            launchAlpha = 0;
            CancelInvoke("Launch");
            isDashing = false;
        }
    }

    void GetLaunchPosition()
    {
        Vector3 origin = transform.position;
        //float objectSize = 1.0f;
        RaycastHit hit;

        //Launch player in direction of movement                   //Launch in direction of mouse cursor if not moving
        Vector3 launchDirection = PhysBody.velocity.magnitude > 1 ? PhysBody.velocity.normalized : transform.forward;
        //Debug.Log(PhysBody.velocity.magnitude);

        Physics.Raycast(origin, launchDirection * launchDistance, out hit, 14);
        //Debug.DrawRay(origin, launchDirection * launchDistance, Color.red, 10);

        if (hit.collider)
        {
            Vector3 _direction = hit.point - transform.position;
            _direction.Normalize();
            _direction = _direction * (hit.distance - .5f);
            launchEnd = _direction + transform.position; ;
            //distanceMod = .25f;
                //(transform.position - hit.point)*hit.distance + transform.position;
            return;
        }

        distanceMod = 0;
        launchEnd = launchDirection * launchDistance + transform.position;

        //launchEnd = hit.collider ? hit.point : (launchDirection * launchDistance + transform.position);
    }

    /*private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(launchEnd, 1);
    }*/

    void GetLaunchRate()
    {
        float distance = (transform.position - launchEnd).magnitude;/*
        float distanceDelta =  launchDistance - (distance - GetLaunchDistance());
        Debug.Log("Distance delta is " + distanceDelta);

        launchRate = baseLaunchRate * (distanceDelta / GetLaunchDistance());*/

        float alpha = distance / launchDistance;

        launchRate = Mathf.Lerp(baseLaunchRate, .002f, alpha);

        //Debug.Log("Launch rate is " + launchRate);
    }

    void AllowLaunch()
    {
        LaunchCooldown -= launchCooldownRate;

        if(LaunchCooldown <= 0)
        {
            LaunchCooldown = 0;
            CancelInvoke("AllowLaunch");
        }

        if (OnDashed != null)
            OnDashed(LaunchCooldown / LaunchCooldownTime);
    }

    float GetLaunchDistance()
    {
        return launchDistance - distanceMod;
    }
}
