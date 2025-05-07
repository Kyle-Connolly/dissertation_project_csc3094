using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class Enemy : DamageHandler
{
    public EnemyStateMachineController stateMachine;
    public GameObject teleportPrefab;
    public float visionRange = 100f;
    public float visionAngle = 180f;

    protected NavMeshAgent agent;
    [SerializeField]
    protected List<SentinelAgent> targets = new List<SentinelAgent>();
    protected SentinelAgent currentTarget;
    protected float sentinelIDamage = 0f;
    protected float stormSentinelIIDamage = 0f;
    protected float tempestSentinelIIDamage = 0f;
    protected bool isSentinelII = false;
    protected bool _destinationLocked = false; //To store if Enemy has locked onto the target
    protected bool targetAcquired = false;
    [SerializeField]
    protected int currentHealth;
    protected int maxHealth = 1000;

    //Start is called before the first frame update
    protected virtual void Start()
    {
        currentHealth = maxHealth;
        agent = GetComponent<NavMeshAgent>();
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        stateMachine = new EnemyStateMachineController(this);
    }

    //Update is called once per frame
    protected virtual void Update()
    {
        stateMachine.Update();
        SwitchTargetsDamageBased();
    }

    public bool CanSeeTarget()
    {
        //agent has a target (enemy agent) to focus on
        if (currentTarget != null)
        {
            //get position, direction and distance to target
            Vector3 targetEntity = currentTarget.transform.position + Vector3.up;
            Vector3 directionToTarget = (targetEntity - transform.position).normalized;
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);

            //target is outside vision range so return false - can't see target
            if (distanceToTarget > visionRange)
            {
                return false;
            }

            //calculate what angle horizontal and vertical - the target is to this agent
            float verticalAngle = Mathf.Asin((currentTarget.transform.position.y - transform.position.y) / distanceToTarget) * Mathf.Rad2Deg;
            float horizontalAngle = Vector3.SignedAngle(transform.forward, directionToTarget, Vector3.up);

            //target is outside of peripheral vision angle so return false - can't see target
            if (horizontalAngle > visionAngle / 2f)
            {
                return false;
            }

            //target is outside of peripheral vision angle so return false - can't see target
            if (Mathf.Abs(verticalAngle) > 90f)
            {
                return false;
            }

            //raycast from agent's position outwards (check line of sight)
            if (Physics.Raycast(transform.position, directionToTarget, out RaycastHit hit, visionRange))
            {
                //check to see if what the ray cast hits is the target by checking its tag
                if (hit.transform.CompareTag("Target") && this.transform.CompareTag("TargetII") || hit.transform.CompareTag("TargetII") && this.transform.CompareTag("Target") 
                    || hit.transform.CompareTag("Target") && this.transform.CompareTag("Target") || hit.transform.CompareTag("TargetII") && this.transform.CompareTag("TargetIII")
                    || hit.transform.CompareTag("TargetIII") && this.transform.CompareTag("TargetII"))
                {
                    Debug.DrawLine(transform.position, hit.point, Color.red, 0.1f); //debug line only shows up in viewport for testing purposes
                    return true; //agents sees its target so return true
                }
                else
                {
                    Debug.DrawLine(transform.position, hit.point, Color.yellow, 0.1f); //debug line only shows up in viewport for testing purposes
                }
            }

            //no line of sight on the target - return false
            return false;
        }

        return false;
    }

    //tries to acquire one of the agent's targets by raycasting
    public bool AcquireTarget()
    {
        //for each target in the agent's list of targets
        foreach (var target in targets)
        {
            //if no targets continue instead of throwing an error
            if (target == null) continue;

            //get position, direction and distance to target
            Vector3 targetEntity = target.transform.position + Vector3.up;
            Vector3 directionToTarget = (targetEntity - transform.position).normalized;
            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

            //target is outside vision range so return false - can't see target
            if (distanceToTarget > visionRange) continue;

            //calculate what angle horizontal and vertical - the target is to this agent
            float verticalAngle = Mathf.Asin((target.transform.position.y - transform.position.y) / distanceToTarget) * Mathf.Rad2Deg;
            float horizontalAngle = Vector3.SignedAngle(transform.forward, directionToTarget, Vector3.up);

            //target is outside of peripheral vision angle so return false - can't see target
            if (horizontalAngle > visionAngle / 2f || Mathf.Abs(verticalAngle) > 90f) continue;

            //raycast from agent's position outwards (check line of sight)
            if (Physics.Raycast(transform.position, directionToTarget, out RaycastHit hit, visionRange))
            {
                //check to see if what the ray cast hits is the target currently being checked by looking at its tag

                if (hit.transform.CompareTag("Target") && this.transform.CompareTag("TargetII") || hit.transform.CompareTag("TargetII") && this.transform.CompareTag("Target")
                    || hit.transform.CompareTag("Target") && this.transform.CompareTag("Target") || hit.transform.CompareTag("TargetII") && this.transform.CompareTag("TargetIII")
                    || hit.transform.CompareTag("TargetIII") && this.transform.CompareTag("TargetII"))
                {
                    currentTarget = target; //update current target
                    Debug.DrawLine(transform.position, hit.point, Color.red, 0.1f);
                    targetAcquired = true; //set to true so that method isn't called again unless required
                    return true; //agent has line of sight on this target so return true
                }
                else
                {
                    Debug.DrawLine(transform.position, hit.point, Color.yellow, 0.1f);
                }
            }
        }
        return false; //no line of sight on any targets
    }

    //if the Enemy has more than one opponent then switch current targets based on highest damage inflicted by another target
    public void SwitchTargetsDamageBased()
    {
        if (targets.Count <= 1)
        {
            return;
        }

        if (stormSentinelIIDamage > tempestSentinelIIDamage)
        {
            foreach (var target in targets)
            {
                if (target != null && target.GetSentinelVariantType())
                {
                    currentTarget = target;
                    break;
                }
            }
        }
        else if (tempestSentinelIIDamage > stormSentinelIIDamage)
        {
            foreach (var target in targets)
            {
                if (target != null && !target.GetSentinelVariantType())
                {
                    currentTarget = target;
                    break;
                }
            }
        }
        else
        {
            int randomIndex = UnityEngine.Random.Range(0, targets.Count);
            currentTarget = targets[randomIndex];
            
        }
    }

    //Called from projectile class when this enemy is hit with that projectile
    public void RegisterDamage(SentinelAgent attacker, float damage)
    {
        
        if (attacker.CompareTag("Target"))
        {
            sentinelIDamage += damage;
        }
        else if (attacker.CompareTag("TargetII"))
        {
            if (attacker.GetSentinelVariantType())
            {
                stormSentinelIIDamage += damage;
            }
            else
            {
                tempestSentinelIIDamage += damage;
            }
        }
        else if(attacker.CompareTag("TargetIII"))
        {
            tempestSentinelIIDamage += damage;
        }
    }

    public void FaceTarget()
    {
        Vector3 directionToTarget = (currentTarget.transform.position - transform.position).normalized;

        //Avoid tilting on y axis
        directionToTarget.y = 0;

        if (directionToTarget.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 50);
        }
    }

    public override int GetHealth()
    {
        return currentHealth;
    }

    public override void SetHealth(int value)
    {
        currentHealth = value;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public bool GetDestinationLocked()
    {
        return _destinationLocked;
    }

    public void SetDestinationLocked(bool lockState)
    {
        _destinationLocked = lockState;
    }

    public NavMeshAgent GetNavMeshAgent()
    {
        return agent;
    }

    public SentinelAgent GetTargetAgent()
    {
        return currentTarget;
    }

    public GameObject GetTeleportPrefab()
    {
        return teleportPrefab;
    }

    public void SetTargetAcquired(bool acquiredStatus)
    {
        targetAcquired = acquiredStatus;
    }

    public List<SentinelAgent> GetTargets()
    {
        return targets;
    }
}
