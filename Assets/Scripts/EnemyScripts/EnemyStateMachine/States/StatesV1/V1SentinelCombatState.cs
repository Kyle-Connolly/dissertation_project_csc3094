using UnityEngine;
using UnityEngine.AI;

public class V1SentinelCombatState : IEnemyState
{
    private SentinelAgent _sentinelAgent;
    private float _lostPlayerTimer = 0f;
    private float _waitBeforeSearch = 2f;

    public void EnterState(Enemy enemy)
    {
        this._sentinelAgent = (SentinelAgent)enemy;
    }

    void Update()
    {
        UpdateState();
    }

    public void UpdateState()
    {
        //check if health is below 50, transition to Evade state once
        if (_sentinelAgent.GetHealth() <= 50 && !_sentinelAgent.blackboard.EvasionComplete)
        {
            _sentinelAgent.stateMachine.ChangeState(new SentinelEvadeState());
            _sentinelAgent.SetStateTransitionNum();
            return;
        }

        float distanceToPlayer = Vector3.Distance(_sentinelAgent.transform.position, _sentinelAgent.GetTargetAgent().transform.position);

        if (_sentinelAgent.CanSeeTarget())
        {
            FacePlayer();

            //for debug purposes
            Vector3 rangeEnd = _sentinelAgent.GetProjectileOrigin().position + _sentinelAgent.transform.forward * _sentinelAgent.GetStormRange();
            Debug.DrawLine(_sentinelAgent.GetProjectileOrigin().position, rangeEnd, Color.green, 2f);

            //Storm attack if within Storm range
            if (distanceToPlayer <= _sentinelAgent.GetStormRange() + 0.5f && _sentinelAgent.GetCharges() >= 2)
            {
                FacePlayer();
                SentinelAttack(true);
            }

            //Tempest attack if within Tempest range
            if (distanceToPlayer <= _sentinelAgent.GetTempestRange() + 0.5f && distanceToPlayer > _sentinelAgent.GetStormRange())
            {
                FacePlayer();
                for (int i = 0; i < _sentinelAgent.GetCharges() / 3; i++)
                {
                    SentinelAttack(false);
                }
            }

            if (distanceToPlayer > _sentinelAgent.GetStormRange() + 0.5f && distanceToPlayer < _sentinelAgent.GetTempestRange() + 0.5f && _sentinelAgent.GetHealth() > 150)
            {
                ChasePlayer(true);
            }

            if (distanceToPlayer > _sentinelAgent.GetTempestRange() + 0.5f && _sentinelAgent.GetHealth() <= 150)
            {
                ChasePlayer(false);
            }
        }

        //lost player so need to search but only after a certain period of time has passed
        if (!_sentinelAgent.CanSeeTarget())
        {
            _lostPlayerTimer += Time.deltaTime;

            if (_lostPlayerTimer >= _waitBeforeSearch)
            {
                _sentinelAgent.SetDestinationLocked(false);
                _lostPlayerTimer = 0f;

                _sentinelAgent.stateMachine.ChangeState(new SentinelSearchState());
                _sentinelAgent.SetStateTransitionNum();
            }
        }
    }

    public void ChasePlayer(bool isStormAttack)
    {
        NavMeshAgent agent = _sentinelAgent.GetNavMeshAgent();

        _sentinelAgent.SetDestinationLocked(true);

        float followDist = isStormAttack ? _sentinelAgent.GetStormRange() : _sentinelAgent.GetTempestRange();
        Vector3 directionToPlayer = (_sentinelAgent.GetTargetAgent().transform.position - _sentinelAgent.transform.position).normalized;

        //player position + attack range
        Vector3 targetPosition = _sentinelAgent.GetTargetAgent().transform.position - directionToPlayer * followDist;

        //move only if AI is out of range of the attack
        if (Vector3.Distance(_sentinelAgent.transform.position, _sentinelAgent.GetTargetAgent().transform.position) > followDist)
        {
            agent.SetDestination(targetPosition);
            agent.isStopped = false;
        }
        else
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    private void FacePlayer()
    {
        Vector3 directionToPlayer = (_sentinelAgent.GetTargetAgent().transform.position - _sentinelAgent.transform.position).normalized;
        //avoid tilting on the y axis
        directionToPlayer.y = 0;

        if (directionToPlayer.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            _sentinelAgent.transform.rotation = Quaternion.Slerp(_sentinelAgent.transform.rotation, targetRotation, Time.deltaTime * 50);
        }
    }

    public void SentinelAttack(bool isStormAttack)
    {
        int attackCost = isStormAttack ? -2 : -3;
        _sentinelAgent.SetCurrentCharges(attackCost);
       
        if (isStormAttack)
        {
            _sentinelAgent.SetStormCount();
        }
        else if (!isStormAttack)
        {
            _sentinelAgent.SetTempestCount();
        }
        
        Vector3 target = _sentinelAgent.GetTargetAgent().transform.position;

        CreateProjectile(target, isStormAttack);
    }

    public void CreateProjectile(Vector3 target, bool isStormAttack)
    {
        GameObject projectile;

        if (isStormAttack)
        {
            projectile = GameObject.Instantiate(_sentinelAgent.GetStormProjectile(), _sentinelAgent.GetProjectileOrigin().position, Quaternion.identity);
        } else
        {
            projectile = GameObject.Instantiate(_sentinelAgent.GetTempestProjectile(), _sentinelAgent.GetProjectileOrigin().position, Quaternion.identity);
        }

        Rigidbody body = projectile.GetComponent<Rigidbody>();

        Projectile projectileScript = projectile.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.SetShooter(_sentinelAgent);
        }

        // Calculate direction + velocity
        Vector3 projectilePosition = _sentinelAgent.GetProjectileOrigin().position;
        Vector3 targetDistance = target - projectilePosition;
        float distance = targetDistance.magnitude;

        // Get height difference
        float heightDiff = target.y - projectilePosition.y;

        // Set initial launch angle
        float angle = Mathf.Deg2Rad * 20;
        float gravity = Physics.gravity.y;
        float velocityMagnitude = Mathf.Sqrt(distance * Mathf.Abs(gravity) / Mathf.Sin(2 * angle));

        // Launch velocity vector
        Vector3 horizontalDirection = new Vector3(targetDistance.x, 0, targetDistance.z).normalized;
        Vector3 launchVelocity = horizontalDirection * velocityMagnitude * Mathf.Cos(angle);
        launchVelocity.y = velocityMagnitude * Mathf.Sin(angle);

        // Calculated velocity to projectile rigid body
        body.velocity = launchVelocity;
    }
}
