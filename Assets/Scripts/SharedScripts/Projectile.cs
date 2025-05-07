using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject impactPrefab;
    public int projectileDamage = 75;
    public bool isStormAttack = false;
    
    protected SentinelAgent shooter;
    protected bool collided;
    protected Vector3 _startPosition;
    protected float _travelDistance = 0f;

    [SerializeField]
    private float _tempestMaxRange = 15f;

    protected void Start()
    {
        _startPosition = transform.position;
    }

    private void Update()
    {
        _travelDistance = Vector3.Distance(_startPosition, transform.position);

        if (_travelDistance >= _tempestMaxRange)
        {
            CreateImpact();
        }
    }

    public void SetShooter(SentinelAgent agent)
    {
        shooter = agent;
    }

    protected void CreateImpact()
    {
        Vector3 spawnPosition = transform.position;
        var impact = Instantiate(impactPrefab, spawnPosition, Quaternion.identity) as GameObject; ;
        Destroy(impact, 2);
        Destroy(gameObject);
    }

    protected void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Target") || collision.CompareTag("TargetII") || collision.CompareTag("TargetIII") && collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(projectileDamage);

                if (shooter != null)
                {
                    shooter.SetHits();

                    if (isStormAttack)
                    {
                        shooter.SetStormHits();
                    }
                    else
                    {
                        shooter.SetTempestHits();
                    }

                    shooter.SetDamageOutput(projectileDamage);

                    //Register damage from attacker
                    enemy.RegisterDamage(shooter, projectileDamage);
                }
            }
        }
    }

    protected void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag != "Tempest" || collision.gameObject.tag != "Storm" && !collided)
        {
            collided = true;
            var impact = Instantiate(impactPrefab, collision.contacts[0].point, Quaternion.identity) as GameObject;
            Destroy(impact, 2);
            Destroy(gameObject);
        }
    }
}
