using UnityEngine;

public class SentinelAgent : Enemy
{
    [SerializeField]
    
    public Blackboard blackboard = new Blackboard();
    public GameObject tempestProjectile;
    public GameObject stormProjectile;
    public Transform projectileOrigin;
    public float tempestSpeed = 15.0f; // Projectile speed

    [SerializeField]
    private Transform[] _patrolCheckpoints; //Patrol points
    private int _patrolCheckpoint = 0; //Index tracks current patrol point
    private int _maxCharges = 10; //10 charge limit
    private int _currentCharges = 10;
    private int _chargeLimit = 10; //Maximum tempest charge count
    private float _elementalRechargeTime = 1f; //Generate charge every 1 second
    private float _chargeTimer = 0f; //Timer to track time since last charge gen
    private float tempestRange = 15f;
    private float stormRange = 5f;
    [SerializeField]
    private bool _stormVariant = false; //true for storm, false for tempest variant

    //Stats for logs
    private int _tpDodgeCount = 0;
    private int _hits = 0;
    private int _tempestHits = 0;
    private int _stormHits = 0;
    private int _stormCount = 0;
    private int _tempestCount = 0;
    private int _damageOutput = 0;
    private int _stateTransitions = 0;
    private int _combatStateTransitions = 0;
    private int _searchStateTransitions = 0;
    private float _logTimer = 0f;
    private bool _logCompleted = false;

    //Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        stateMachine.ChangeState(new ASentinelSearchState());
    }

    //Update is called once per frame
    protected override void Update()
    {
        base.Update();
        GenerateCharges();

        _logTimer += Time.deltaTime;
        CheckTargets();
    }

    private void GenerateCharges()
    {
        if (Time.time >= _chargeTimer + _elementalRechargeTime && _currentCharges < _chargeLimit)
        {
            _currentCharges++;
            _chargeTimer = Time.time;
        }
    }

    private void CheckTargets()
    {
        bool _noTargets = true; //reset for each cycle

        foreach (var target in targets)
        {
            if (target.currentHealth > 0)
            {
                _noTargets = false;
                break;
            }
        }

        if (_noTargets && !_logCompleted)
        {
            LogStats();
            _logCompleted = true;
        }
    }

    public override void LogStats()
    {
        Debug.Log(
                $"[Stats] ({gameObject.name})\n" +
                $"Hits: {_hits}\n" +
                $"Tempest Hits: {_tempestHits}\n" +
                $"Storm Hits: {_stormHits}\n" +
                $"Storm Uses: {_stormCount}\n" +
                $"Tempest Uses: {_tempestCount}\n" +
                $"Teleport Dodges: {_tpDodgeCount}\n" +
                $"Damage Output: {_damageOutput}\n" +
                $"State Transitions: {_stateTransitions}\n" +
                $"   - Combat Transitions: {_combatStateTransitions}\n" +
                $"   - Search Transitions: {_searchStateTransitions}\n" +
                $"Log Timer: {_logTimer:F2}s"
            );
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, tempestRange);
    }

    public bool GetSentinelVariantType()
    {
        return _stormVariant;
    }

    public int GetPatrolCheckpoint()
    {
        return _patrolCheckpoint;
    }

    public Transform[] GetPatrolCheckPoints()
    {
        return _patrolCheckpoints;
    }

    public void SetPatrolCheckpoint(int checkpoint)
    {
        _patrolCheckpoint = checkpoint;
    }

    public float GetTempestRange()
    {
        return tempestRange;
    }

    public float GetStormRange()
    {
        return stormRange;
    }

    public int GetMaxCharges()
    {
        return _maxCharges;
    }

    public int GetCharges()
    {
        return _currentCharges;
    }

    public void SetCurrentCharges(int change)
    {
        _currentCharges += change;
    }

    public GameObject GetTempestProjectile()
    {
        return tempestProjectile;
    }

    public GameObject GetStormProjectile()
    {
        return stormProjectile;
    }

    public Transform GetProjectileOrigin()
    {
        return projectileOrigin;
    }

    public void SetStateTransitionNum()
    {
        _stateTransitions++;
    }

    public void SetCombatStateTransitionNum()
    {
        _combatStateTransitions++;
    }

    public void SetSearchStateTransitionNum()
    {
        _searchStateTransitions++;
    }

    public void SetStormCount()
    {
        _stormCount++;
    }

    public void SetTempestCount()
    {
        _tempestCount++;
    }
    public void SetHits()
    {
        _hits++;
    }

    public void SetStormHits()
    {
        _stormHits++;
    }

    public void SetTempestHits()
    {
        _tempestHits++;
    }

    public void SetTeleportDodgeCount()
    {
        _tpDodgeCount ++;
    }

    public void SetDamageOutput(int dmgOutputUpdate)
    {
        _damageOutput += dmgOutputUpdate;
    }
}
