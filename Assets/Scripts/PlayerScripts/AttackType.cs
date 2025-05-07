public struct AttackType
{
    public string name { get; }
    public int damage { get; }
    public int elementalChargeCost { get; }

    public AttackType(string attackName, int damageVal, int cost)
    {
        name = attackName;
        damage = damageVal;
        elementalChargeCost = cost;
    }

    public override string ToString()
    {
        return $"Attack: {name}, Damage: {damage}, Cost: {elementalChargeCost}";
    }
}

