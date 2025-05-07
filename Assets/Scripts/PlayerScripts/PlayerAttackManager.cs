using UnityEngine;

public class PlayerAttackManager : Attack
{
    public LayerMask targetLayer;

    [SerializeField]
    private Player _player;

    private string _currentAttack;
    private int _chargeLimit = 10; // Maximum elemental charge count
    private int _currentCharges = 10; // 10 charge max
    private float _elementalRechargeTime = 0.5f; // Generate charge every 2 seconds
    private float _chargeTimer = 0f; // Timer to track time since last charge generation

    private void Start()
    {
        // Default attack
        _currentAttack = AttackSet.Tempest.name;
    }

    // Update is called once per frame
    void Update()
    {
        // Regenerate charges over time
        if (Time.time >= _chargeTimer + _elementalRechargeTime && _currentCharges < _chargeLimit)
        {
            _currentCharges++;
            _chargeTimer = Time.time;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _currentAttack = "Tempest";
        }

        if (Input.GetMouseButtonDown(0))
        {
            //Debug.Log($"USING: {_currentAttack}");
            bool attackSuccess = false;

            //Tempest (ranged) attack
            if (_currentAttack.Equals("Tempest")) {
                attackSuccess = RangedAttack(_currentAttack, _currentCharges, targetLayer);
            }

            if (attackSuccess)
            {
                _currentCharges -= AttackSet.Attacks[_currentAttack].elementalChargeCost;
                return;
            }
        }
    }

    public int GetCurrentChargeCount()
    {
        return _currentCharges;
    }

    public string GetCurrentAttack()
    {
        return _currentAttack;
    }
}
