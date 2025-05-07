using UnityEngine;

public class USentinelCombatState : SentinelCombatState
{
    public override void UpdateState()
    {
        //Check if health is below 50, transition to Evade state once
        if (_sentinelAgent.GetHealth() <= 250 && !_sentinelAgent.blackboard.EvasionComplete)
        {
            _sentinelAgent.stateMachine.ChangeState(new SentinelEvadeState());
            _sentinelAgent.SetStateTransitionNum();
            return;
        }

        //Tempest cooldown timer
        if (tempestCooldownTimer > 0f)
        {
            tempestCooldownTimer -= Time.deltaTime;
        }

        //after teleport dodge has completed set to false so combat cycle can begin again
        if (isTeleporting)
        {
            isTeleporting = false;
        }

        if (_sentinelAgent.GetTargetAgent() != null && _sentinelAgent.CanSeeTarget() && !_sentinelAgent.blackboard.CombatCycleActive && !isTeleporting)
        {
            //set to true so that update method won't run this again mid cycle
            _sentinelAgent.blackboard.CombatCycleActive = true;

            //always facing the target for attacks
            _sentinelAgent.FaceTarget();

            //For debug purposes - visualising ranges in the viewport
            Vector3 rangeEnd = _sentinelAgent.GetProjectileOrigin().position + _sentinelAgent.transform.forward * _sentinelAgent.GetStormRange();
            Debug.DrawLine(_sentinelAgent.GetProjectileOrigin().position, rangeEnd, Color.green, 2f);

            distanceToTarget = Vector3.Distance(_sentinelAgent.transform.position, _sentinelAgent.GetTargetAgent().transform.position);
            
            health = _sentinelAgent.GetHealth();
            maxHealth = _sentinelAgent.GetMaxHealth();
            charges = _sentinelAgent.GetCharges();
            maxCharges = _sentinelAgent.GetMaxCharges();
            stormRange = _sentinelAgent.GetStormRange();
            tempestRange = _sentinelAgent.GetTempestRange();
            tempestDistMidpoint = (stormRange + tempestRange) / 2f;

            float stormScore = SentinelUtility.CalculateStormUtilityScore(distanceToTarget, stormRange, charges, maxCharges);
            float tempestScore = SentinelUtility.CalculateTempestUtilityScore(distanceToTarget, tempestDistMidpoint, charges, maxCharges);
            
            //Select attack based on utility scores
            if (stormScore > tempestScore && charges >= 2)
            {
                _sentinelAgent.FaceTarget();
                //true for storm attack
                SentinelAttack(true);
            }
            else if (tempestScore > stormScore && charges >= 3 && tempestCooldownTimer <= 0f)
            {
                _sentinelAgent.FaceTarget();
                //false for tempest attack
                SentinelAttack(false);
                tempestCooldownTimer = tempestCooldown;
            }

            //Check if too much damage was taken in the last 4 seconds
            if (Time.time - healthCheckTimer >= healthCheckTime)
            {
                float health = _sentinelAgent.GetHealth();
                float damage = previousHealth - health;

                if (damage >= damageThreshold)
                {
                    TeleportDodge();
                }

                previousHealth = health;
                healthCheckTimer = Time.time;
            }

            float chaseScore = SentinelUtility.CalculateChaseDistanceUtilityScore(health, maxHealth, distanceToTarget, tempestRange);

            if (chaseScore >= 0.5f)
            {
                //Aggressive chase for Storm (close range) attack
                ChaseTarget(true);
            }
            else
            {
                //More conservative chase for Tempest (long range) attack
                ChaseTarget(false);
            }

            //set to false as combat cycle has completed so this section of code can run again
            _sentinelAgent.blackboard.CombatCycleActive = false;
        }

        //lost enemy SentinelAgent so need to search but only after a certain period of time has passed
        if (!_sentinelAgent.CanSeeTarget())
        {
            lostTargetTimer += Time.deltaTime;

            if (lostTargetTimer >= waitBeforeSearch)
            {
                _sentinelAgent.SetDestinationLocked(false);
                lostTargetTimer = 0f;

                _sentinelAgent.SetTargetAcquired(false);
                _sentinelAgent.SetSearchStateTransitionNum();
                _sentinelAgent.stateMachine.ChangeState(new ASentinelSearchState());
                _sentinelAgent.SetStateTransitionNum();
            }
        }
    }
}
