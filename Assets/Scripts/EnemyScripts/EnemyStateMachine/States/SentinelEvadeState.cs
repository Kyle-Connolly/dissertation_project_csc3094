using UnityEngine;
using UnityEngine.AI;

public class SentinelEvadeState : IEnemyState
{
    private SentinelAgent _sentinelAgent;
    private NavMeshAgent _agent;
    private float _evasionTimer = 0f;
    private float _evasionDuration = 20f;
    private float _originalSpeed;
    private float _originalAcceleration;
    private float _speedIncrease = 2.0f;
    private float _accelerationIncrease = 1.5f;
    private bool _isHiding = false;

    public void EnterState(Enemy enemy)
    {
        this._sentinelAgent = (SentinelAgent)enemy;
        _agent = _sentinelAgent.GetNavMeshAgent();
        _evasionTimer = 0f;
        _originalSpeed = _agent.speed;
        _originalAcceleration = _agent.acceleration;
        _agent.speed *= _speedIncrease;
        _agent.acceleration *= _accelerationIncrease;
        SelectHidingPosition();
    }

    public void UpdateState()
    {
        _evasionTimer += Time.deltaTime;

        //Stop evasion after 25 seconds
        if (_evasionTimer >= _evasionDuration)
        {
            _sentinelAgent.blackboard.EvasionComplete = true;
            _agent.ResetPath();
            _agent.speed = _originalSpeed;
            _agent.acceleration = _originalAcceleration;

            _sentinelAgent.SetStateTransitionNum();

            if (_sentinelAgent.CompareTag("TargetII") || _sentinelAgent.CompareTag("TargetIII"))
            {
                
                _sentinelAgent.stateMachine.ChangeState(new SplitSentinelCombatState());
            }
            if (_sentinelAgent.CompareTag("Target"))
            {
                _sentinelAgent.stateMachine.ChangeState(new USentinelCombatState());
            }

            return;
        }

        if (!_isHiding && !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
        {
            _isHiding = true;
            _sentinelAgent.FaceTarget();
        }

        if (_isHiding)
        {
            if (HasTargetLineOfSight())
            {
                _isHiding = false;
                SelectHidingPosition();
            }
        }
    }

    private void SelectHidingPosition()
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * 30f;
        randomDirection += _sentinelAgent.transform.position;
        NavMeshHit hit;

        if (NavMesh.SamplePosition(randomDirection, out hit, 30f, NavMesh.AllAreas))
        {

            _agent.SetDestination(hit.position);
            _isHiding = false;
        }
    }

    private bool HasTargetLineOfSight()
    {
        var targets = _sentinelAgent.GetTargets();

        if (targets == null)
        {
            return false;
        }

        Vector3 sentinelPosition = _sentinelAgent.transform.position;

        foreach (var target in targets)
        {
            if (target == null) continue;

            Vector3 targetPosition = target.transform.position;
            Vector3 direction = sentinelPosition - targetPosition;

            Debug.DrawRay(targetPosition, direction.normalized * direction.magnitude, Color.blue, 1f);

            //If there's a clear line of sight from the target to the sentinel
            if (Physics.Raycast(targetPosition, direction.normalized, direction.magnitude))
            {
                return true;
            }
        }

        return false;
    }
}
