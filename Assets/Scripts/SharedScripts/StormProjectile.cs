using UnityEngine;

public class StormProjectile : Projectile
{
    [SerializeField]
    private float _stormMaxRange = 5f;

    private void Update()
    {
        _travelDistance = Vector3.Distance(_startPosition, transform.position);

        if (_travelDistance >= _stormMaxRange)
        {
            CreateImpact();
        }
    }
}