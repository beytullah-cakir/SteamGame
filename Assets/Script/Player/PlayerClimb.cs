using System;
using System.Collections;
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

    public bool isZipped;
    public PlayerState playerState;

    public bool isClimbing;
    public bool canGrabLedge;
    public LayerMask ledgeLayer;

    public float rayYHandCorrection;
    public float rayZHandCorrection;

    public float boxX, boxY, boxZ;
    private Vector3 climbTarget;
    private Collider ledge;
    public Transform playerCenter;

    private float h, v;

    [Header("Hop Values")] public float leftUp = .4f;
    public float leftForward = .1f;
    public float rightUp, rightForward;
    public float hopUpUp, hopUpForward;
    public float hopDownUp, hopDownForward;

    private bool isHopDown;
    private bool isHopping;

    public float moveDistanceX, moveDistanceY,moveDistanceZ;
    public float sphereRadius;

    private void Start()
    {
        playerState = PlayerState.NormalState;
        thirdPersonController = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();
        roofLedgeDetection = GetComponent<RoofLedgeDetection>();
    }

    private void Update()
    {
        if (isZipped) return;

        StateConditionsCheck();
        CheckingMain();
        Inputs();
        MatchTargetToLedge();
        UpdatePos();    

        
        if (isClimbing) HopMove();
    }


    void UpdatePos()
    {
        if (isClimbing)
        {
            Vector3 inputDir = new Vector3(h*moveDistanceX, v*moveDistanceY,moveDistanceZ);
        
            sphereOffset = inputDir != new Vector3(0,0,moveDistanceZ) ? inputDir : new Vector3(0, moveDistanceY, moveDistanceZ);  
        }
        else
        {
            sphereOffset=new Vector3(0, moveDistanceY, moveDistanceZ);  
        }
        
        
    }


    private void HopMove()
    {
        if (isHopping) return; // 🔹 hop sırasında tekrar hop yapılamaz
        if (isHopDown && Input.GetKeyDown(KeyCode.Space))
        {
            switch (h, v)
            {
                case (1, 0): StartCoroutine(HopRight()); break;
                case (-1, 0): StartCoroutine(HopLeft()); break;
                case (0, 1): StartCoroutine(HopUp()); break;
                case (0, -1): StartCoroutine(HopDown()); break;
                case (1, 1): StartCoroutine(HopRight()); break;
                case (-1, 1): StartCoroutine(HopLeft()); break;
                case (-1, -1): StartCoroutine(HopDown()); break;
                case (1, -1): StartCoroutine(HopDown()); break;
            }
        }
    }

    private void Inputs()
    {
        if (isHopping) return; // 🔹 hop animasyonu boyunca inputları kilitle

        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.Space) && !roofLedgeDetection.isRoofLedgeDetected)
        {
            if (!isClimbing)
            {
                if (canGrabLedge && climbTarget != Vector3.zero)
                {
                    Vector3 lookDir = (climbTarget - transform.position).normalized;
                    lookDir.y = 0;
                    if (lookDir != Vector3.zero)
                        transform.rotation = Quaternion.LookRotation(lookDir);

                    StartCoroutine(GrabLedge());
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.C) && !roofLedgeDetection.isRoofLedgeDetected && isClimbing)
            StartCoroutine(DropLedge());
    }


    public Vector3 sphereOffset;

    private void CheckingMain()
    {
        // Çakışan collider’ları kontrol et
        Collider[] hits = Physics.OverlapSphere(playerCenter.position+sphereOffset, sphereRadius, ledgeLayer);

        if (hits.Length > 0)
        {
            canGrabLedge = true;
            ledge = hits[0];
            climbTarget = ledge.ClosestPoint(transform.position);
            isHopDown = true;

            Debug.DrawLine(transform.position, climbTarget, Color.green);
        }
        else
        {
            canGrabLedge = false;
            isHopDown = false;
        }
    }


    private void StateConditionsCheck()
    {
        if (thirdPersonController.isClimbingEnd || thirdPersonController.isLadderClimbing ||
            thirdPersonController.isObjectPushing) return;

        if (playerState == PlayerState.NormalState)
        {
            thirdPersonController.rb.isKinematic = false;
            thirdPersonController.enabled = true;
        }
        else if (playerState == PlayerState.ClimbingState)
        {
            thirdPersonController.rb.isKinematic = true;
            thirdPersonController.enabled = false;
        }
    }

    // 🔹 Tek fonksiyonla MatchTarget yönetimi
    private void MatchTargetToLedge()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (IsInState(stateInfo, "Idle To Braced Hang"))
            ApplyMatchTarget(Vector3.forward * rayZHandCorrection + Vector3.up * rayYHandCorrection,
                AvatarTarget.RightHand, new Vector3(0, 1, 1));

        if (IsInState(stateInfo, "Braced Hang Hop Up"))
            ApplyMatchTarget(Vector3.forward * hopUpForward + Vector3.up * hopUpUp, AvatarTarget.RightHand,
                new Vector3(1, 1, 1));

        if (IsInState(stateInfo, "Braced Hang Hop Right"))
            ApplyMatchTarget(Vector3.forward * rightForward + Vector3.up * rightUp, AvatarTarget.LeftHand,
                new Vector3(1, 1, 1));

        if (IsInState(stateInfo, "Braced Hang Hop Left"))
            ApplyMatchTarget(Vector3.forward * leftForward + Vector3.up * leftUp, AvatarTarget.RightHand,
                new Vector3(1, 1, 1));

        if (IsInState(stateInfo, "Braced Hang Hop Down"))
            ApplyMatchTarget(Vector3.forward * hopDownForward + Vector3.up * hopDownUp, AvatarTarget.RightHand,
                new Vector3(0, 1, 1));
    }

    private bool IsInState(AnimatorStateInfo stateInfo, string stateName)
    {
        return stateInfo.IsName(stateName) && !animator.IsInTransition(0);
    }

    private void ApplyMatchTarget(Vector3 offset, AvatarTarget target, Vector3 maskWeights)
    {
        animator.MatchTarget(
            climbTarget + transform.TransformDirection(offset),
            transform.rotation,
            target,
            new MatchTargetWeightMask(maskWeights, 0),
            0.36f, 0.57f
        );
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

    // 🔹 Tüm hop animasyonları sırasında input kilitleniyor
    IEnumerator HopUp()
    {
        isHopping = true;
        animator.CrossFade("Braced Hang Hop Up", 0);
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        isHopping = false;
    }

    IEnumerator HopRight()
    {
        isHopping = true;
        animator.CrossFade("Braced Hang Hop Right", 0);
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        isHopping = false;
    }

    IEnumerator HopLeft()
    {
        isHopping = true;
        animator.CrossFade("Braced Hang Hop Left", 0);
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        isHopping = false;
    }

    IEnumerator HopDown()
    {
        isHopping = true;
        animator.CrossFade("Braced Hang Hop Down", 0.2f);
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        isHopping = false;
    }

    private void OnDrawGizmos()
    {
        if (playerCenter == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(playerCenter.position+sphereOffset, sphereRadius);
    }
}