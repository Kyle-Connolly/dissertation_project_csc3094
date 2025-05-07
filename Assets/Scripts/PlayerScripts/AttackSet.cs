using System.Collections.Generic;

public static class AttackSet
{
    public static readonly AttackType Tempest = new AttackType("Tempest", 75, 5);

    public static readonly Dictionary<string, AttackType> Attacks = new Dictionary<string, AttackType>
    {
        { "Tempest", Tempest },
    };
}
