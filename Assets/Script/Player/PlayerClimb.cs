using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public enum PlayerState
{
    NormalState,
    ClimbingState
}

public class PlayerClimb : MonoBehaviour
{
    PlayerMovement thirdPersonController;
    RoofLedgeDetection roofLedgeDetection;
    public Animator animator;
    private LedgeToRoofClimb ledgeToRoofClimb;
    public bool isZipped = false;

    public PlayerState playerState;


    public bool isClimbing;
    public bool canGrabLedge;


    public LayerMask ledgeLayer;

    public float rayYHandCorrection;
    public float rayZHandCorrection;
    public float yDropToHangPos = -0.1f;
    public float zDropToHangPos = -0.05f;
    public float upHopPos = -0.1f;
    public float frowardHopPos = -0.05f;

    [Header("Climb Detection")] 
    public Vector3 boxHalfExtents = new Vector3(0.3f, 0.5f, 0.1f);
    public float detectDistance = 0.6f;
    private Vector3 climbTarget;
    private Collider ledge;
    private Vector3 center;
    public float checkX, checkY;
    public bool isHopDown;
    public Transform playerCenter;
    private float h, v;

    [Header("Hop Values")] public float leftUp = .4f;
    public float leftForward=.1f;
    public float rightUp, rightForward;
    public float hopUpUp, hopUpForward;
    public float hopDownUp, hopDownForward;

    private void Start()
    {
        playerState = PlayerState.NormalState;
        thirdPersonController = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();
        ledgeToRoofClimb = GetComponent<LedgeToRoofClimb>();
        roofLedgeDetection = GetComponent<RoofLedgeDetection>();
    }


    private void Update()
    {
        if (isZipped) return;

        StateConditionsCheck();
        CheckingMain();
        Inputs();
        MatchTargetToLedge();
        UpdateBoxPos();

        if (isClimbing) HopMove();
    }

    private void HopMove()
    {
        if (isHopDown && Input.GetKeyDown(KeyCode.Space))
        {
            //StartCoroutine(HopRight());
            switch (h,v)
            {
                case (1,0): StartCoroutine(HopRight()); break;
                case (-1,0): StartCoroutine(HopLeft()); break;
                case (0,1): StartCoroutine(HopUp()); break;
                case (0,-1): StartCoroutine(HopDown()); break;
                case (1,1): StartCoroutine(HopLeft()); break;
                case (-1,1): StartCoroutine(HopUp()); break;
                case (-1,-1): StartCoroutine(HopDown()); break;
                case (1,-1): StartCoroutine(HopDown()); break;
            }
        }
    }


    private void Inputs()
    {
        if (Input.GetKeyDown(KeyCode.Space) &&
            !roofLedgeDetection.isRoofLedgeDetected) // Pressing Button and roof ledge is not detected
        {
            if (!isClimbing) // if Not Climbing 
            {
                if (canGrabLedge && climbTarget != Vector3.zero)
                {
                    // 🔹 Kenara dön
                    Vector3 lookDir = (climbTarget - transform.position).normalized;
                    lookDir.y = 0; // sadece yatayda dönsün
                    if (lookDir != Vector3.zero)
                        transform.rotation = Quaternion.LookRotation(lookDir);

                    // 🔹 Tırmanmayı başlat
                    StartCoroutine(GrabLedge());
                }
            }
            
        }
        
        if(Input.GetKeyDown(KeyCode.C) && !roofLedgeDetection.isRoofLedgeDetected && isClimbing) StartCoroutine(DropLedge());
    }


    private Vector3 direction;

    void UpdateBoxPos()
    {
        Vector3 forward = playerCenter.forward;
        Vector3 right = playerCenter.right;
        Vector3 up = playerCenter.up;


        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");

        direction = right * h + up * v;

        if (isClimbing)
        {
            if (direction != Vector3.zero)
            {
                center = playerCenter.position + forward * detectDistance + direction * checkX;
                isHopDown = true;
            }
            else
            {
                center = playerCenter.position + forward * detectDistance + up * checkY;
                isHopDown = false;
            }
        }
        else
        {
            center = playerCenter.position + forward * detectDistance + up * checkY;
        }
    }


    private void CheckingMain()
    {
        Collider[] hits = Physics.OverlapBox(center, boxHalfExtents, transform.rotation, ledgeLayer);

        if (hits.Length > 0)
        {
            canGrabLedge = true;
            ledge = hits[0];
            climbTarget = ledge.ClosestPoint(transform.position);

            Debug.DrawLine(transform.position, climbTarget, Color.green);
        }
        else
        {
            canGrabLedge = false;
        }
    }

    private void StateConditionsCheck()
    {
        if (thirdPersonController.isClimbingEnd || thirdPersonController.isLadderClimbing ||
            thirdPersonController.isObjectPushing) return;
        if (playerState == PlayerState.NormalState)
        {
            animator.applyRootMotion = false;

            thirdPersonController.rb.isKinematic = false;
            thirdPersonController.enabled = true;
        }
        else if (playerState == PlayerState.ClimbingState)
        {
            animator.applyRootMotion = true;

            thirdPersonController.rb.isKinematic = true;
            thirdPersonController.enabled = false;
        }
    }


    private void MatchTargetToLedge()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle To Braced Hang") && !animator.IsInTransition(0))
        {
            Vector3 handPos = transform.forward * rayZHandCorrection + transform.up * rayYHandCorrection;
            animator.MatchTarget(climbTarget + handPos, transform.rotation, AvatarTarget.RightHand,
                new MatchTargetWeightMask(new Vector3(0, 1, 1), 0), 0.36f, 0.57f);
        }
        
        
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Braced Hang Hop Up") && !animator.IsInTransition(0))
        {
            Vector3 handDropPos = transform.forward * hopUpForward + transform.up * hopUpUp;

            animator.MatchTarget(climbTarget + handDropPos, transform.rotation, AvatarTarget.RightHand,
                new MatchTargetWeightMask(new Vector3(0, 1, 1), 0), 0.36f, 0.57f);
        }

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Braced Hang Hop Right") && !animator.IsInTransition(0))
        {
            Vector3 handDropPos = transform.forward * rightForward + transform.up * rightUp;
            animator.MatchTarget(climbTarget + handDropPos, transform.rotation, AvatarTarget.LeftHand,
                new MatchTargetWeightMask(new Vector3(1, 1, 1), 0), 0.36f, 0.57f);
        }

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Braced Hang Hop Left") && !animator.IsInTransition(0))
        {
            Vector3 handDropPos = transform.forward * leftForward + transform.up * leftUp;
            animator.MatchTarget(climbTarget + handDropPos, transform.rotation, AvatarTarget.RightHand,
                new MatchTargetWeightMask(new Vector3(1, 1, 1), 0), 0.36f, 0.57f);
        }
        
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Braced Hang Hop Down") && !animator.IsInTransition(0))
        {
            Vector3 handDropPos = transform.forward * hopDownForward + transform.up * hopDownUp;
            animator.MatchTarget(climbTarget + handDropPos, ledge.transform.rotation, AvatarTarget.RightHand,
                new MatchTargetWeightMask(new Vector3(0, 1, 1), 0), 0.31f, 0.56f);
        }
    }

    IEnumerator GrabLedge()
    {
        playerState = PlayerState.ClimbingState;
        isClimbing = true;
        animator.CrossFade("Idle To Braced Hang", 0);

        yield return null;
    }

    public IEnumerator DropLedge()
    {
        animator.CrossFade("Braced To Drop", 0);
        yield return new WaitForSeconds(.5f);
        playerState = PlayerState.NormalState;
        isClimbing = false;
    }

    IEnumerator HopUp()
    {
        animator.CrossFade("Braced Hang Hop Up", 0.2f);
        yield return null;
    }

    IEnumerator HopRight()
    {
        animator.CrossFade("Braced Hang Hop Right", 0);
        yield return null;
    }

    IEnumerator HopLeft()
    {
        animator.CrossFade("Braced Hang Hop Left", 0);
        yield return null;
    }

    IEnumerator HopDown()
    {
        animator.CrossFade("Braced Hang Hop Down", 0.2f);

        yield return null;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.matrix = Matrix4x4.TRS(center, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxHalfExtents * 2);
    }
}