using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class Grappling : MonoBehaviour
{
    
    private PlayerMovement pm;

    public Camera cam;

    public Transform gunTip;

    public LayerMask whatIsGrappleable;

    public LineRenderer lr;   
    
    public float maxGrappleDistance;

    public float grappleDelayTime;

    public float overshootYAxis;

    private Vector3 grapplePoint;
    
    public float grapplingCd;

    private float grapplingCdTimer;    

    public KeyCode grappleKey;

    public GameObject grappleIndicator;

    public MultiAimConstraint headAimConstraint, bodyAimConstraint;

    public Transform aimTarget;

    public bool grappling;
    private void Start()
    {
        pm = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        RaycastHit hit;
        bool found = Physics.SphereCast(ray, 1f, out hit, maxGrappleDistance, whatIsGrappleable);

        if (found && hit.collider.CompareTag("Grappleable"))
        {
            grappleIndicator.SetActive(true);
            grappleIndicator.transform.position = cam.WorldToScreenPoint(hit.point);
            
            if (Input.GetKeyDown(grappleKey)) StartGrapple(hit);
        }
        else
        {            
            grappleIndicator.SetActive(false);
        }


        if (grapplingCdTimer > 0)
            grapplingCdTimer -= Time.deltaTime;
    }
    private void StartGrapple(RaycastHit hit)
    {
        if (grapplingCdTimer > 0) return;
        grappling = true;
        pm.freeze = true;
        grapplePoint = hit.point; 
        aimTarget.position = grapplePoint;
        pm.anm.SetBool("Fire", true);
        headAimConstraint.weight = 1f;
        bodyAimConstraint.weight = 1f;
        Vector3 lookDirection = grapplePoint - transform.position;
        lookDirection.y = 0;         
        pm.playerObj.transform.rotation = Quaternion.LookRotation(lookDirection);
        Invoke(nameof(ExecuteGrapple), grappleDelayTime);
    }


    private void ExecuteGrapple()
    {
        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

        if (grapplePointRelativeYPos < 0) highestPointOnArc = overshootYAxis;
        
        pm.JumpToPosition(grapplePoint, highestPointOnArc);
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