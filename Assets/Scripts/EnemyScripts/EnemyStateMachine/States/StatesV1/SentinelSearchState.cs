using UnityEngine;
using UnityEngine.AI;

public class SentinelSearchState : IEnemyState
{
    private SentinelAgent _sentinelAgent;
    private Vector3 _searchPosition;
    private bool _foundDestination = false;

    public void EnterState(Enemy enemy)
    {
        this._sentinelAgent = (SentinelAgent)enemy;
        PickRandomPosition();
        _sentinelAgent.GetNavMeshAgent().updateRotation = true;
    }

    public void UpdateState()
    {
        if (_sentinelAgent.CanSeeTarget())
        {
            _sentinelAgent.stateMachine.ChangeState(new USentinelCombatState());
            _sentinelAgent.SetStateTransitionNum();
            return;
        }

        if (_foundDestination && !_sentinelAgent.GetNavMeshAgent().pathPending && _sentinelAgent.GetNavMeshAgent().remainingDistance <= _sentinelAgent.GetNavMeshAgent().stoppingDistance)
        {
            _foundDestination = false;
        }

        if (!_foundDestination)
        {
            PickRandomPosition();
        }
    } 

    //mimics searching for the target
    protected void PickRandomPosition()
    {
        float radius = 8.0f;
        //maximum attempts
        int attempts = 10;

        for (int i = 0; i < attempts; i++)
        {
            //generate random direction within radius
            Vector2 randomDirection = Random.insideUnitCircle * radius;
            Vector3 newPosition = _sentinelAgent.transform.position + new Vector3(randomDirection.x, 0, randomDirection.y);

            //check if random position is on NavMesh
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(newPosition, out navHit, radius, NavMesh.AllAreas))
            {
                //distance is far enough - avoids jitter
                if (Vector3.Distance(_sentinelAgent.transform.position, navHit.position) > 4.0f)
                {
                    _searchPosition = navHit.position;
                    _sentinelAgent.GetNavMeshAgent().SetDestination(_searchPosition);
                    _foundDestination = true;
                    return;
                }
            }
        }

        _searchPosition = _sentinelAgent.transform.position;
        _foundDestination = false;
    }
}
