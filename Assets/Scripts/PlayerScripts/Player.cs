using UnityEngine;

public class Player : DamageHandler
{
    [Header("Player Settings")]
    public int maxHealth = 500;
    public int score = 1;

    private int _currentHealth;

    // Start is called before the first frame update
    void Start()
    {
        _currentHealth = maxHealth;
    }

    public override int GetHealth()
    {
        return _currentHealth;
    }

    public override void SetHealth(int value)
    {
        _currentHealth = value;
    }

    public override void LogStats()
    {
        //do something
    }
}
