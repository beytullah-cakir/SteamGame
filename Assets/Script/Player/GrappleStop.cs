using StarterAssets;
using UnityEngine;

public class GrappleStop : MonoBehaviour
{
    public float grappleDetectRadius = 1.5f;
    [Tooltip("Hedefe ulaþýnca karakteri ne kadar yukarý/ileri fýrlatacaðýmýz")]
    public float boostUpwardsForce = 4f;
    public float boostForwardForce = 3f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HookManager hookManager = other.GetComponent<HookManager>();
            Rigidbody rb = other.GetComponent<Rigidbody>();

            if (hookManager != null && hookManager.isMovingToGrapplePoint)
            {
                hookManager.canGrapple = false;
                hookManager.isMovingToGrapplePoint = false;

                if (rb != null)
                {
                    rb.useGravity = true;
                    // Hedefe çarpýnca önceki dengesiz hýzlarý sýfýrla
                    rb.linearVelocity = Vector3.zero;

                    // Karakteri platformun üstüne doðru hafifçe zýplat
                    Vector3 boostForce = (other.transform.forward * boostForwardForce) + (Vector3.up * boostUpwardsForce);
                    rb.AddForce(boostForce, ForceMode.Impulse);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, grappleDetectRadius);
    }
}