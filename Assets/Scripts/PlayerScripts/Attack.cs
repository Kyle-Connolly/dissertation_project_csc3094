using UnityEngine;

public abstract class Attack : MonoBehaviour
{
    public Camera cameraView;
    public GameObject tempestProjectile;
    public Transform projectileOrigin;
    public float tempestRange = 15f;
    public float tempestSpeed = 30f;

    private Vector3 _target;

    public bool RangedAttack(string attackName, int chargeCount, LayerMask layer)
    {
        AttackType currentAttack = ValidateAttackRequest(attackName, chargeCount, layer);
        
        // check that the current attack is set and is a registered attack in the AttackSet
        if ((currentAttack.Equals(new AttackType())))
        {
            return false;
        }

        
        // Check if enough charges are available
        if (!CheckCharge(chargeCount, currentAttack))
        {
            return false;
        }

        Ray ray = cameraView.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        RaycastHit rayHit;

        if (Physics.Raycast(ray, out rayHit))
        {
            _target = rayHit.point;
        }
        else
        {
            _target = ray.GetPoint(1000);
        }

        CreateProjectile();
        return true;
    }

    public void CreateProjectile()
    {
        var projectile = Instantiate(tempestProjectile, projectileOrigin.position, Quaternion.identity) as GameObject;
        projectile.GetComponent<Rigidbody>().velocity = (_target - projectileOrigin.position).normalized * tempestSpeed;
    }

    public AttackType ValidateAttackRequest(string attackName, int chargeCount, LayerMask layer)
    {
        // Switch expression to set currentAttack
        AttackType currentAttack = attackName switch
        {
            "Tempest" => AttackSet.Attacks["Tempest"],
            _ => new AttackType()
        };

        return currentAttack;
    }

    public bool CheckCharge(int chargeCount, AttackType currentAttack)
    {
        // Check if enough charges are available
        if (chargeCount < currentAttack.elementalChargeCost)
        {
            return false;
        }

        return true;
    }
}
