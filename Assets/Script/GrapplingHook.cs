using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;
    public Transform gunTip;
    public LineRenderer lineRenderer;
    public LayerMask grappleableLayer;

    [Header("Grappling")]
    public float maxGrappleDistance = 30f;
    public float grappleSpeed = 10f;
    public float stopDistance = 1f;

    private Vector3 grapplePoint;
    private bool isGrappling = false;

    private CharacterController controller;
    private PlayerMovement playerMovement; // hareket scriptin adý buysa

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (Input.GetMouseButton(1) && Input.GetMouseButtonDown(0)) // Sað týk + Sol týk
        {
            StartGrapple();
        }

        if (isGrappling)
        {
            GrappleMove();
        }

        if (Input.GetMouseButtonUp(1))
        {
            StopGrapple();
        }

        DrawRope();
    }

    private void StartGrapple()
    {
        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, maxGrappleDistance, grappleableLayer))
        {
            grapplePoint = hit.point;
            isGrappling = true;
            
        }
    }

    private void GrappleMove()
    {
        Vector3 direction = (grapplePoint - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, grapplePoint);

        if (distance > stopDistance)
        {
            controller.Move(direction * grappleSpeed * Time.deltaTime);
        }
        else
        {
            StopGrapple();
        }
    }

    private void StopGrapple()
    {
        isGrappling = false;
        if (playerMovement != null)
            playerMovement.enabled = true;

        lineRenderer.enabled = false;
    }

    private void DrawRope()
    {
        if (!isGrappling) return;

        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, gunTip.position);
        lineRenderer.SetPosition(1, grapplePoint);
    }
}
