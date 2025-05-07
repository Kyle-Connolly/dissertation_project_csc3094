using UnityEngine;

public static class SentinelUtility
{
    //Quadratic curve based utility calculation - (1 - x²). Higher score when distance to target is low.
    public static float CalculateStormDistanceUtility(float distance, float maxDistance)
    {
        float distanceRatio = Mathf.Clamp01(distance / maxDistance);
        return 1f - (distanceRatio * distanceRatio);
    }

    //Logistic Function based utility calculation - 1 / (1 + e^(-k(x - x0))). Higher score when distance to target is ideal (tempest range midpoint)
    public static float CalculateTempestDistanceUtility(float distance, float midpoint)
    {
        float k = 1.5f; //curve steepness/slope
        return 1f / (1f + Mathf.Exp(-k * (distance - midpoint)));
    }

    //Linear curve based utility score calculation for managing resources (elemental charges/ammunition)
    //Normalisation of charge level. Returns a value between 0 (no charges) and 1 (max charges).
    public static float CalculateChargeUtility(int currentCharges, int maxCharges)
    {
        return Mathf.Clamp01((float)currentCharges / maxCharges);
    }

    //Final Storm attack utility score using weighted utility aggregation. Weighting of 0.7 for distance since more important than charges
    public static float CalculateStormUtilityScore(float distance, float maxDistance, int charges, int maxCharges)
    {
        float distanceUtilityScore = CalculateStormDistanceUtility(distance, maxDistance);
        float chargeUtilityScore = CalculateChargeUtility(charges, maxCharges);
        return 0.7f * distanceUtilityScore + 0.3f * chargeUtilityScore;
    }

    //Final Tempest attack utility score using weighted utility aggregation
    public static float CalculateTempestUtilityScore(float distance, float optimalMid, int charges, int maxCharges)
    {
        float distanceUtilityScore = CalculateTempestDistanceUtility(distance, optimalMid);
        float chargeUtilityScore = CalculateChargeUtility(charges, maxCharges);
        return 0.7f * distanceUtilityScore + 0.3f * chargeUtilityScore;
    }

    //Linear curve based utility score calculation for assessing health of the AI.
    public static float CalculateHealthUtilityScore(float health, float maxHealth)
    {
        return Mathf.Clamp01(health / maxHealth);
    }

    //Final Chase utility score.Higher health -> more aggressive and so closer follow/chase distance
    public static float CalculateChaseDistanceUtilityScore(float health, float maxHealth, float distance, float tempestRange)
    {
        float healthUtility = CalculateHealthUtilityScore(health, maxHealth);
        return healthUtility;
    }
}
