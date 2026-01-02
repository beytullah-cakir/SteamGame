using System;
using UnityEngine;

public class HookManager : MonoBehaviour
{
    public Transform gunTip;
    public LayerMask grappleLayer,swingLayer;
    public float maxDistance;
    protected Vector3 targetPoint;
    public GameObject indicator;
    protected RaycastHit grappleHit,swingHit;
    protected Rigidbody rb;
    //protected PlayerMovement pm;
    public bool isGrappling;
    public bool isSwinging;
    
    
    
    
    public float grappleDelayTime;
    public float overshootYAxis;
    private float grapplingCdTimer;
    
    
    
    
    
    public float jointSpring = 4.5f;
    public float jointDamper = 7f;
    public float jointMassScale = 4.5f;
    private SpringJoint joint;
    public float horizontalThrustForce;
    public float forwardThrustForce;
    public float extendCableSpeed;


    protected virtual void Update()
    {
        ChechPoint();
        
        
        if (joint != null) OdmGearMovement();
        if (grapplingCdTimer > 0)
            grapplingCdTimer -= Time.deltaTime;
    }

    protected void Start()
    {
        rb = GetComponent<Rigidbody>();
        //pm = GetComponent<PlayerMovement>();
    }

    void ChechPoint()
    {
        
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        bool foundGrapple = Physics.SphereCast(ray, 1f, out grappleHit, maxDistance, grappleLayer);
        bool foundSwing = Physics.SphereCast(ray, 1f, out swingHit, maxDistance, swingLayer);
        indicator.SetActive(foundSwing || foundGrapple);
        if (foundGrapple)
        {
            print("grapple");
            indicator.transform.position = grappleHit.point;
            if (Input.GetKeyDown(KeyCode.E)) StartGrapple();
        }

        if (foundSwing)
        {
            
            indicator.transform.position = swingHit.point;
            if (Input.GetKeyDown(KeyCode.E)) StartSwing();
        }
    }
    private void StartGrapple()
    {
        if (grapplingCdTimer > 0) return;
        isGrappling = true;
        StopSwing();
        //pm.freeze = true;
        targetPoint = grappleHit.point; 
        Vector3 lookDirection = targetPoint - transform.position;
        lookDirection.y = 0;         
        transform.rotation = Quaternion.LookRotation(lookDirection);
        Invoke(nameof(ExecuteGrapple), grappleDelayTime);
    }


    private void ExecuteGrapple()
    {
        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeYPos = targetPoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

        if (grapplePointRelativeYPos < 0) highestPointOnArc = overshootYAxis;
        
        JumpToPosition(targetPoint, highestPointOnArc);
    }
    
    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        Vector3 offset = new Vector3(0, 2f, 0); // Biraz yukarıya nişanla
        Vector3 velocityToSet = CalculateJumpVelocity(transform.position, targetPosition + offset, trajectoryHeight);
        rb.linearVelocity = velocityToSet;
        isGrappling = false;
        //pm.freeze = false;
    }
    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float g = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * g * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / g)
                                               + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / g));

        return velocityXZ + velocityY;
    }
   

    public Vector3 GetGrapplePoint()
    {
        return targetPoint;
    }
    
    private void StartSwing()
    {
        if (joint != null) return;
        if (grappleHit.point == Vector3.zero) return;
        
        isSwinging = true;
        targetPoint = swingHit.point;
        joint = gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = targetPoint;

        float distanceFromPoint = Vector3.Distance(transform.position, targetPoint);

        // the distance grapple will try to keep from grapple point. 
        joint.maxDistance = distanceFromPoint * 0.8f;
        joint.minDistance = distanceFromPoint * 0.25f;

        // customize values as you like
        joint.spring = jointSpring;
        joint.damper = jointDamper;
        joint.massScale = jointMassScale;

        if (Input.GetKeyUp(KeyCode.Space)) StopSwing();
    }

    public void StopSwing()
    {
        isSwinging = false;
        Destroy(joint);
    }

    private void OdmGearMovement()
    {
        if (Input.GetKey(KeyCode.W)) rb.AddForce(transform.forward * horizontalThrustForce * Time.deltaTime);

        // shorten cable
        if (Input.GetKey(KeyCode.Space))
        {
            Vector3 directionToPoint = targetPoint - transform.position;
            rb.AddForce(directionToPoint.normalized * forwardThrustForce * Time.deltaTime);

            float distanceFromPoint = Vector3.Distance(transform.position, targetPoint);

            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;
        }

        // extend cable
        if (Input.GetKey(KeyCode.S))
        {
            float extendedDistanceFromPoint = Vector3.Distance(transform.position, targetPoint) + extendCableSpeed;

            joint.maxDistance = extendedDistanceFromPoint * 0.8f;
            joint.minDistance = extendedDistanceFromPoint * 0.25f;
        }
    }

    

    public Vector3 GetSwingingPoint()
    {
        return targetPoint;
    }
}
