using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(NavMeshAgent))]
public class AI_Base : MonoBehaviour, IEnemy {

    NavMeshAgent NavAgent;

    protected GameObject Target;
    protected GameManager Manager;
    Enemy_Spawner Spawner;

    EnemyHealth Health;
    PlayerHealth TargetHealth;

    [SerializeField] float attackDistance = 0.75f;

    [SerializeField] float delayTime = 2.0f;
    bool isOnDelay;

    [SerializeField] float stunTime = 1.0f;
    bool isStunned;

    EAIState State = EAIState.ATTACK;

    [SerializeField] float baseSpeed = 5.0f, maxSpeed = 10.0f;

    const float speedCheckRate = 3.0f;
    const float UpdateRate = .1f;

    // Use for Dashing
    Vector3 StartPosition;
    Vector3 LaunchPosition;
    const float dashUpdateInterval = .02f;

    void Awake()
    {
        NavAgent = GetComponent<NavMeshAgent>();

        Health = GetComponent<EnemyHealth>();
        if (Health == null)
        {
            Health = gameObject.AddComponent<EnemyHealth>();
            Debug.LogError(name + " HealthComponent added with DEFAULT VALUES");
        }

        if (Target == null) Target = FindObjectOfType<PlayerHealth>().gameObject;

        if (Manager == null) Manager = FindObjectOfType<GameManager>();
    }

    private void Start()
    {
        TargetHealth = Target.GetComponent<PlayerHealth>();

        TargetHealth.OnDeath += TargetKilled;

        Health.OnDeath += Destruction;
        Health.OnDamaged += Damaged;

        if(Manager)
            Manager.OnGameEnded += Kill;

        InvokeRepeating("GetNewSpeed", 0, speedCheckRate);
        InvokeRepeating("StateCheck", 0, UpdateRate);

        Initialized();
    }

    void GetNewSpeed()
    {
        if (NavAgent == null) return;

        float speed = Mathf.Lerp(baseSpeed, maxSpeed, GameManager.GetSpeedModifier());

        NavAgent.speed = speed;
    }

    protected virtual void Initialized() { /*FOR INHERITANCE ONLY*/ }

    public void SetSpawnReferences(GameManager GM, GameObject Player)
    {
        Manager = GM;
        Target = Player;
    }

    protected virtual void Attack()
    {
        CancelInvoke();
        Health.SpawnEffects();
        TargetHealth.ApplyDamage(1, EDamageType.ALL, gameObject);
        //Health.ApplyDamage(Health.GetCurrentHealth, EDamageType.ALL, Target);
        Health.Death(Target);
    }

    void SetState(EAIState newState)
    {
        State = newState;
    }

    void StateCheck()
    {
        if (Health.GetIsDead) return;
        /*
        RaycastHit _hit;
        Physics.Raycast(transform.position, transform.forward, out _hit, 5);
        Color _color = _hit.collider ? Color.green : Color.red;
        Debug.DrawRay(transform.position, transform.forward * 5, _color, UpdateRate);
        */
        switch (State)
        {
            case EAIState.ATTACK:
                AttackState();
                break;

            case EAIState.DELAY:
                DelayState();
                break;

            case EAIState.STUN:
                StunState();
                break;
        }
    }

    protected virtual void AttackState()
    {
        Navigation();
    }

    void StunState()
    {
        if (isStunned)
            return;

        Invoke("Stun", stunTime);
        isStunned = true;
        SetDestination(gameObject);
        NavAgent.enabled = false;
    }

    void DelayState()
    {
        if (isOnDelay)
            return;

        SetDestination(gameObject);
        isOnDelay = true;
        Invoke("Delay", delayTime);

        CancelInvoke("Stun");
        isStunned = false;
    }

    void Delay()
    {
        SetState(EAIState.ATTACK);
        isOnDelay = false;
    }

    public void MoveToStun()
    {
        SetState(EAIState.STUN);
    }

    void Stun()
    {
        SetState(EAIState.ATTACK);
        isStunned = false;
        NavAgent.enabled = true;
    }

    void SetDestination(GameObject Goal)
    {
        if (!NavAgent.isOnNavMesh)
        {
            Destroy(gameObject);
            return;
        }

        NavAgent.SetDestination(Goal.transform.position);
    }

    void Navigation()
    {
        if (NavAgent.enabled == false)
        {
            return;
        }

        if(NavAgent.isOnNavMesh == false)
        {
            Kill();
            //GameplayStatics.DealDamage(gameObject, 10, EDamageType.ALL, gameObject);
            return;
        }

        if (Target == null || TargetHealth == null)
        {
            SetDestination(gameObject);
            return;
        }

        SetDestination(Target);

        float currentDistance = Vector3.Distance(transform.position, Target.transform.position);

        if (currentDistance < attackDistance)
        {
            Attack();
        }
    }

    void TargetKilled(GameObject Instigator)
    {
        SetState(EAIState.DELAY);
    }
    
    //This AI is dead
    void Kill()
    {
        CancelInvoke();

        SetDestination(gameObject);

        GameplayStatics.DealDamage(gameObject, 100, EDamageType.ALL, gameObject);
        //Health.SpawnEffects();
    }

    void OnDestroy()
    {
        if(TargetHealth)
            TargetHealth.OnDeath -= TargetKilled;

        if (Manager)
        {
            Manager.OnGameEnded -= Kill;
            Manager.EnemyCheck(false);
        }


        if (Spawner) Spawner.DecreaseEnemyCount();
    }

    public void LaunchEnemy(Vector3 Direction, float Distance)
    {
        CancelInvoke("DoLaunch");

        Distance = 10;
        Direction.Normalize();
        StartPosition = transform.position;
        LaunchPosition = (Direction * Distance) + StartPosition;
        LaunchPosition.y = transform.position.y;

        //Debug.DrawRay(transform.position, Direction * Distance, Color.red, 1f);

        float invokeRate = dashUpdateInterval;

        RaycastHit hit;
        //should probably use distance, but I don't want to.
        //overriding with random value 2f
        Distance = 2f;
        //Physics.Raycast(transform.position, Direction, out hit, Distance);
        Physics.Raycast(transform.position, Direction * Distance, out hit);
        if (hit.collider)
        {
            //movement rate
            float hitdistance = hit.collider ? hit.distance : Distance;//(transform.position - hit.point).magnitude;
            float alpha = hitdistance / Distance;

            invokeRate = Mathf.Lerp(invokeRate, .05f, alpha);

            //endpoint
            Vector3 _direction = hit.point - transform.position;
            _direction.Normalize();
            _direction = _direction * (hit.distance - .5f);
            LaunchPosition = _direction + transform.position;
        }

        //Color debugColor = hit.collider ? Color.green : Color.red;
        //Debug.DrawLine(transform.position, LaunchPosition, debugColor, 5f);

        //NavAgent.enabled = false;
        SetState(EAIState.STUN);
        StateCheck();
        alpha = 0;
        InvokeRepeating("DoLaunch", 0, invokeRate);
    }

    float alpha;
    
    void DoLaunch()
    {
        alpha += dashUpdateInterval;
        transform.position = Vector3.Lerp(StartPosition, LaunchPosition, alpha);

        if(alpha >= 1)
        {
            //NavAgent.enabled = true;
            CancelInvoke("DoLaunch");
        }
    }

    public void SetSpawner(Enemy_Spawner spawner)
    {
        if(spawner != null)
            Spawner = spawner;
    }

    void Damaged(int Damage, int CurrentHealth, GameObject Instigator)
    {
        StateCheck();
    }

    void Destruction(GameObject Instigator)
    {
        CancelInvoke();

        TargetHealth.OnDeath -= TargetKilled;

        Health.OnDeath -= Destruction;
        Health.OnDamaged -= Damaged;

        if (Manager)
            Manager.OnGameEnded -= Kill;
    }

}
