public class EnemyStateMachineController
{
    private IEnemyState _currentState;
    private Enemy _enemy;

    public EnemyStateMachineController(Enemy enemy)
    {
        this._enemy = enemy;
    }

    public void ChangeState(IEnemyState newState)
    {
        _currentState = newState;
        _currentState.EnterState(_enemy);
    }

    public void Update()
    {
        _currentState?.UpdateState();
    }
}
