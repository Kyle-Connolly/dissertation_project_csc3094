using UnityEngine;

public class SplitSentinelCombatState : SentinelCombatState
{
    public override void UpdateState()
    {
        //check if health is below 50, transition to Evade state once
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

        if (isTeleporting)
        {
            isTeleporting = false;
        }

        if (_sentinelAgent.GetTargetAgent() != null && _sentinelAgent.CanSeeTarget() && !_sentinelAgent.blackboard.CombatCycleActive && !isTeleporting)
        {
            _sentinelAgent.blackboard.CombatCycleActive = true;

            _sentinelAgent.FaceTarget();

            //for debug purposes
            Vector3 rangeEnd = _sentinelAgent.GetProjectileOrigin().position + _sentinelAgent.transform.forward * _sentinelAgent.GetStormRange();
            Debug.DrawLine(_sentinelAgent.GetProjectileOrigin().position, rangeEnd, Color.green, 2f);

            distanceToTarget = Vector3.Distance(_sentinelAgent.transform.position, _sentinelAgent.GetTargetAgent().transform.position);

            health = _sentinelAgent.GetHealth();
            maxHealth = _sentinelAgent.GetMaxHealth();
            charges = _sentinelAgent.GetCharges();
            maxCharges = _sentinelAgent.GetMaxCharges();
            stormRange = _sentinelAgent.GetStormRange();
            tempestRange = _sentinelAgent.GetTempestRange();

            //Storm attack if within Storm range and Sentinel is the Storm variant
            if (_sentinelAgent.CompareTag("TargetII") && _sentinelAgent.GetSentinelVariantType() && distanceToTarget <= _sentinelAgent.GetStormRange() && charges >= 2)
            {
                _sentinelAgent.FaceTarget();
                //true for storm attack
                SentinelAttack(true);
            }

            //Tempest attack if within Tempest range and Sentinel is the Tempest variant
            if (_sentinelAgent.CompareTag("TargetII") && !_sentinelAgent.GetSentinelVariantType() && tempestCooldownTimer <= 0f && charges >= 3 
                || _sentinelAgent.CompareTag("TargetIII") && !_sentinelAgent.GetSentinelVariantType() && tempestCooldownTimer <= 0f && charges >= 3)
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


            if (_sentinelAgent.CompareTag("TargetII") && _sentinelAgent.GetSentinelVariantType())
            {
                //Aggressive chase for Storm (close range) attack
                ChaseTarget(true);
            }

            if (_sentinelAgent.CompareTag("TargetII") && !_sentinelAgent.GetSentinelVariantType())
            {
                //More conservative chase for Tempest (long range) attack
                ChaseTarget(false);
            }

            _sentinelAgent.blackboard.CombatCycleActive = false;
        }

        //Lost enemy SentinelAgent so need to search but only after a certain period of time has passed
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
