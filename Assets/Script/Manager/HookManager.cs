using StarterAssets;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

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

    //[Header("Rigging Settings")]
    //public Rig spineRig,armRig;          // Animator hiyerarþisindeki Rig objesi
    //public Transform rigTarget;     // Oluþturduðunuz RigTarget objesi
    //public float rigWeightSpeed = 5f; // Bakýþýn yumuþaklýðý


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
        rb.useGravity = true;
        tp.freezeMovement = false;
        moveVelocity = Vector3.zero;
    }

    

    void ChechPoint()
    {
        
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        bool foundGrapple = Physics.SphereCast(ray, 1f, out grappleHit, maxDistance, grappleLayer);
       
        
        if (foundGrapple)
        {
            print("grapple");
            
            if (Input.GetKeyDown(KeyCode.E)) StartCoroutine(StartGrapple());
        }
    }


   
    private IEnumerator StartGrapple()
    {
        targetPoint = grappleHit.collider.transform.position;
        //if (rigTarget != null) rigTarget.position = targetPoint;
        tp.freezeMovement = true;
        Vector3 lookDirection = targetPoint - transform.position;
        lookDirection.y = 0;
        transform.rotation = Quaternion.LookRotation(lookDirection);

        //spineRig.weight = 1;
        //armRig.weight = 1;
        tp._animator.Play("Gunplay");
        
        yield return new WaitForSeconds(1.5f);
        


    }
    public void ThrowRope()
    {
        canGrapple = true;
    }



    public void ExecuteGrapple()
    {
        tp._animator.SetTrigger("GrappleEnd");
        //spineRig.weight = 0;
        //armRig.weight = 0;
        Vector3 targetPosWithOffset = targetPoint + grappleOffset;
        canGrapple = false;        
        tp._animator.SetBool("Grounded", false);
        tp._animator.SetBool("FreeFall", true);
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
