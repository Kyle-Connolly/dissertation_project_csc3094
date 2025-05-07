using UnityEngine;

public abstract class DamageHandler : MonoBehaviour
{
    [SerializeField]
    protected GameObject deathVisual;

    public void TakeDamage(int damage)
    {
        int currentHealth = GetHealth();
        currentHealth -= damage;
        SetHealth(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        var impact = Instantiate(deathVisual, transform.position, Quaternion.identity) as GameObject;

        LogStats();

        Destroy(impact, 5);
        Destroy(gameObject);
    }

    public abstract int GetHealth();
    public abstract void SetHealth(int value);
    public abstract void LogStats();
}
