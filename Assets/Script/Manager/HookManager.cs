using StarterAssets;
using System;
using UnityEngine;

public class HookManager : MonoBehaviour
{
    public Transform gunTip;
    public LayerMask grappleLayer;
    public float maxDistance;
    private Vector3 targetPoint;
    protected RaycastHit grappleHit;
    public Rigidbody rb;
    protected ThirdPersonMovementController tp;
    public bool canGrapple;
    public float grappleSpeed;
    private Vector3 moveVelocity;
    public Vector3 grappleOffset;
    public bool isMovingToGrapplePoint;


    public static HookManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        tp = GetComponent<ThirdPersonMovementController>();
    }

    
    private void Update()
    {
        ChechPoint();

        if (isMovingToGrapplePoint) ExecuteGrapple();
       


    }
    private void StopGrappleMovement()
    {
        isMovingToGrapplePoint = false;
        canGrapple = false;
        rb.useGravity = true;
        moveVelocity = Vector3.zero;
    }

    

    void ChechPoint()
    {
        
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        bool foundGrapple = Physics.SphereCast(ray, 1f, out grappleHit, maxDistance, grappleLayer);
       
        
        if (foundGrapple)
        {
            print("grapple");
            
            if (Input.GetKeyDown(KeyCode.E)) StartGrapple();
        }
    }
   
    private void StartGrapple()
    {
        
        canGrapple = true;
        targetPoint = grappleHit.collider.transform.position; 
        Vector3 lookDirection = targetPoint - transform.position;
        lookDirection.y = 0;         
        transform.rotation = Quaternion.LookRotation(lookDirection);
        
    }


    public void ExecuteGrapple()
    {
        Vector3 targetPosWithOffset = targetPoint + grappleOffset;


        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosWithOffset,
            ref moveVelocity,
            0.15f,
            grappleSpeed
        );


        float distance = Vector3.Distance(transform.position, targetPosWithOffset);
        if (distance < 0.5f)
        {
            StopGrappleMovement();
        }
    }

    public Vector3 GetGrapplePoint()
    {
        return targetPoint;
    }
   
}
