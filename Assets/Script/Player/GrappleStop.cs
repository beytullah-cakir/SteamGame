using StarterAssets;
using UnityEngine;

public class GrappleStop : MonoBehaviour
{
    public float grappleDetectRadius;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HookManager hookManager = other.GetComponent<HookManager>();
            ThirdPersonMovementController thirdPersonMovementController = other.GetComponent<ThirdPersonMovementController>();
            
                //thirdPersonMovementController.freezeMovement = false;
                hookManager.canGrapple = false;
                other.GetComponent<Rigidbody>().useGravity = true;
                hookManager.isMovingToGrapplePoint= false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, grappleDetectRadius);
    }
}
