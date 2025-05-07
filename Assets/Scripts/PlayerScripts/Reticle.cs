using UnityEngine.UI;
using UnityEngine;

public class Reticle : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Image reticleImage;
    [SerializeField] private LayerMask targetLayer; // Layer mask for enemies
    [SerializeField] private LayerMask obstacleLayer; // Layer mask for obstacles
    [SerializeField] private Color defaultColor = Color.white; // Default reticle colour
    [SerializeField] private Color targetColor = Color.red; // Colour when a target is in range
    [SerializeField] private float detectionRange = 30f; // Range for detection

    private void Update()
    {
        UpdateReticleColor();
    }

    private void UpdateReticleColor()
    {
        // Raycast from center of camera view
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit rayHit;

        // Check if ray hits within range
        if (Physics.Raycast(ray, out rayHit, detectionRange))
        {
            // Check if object is on layer
            if (((1 << rayHit.collider.gameObject.layer) & targetLayer) != 0)
            {
                // Raycast check for obstacles
                Vector3 targetDirection = rayHit.point - playerCamera.transform.position;
                float targetDistance = Vector3.Distance(playerCamera.transform.position, rayHit.point);

                if (!Physics.Raycast(playerCamera.transform.position, targetDirection.normalized, targetDistance, obstacleLayer))
                {
                    // No obstacle blocking
                    reticleImage.color = targetColor;
                    return;
                }
            }
        }

        // Reset the reticle to default color if no valid target
        reticleImage.color = defaultColor;
    }
}
