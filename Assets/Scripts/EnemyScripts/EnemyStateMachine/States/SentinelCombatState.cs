using UnityEngine;
using UnityEngine.AI;

public abstract class SentinelCombatState : IEnemyState
{
    protected SentinelAgent _sentinelAgent;
    protected float lostTargetTimer = 0f;
    protected float waitBeforeSearch = 2f;
    protected float healthCheckTimer = 0f;
    protected float healthCheckTime = 4f;
    protected float damageThreshold = 100f;
    protected float previousHealth = 0f;
    protected float distanceToTarget = 0f;
    protected float health = 0f;
    protected float maxHealth = 0f;
    protected float stormRange = 0f;
    protected float tempestRange = 0f;
    protected float tempestDistMidpoint = 0f;
    protected float tempestCooldown = 1.5f;
    protected float tempestCooldownTimer = 0f;
    protected int charges = 0;
    protected int maxCharges = 0;
    protected bool isTeleporting = false;

    public void EnterState(Enemy enemy)
    {
        this._sentinelAgent = (SentinelAgent)enemy;
        previousHealth = _sentinelAgent.GetHealth();
        healthCheckTimer = Time.time;
    }

    public virtual void UpdateState(){}

    protected void ChaseTarget(bool isStorm)
    {
        NavMeshAgent agent = _sentinelAgent.GetNavMeshAgent();

        _sentinelAgent.SetDestinationLocked(true);

        //if the input chase type is the storm type (close quarters chasing) then set the chase/follow distance to the storm distance
        //else set it to the tempest distance
        float followDist = isStorm ? _sentinelAgent.GetStormRange() : _sentinelAgent.GetTempestRange();

        Vector3 directionToTarget = (_sentinelAgent.GetTargetAgent().transform.position - _sentinelAgent.transform.position).normalized;

        //Target position + attack range
        Vector3 targetPosition = _sentinelAgent.GetTargetAgent().transform.position - directionToTarget * followDist;

        //Move only if AI is out of range of the attack
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

    //Try dodging by searching for space left, right, or behind. If no space found then do nothing.
    protected void TeleportDodge()
    {
        Vector3 currentPosition = _sentinelAgent.transform.position;
        float teleportDistance = 2.5f;
        float radius = 2.0f;

        //Available Teleport directions
        Vector3[] directions = new Vector3[]
        {
            _sentinelAgent.transform.right, //Right
            -_sentinelAgent.transform.right, //Left
            -_sentinelAgent.transform.forward //Backwards
        };

        //Shuffle directions list to make sure AI doesn't dodge too predictably
        for (int i = 0; i < directions.Length; i++)
        {
            int randomIndex = Random.Range(i, directions.Length);
            Vector3 tempDirection = directions[i];
            directions[i] = directions[randomIndex];
            directions[randomIndex] = tempDirection;
        }

        foreach (var dir in directions)
        {
            Vector3 teleportSpace = currentPosition + dir * teleportDistance;

            //Check for valid space to teleport
            if (NavMesh.SamplePosition(teleportSpace, out NavMeshHit hit, radius, NavMesh.AllAreas))
            {
                isTeleporting = true;

                _sentinelAgent.GetNavMeshAgent().Warp(hit.position + new Vector3(0, 1.5f, 0));

                //Teleport visual
                var warpEffect = UnityEngine.Object.Instantiate(_sentinelAgent.GetTeleportPrefab(), _sentinelAgent.GetNavMeshAgent().transform.position + new Vector3(0, -1.7f, 0), Quaternion.identity);
                UnityEngine.Object.Destroy(warpEffect, 1f);

                _sentinelAgent.SetTeleportDodgeCount();

                //Return since successful teleport dodge
                return;
            }
        }
    }

    protected void SentinelAttack(bool isStormAttack)
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

    private void CreateProjectile(Vector3 target, bool isStormAttack)
    {
        GameObject projectile;

        if (isStormAttack)
        {
            projectile = GameObject.Instantiate(_sentinelAgent.GetStormProjectile(), _sentinelAgent.GetProjectileOrigin().position, Quaternion.identity);
        }
        else
        {
            projectile = GameObject.Instantiate(_sentinelAgent.GetTempestProjectile(), _sentinelAgent.GetProjectileOrigin().position, Quaternion.identity);
        }

        Rigidbody body = projectile.GetComponent<Rigidbody>();

        Projectile projectileScript = projectile.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.SetShooter(_sentinelAgent);
        }

        //Calculate direction + velocity
        Vector3 projectilePosition = _sentinelAgent.GetProjectileOrigin().position;
        Vector3 targetDistance = target - projectilePosition;
        float distance = targetDistance.magnitude;

        //Get height difference
        float heightDiff = target.y - projectilePosition.y;

        //Set initial launch angle
        float angle = Mathf.Deg2Rad * 20;
        float gravity = Physics.gravity.y;
        float velocityMagnitude = Mathf.Sqrt(distance * Mathf.Abs(gravity) / Mathf.Sin(2 * angle));

        //Launch velocity vector
        Vector3 horizontalDirection = new Vector3(targetDistance.x, 0, targetDistance.z).normalized;
        Vector3 launchVelocity = horizontalDirection * velocityMagnitude * Mathf.Cos(angle);
        launchVelocity.y = velocityMagnitude * Mathf.Sin(angle);

        //Calculated velocity to projectile rigid body
        body.velocity = launchVelocity;
    }
}
