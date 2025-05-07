using UnityEngine;
using UnityEngine.AI;

public class SentinelPatrolState : IEnemyState
{
    private SentinelAgent _sentinelAgent;
    private Transform[] _checkPointList;
    private int _checkPoint;

    public void EnterState(Enemy enemy)
    {
        this._sentinelAgent = (SentinelAgent)enemy;
        this._checkPointList = _sentinelAgent.GetPatrolCheckPoints();
    }

    public void UpdateState()
    {
        if (_sentinelAgent.AcquireTarget())
        {
            _sentinelAgent.SetStateTransitionNum();

            if (_sentinelAgent.CompareTag("TargetII") || _sentinelAgent.CompareTag("TargetIII"))
            {
                _sentinelAgent.SetCombatStateTransitionNum();
                _sentinelAgent.stateMachine.ChangeState(new SplitSentinelCombatState());
            }
            if (_sentinelAgent.CompareTag("Target"))
            {
                _sentinelAgent.SetCombatStateTransitionNum();
                _sentinelAgent.stateMachine.ChangeState(new USentinelCombatState());
            }
        }
        Patrol();
    }

    private void Patrol()
    {
        NavMeshAgent agent = _sentinelAgent.GetNavMeshAgent();
        _checkPoint = _sentinelAgent.GetPatrolCheckpoint();

        //Continue patrolling if no target lock on
        if (!_sentinelAgent.GetDestinationLocked() && agent.remainingDistance < 0.1f)
        {
            //Move to next checkpoint. Return to start at the end
            _sentinelAgent.SetPatrolCheckpoint((_checkPoint + 1) % _checkPointList.Length);
            agent.SetDestination(_checkPointList[_checkPoint].position);
        }
    }
}
