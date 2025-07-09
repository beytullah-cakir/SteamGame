using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grappling : MonoBehaviour
{
    [Header("References")]
    private PlayerMovement pm;
    public Transform cam;
    public Transform gunTip;
    public LayerMask whatIsGrappleable;
    public LineRenderer lr;

    [Header("Grappling")]
    public float maxGrappleDistance;
    public float grappleDelayTime;
    public float overshootYAxis;

    private Vector3 grapplePoint;

    [Header("Cooldown")]
    public float grapplingCd;
    private float grapplingCdTimer;
    public bool stopGrapple = false;
    
    private KeyCode grappleKey = KeyCode.Mouse0;

    private bool grappling;

    private void Start()
    {
        pm = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(grappleKey)) StartGrapple();
        if (stopGrapple) StopGrapple();
        if (grapplingCdTimer > 0)
            grapplingCdTimer -= Time.deltaTime;
    }
    private void StartGrapple()
    {
        if (grapplingCdTimer > 0) return;        

        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance, whatIsGrappleable))
        {
            grapplePoint = hit.point;           

            pm.freeze = true;
            grappling = true;
            Invoke(nameof(ExecuteGrapple), grappleDelayTime);
            Invoke(nameof(ChangeValue), grappleDelayTime*2);
        }      

        
    }
    private void ChangeValue()=> stopGrapple = true;    

    private void ExecuteGrapple()
    {        
        lr.enabled = true;
        
        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

        if (grapplePointRelativeYPos < 0) highestPointOnArc = overshootYAxis;

        pm.JumpToPosition(grapplePoint, highestPointOnArc);
        
        
    }

    public void StopGrapple()
    {
        if (pm.freeze && pm.isGrounded)
        {
            pm.freeze = false;

            grappling = false;

            grapplingCdTimer = grapplingCd;
            stopGrapple = false;
            lr.enabled = false;
        }
        
    }

    public bool IsGrappling()
    {
        return grappling;
    }

    public Vector3 GetGrapplePoint()
    {
        return grapplePoint;
    }
}