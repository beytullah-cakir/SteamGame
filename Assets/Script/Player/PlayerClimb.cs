using StarterAssets;
using System.Collections;
using System.Linq;
using UnityEngine;

public enum PlayerState
{
    NormalState,
    ClimbingState
}

public class PlayerClimb : MonoBehaviour
{
    private ThirdPersonController _playerController;
    private RoofLedgeDetection roofLedgeDetection;
    [HideInInspector] public Animator animator;
    public GameObject interactUI;

    [Header("Climbing State")]
    public PlayerState playerState = PlayerState.NormalState;
    public bool isClimbing;
    public bool canGrabLedge;
    public bool isZipped;

    [Header("Layer Settings")]
    public LayerMask ledgeLayer;


    [Header("Animation Match Target Offsets")]
    public float rayYHandCorrection = 0.5f;
    public float rayZHandCorrection = 0.5f;

    [Header("Hop Values")]
    public float leftUp = .4f;
    public float leftForward = .1f;
    public float rightUp, rightForward;
    public float hopUpUp, hopUpForward;
    public float hopDownUp, hopDownForward;
    public float jumpUp, jumpForward;

    private Vector3 climbTarget;
    private Quaternion targetRot;
    private float h, v;
    private bool isHopping;
    private bool isIdleLedgeBusy;

    private void Start()
    {
        _playerController = GetComponent<ThirdPersonController>();
        roofLedgeDetection = GetComponent<RoofLedgeDetection>();

        
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (isZipped || animator == null) return;

        StateConditionsCheck();
        CheckingMain();
        Inputs();

        if (isClimbing)
        {
            HopMove();
        }

        MatchTargetToLedge();

        if (interactUI != null)
            interactUI.SetActive(canGrabLedge);
    }
    public float sphereOffset;
    public float sphereRadius;
    public float maxDistance;
    
    private void CheckingMain()
    {
        if (isIdleLedgeBusy || isClimbing) return;

        Vector3 sphereOrigin=transform.position+Vector3.up*sphereOffset;

        RaycastHit hit;

        bool hitInfo = Physics.SphereCast(
            sphereOrigin,
            sphereRadius,
            transform.forward,
            out hit,
            maxDistance,
            ledgeLayer
            );
        Vector3 end=sphereOrigin+(transform.forward*maxDistance);
        Debug.DrawLine(sphereOrigin, end, Color.black);

        if (hitInfo)
        {
            canGrabLedge = true;
            climbTarget = hit.point;
            targetRot = Quaternion.LookRotation(hit.collider.transform.position);
            
            if (interactUI != null)
            {
                Vector3 uiPos = climbTarget + Vector3.up * 0.5f;
                interactUI.transform.position = Camera.main.WorldToScreenPoint(uiPos);
            }   
        }
        else
        {
            canGrabLedge = false;
            climbTarget = Vector3.zero;
        }
    }

    private void Inputs()
    {
        if (isHopping) return;

        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");

        //if (Input.GetKeyDown(KeyCode.Space) && !roofLedgeDetection.isRoofLedgeDetected)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isClimbing && canGrabLedge && climbTarget != Vector3.zero)
            {
                Vector3 lookDir = (climbTarget - transform.position).normalized;
                lookDir.y = 0;
                if (lookDir != Vector3.zero)
                    transform.rotation = Quaternion.LookRotation(lookDir);

                StartCoroutine(GrabLedge());
            }
        }

        if (Input.GetKeyDown(KeyCode.C) && isClimbing)
            StartCoroutine(DropLedge());
    }

    private void HopMove()
    {
        if (isHopping) return;

        if (isClimbing && Input.GetKeyDown(KeyCode.Space))
        {
            if (h > 0) StartCoroutine(HopRight());
            else if (h < 0) StartCoroutine(HopLeft());
            else if (v > 0) StartCoroutine(HopUp());
            else if (v < 0) StartCoroutine(HopDown());
        }
    }

    private void StateConditionsCheck()
    {
        //if (_playerController == null || _playerController.rb == null) return;

        if (playerState == PlayerState.NormalState)
        {
            //_playerController.rb.isKinematic = false;
            _playerController.enabled = true;
            animator.applyRootMotion = false;
        }
        else if (playerState == PlayerState.ClimbingState)
        {
            //_playerController.rb.isKinematic = true;
            _playerController.enabled = false;
            animator.applyRootMotion = true;
        }
    }

    private bool IsInState(AnimatorStateInfo stateInfo, string stateName)
    {
        return stateInfo.IsName(stateName) && !animator.IsInTransition(0);
    }

    private void ApplyMatchTarget(Vector3 offset, AvatarTarget target, Vector3 maskWeights)
    {
        animator.MatchTarget(
            climbTarget + transform.TransformDirection(offset),
            targetRot,
            target,
            new MatchTargetWeightMask(maskWeights, 0),
            0.36f, 0.57f
        );
    }

    private void MatchTargetToLedge()
    {
        if (climbTarget == Vector3.zero || !isClimbing) return;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (IsInState(stateInfo, "Idle To Braced Hang"))
            ApplyMatchTarget(Vector3.forward * rayZHandCorrection + Vector3.up * rayYHandCorrection,
                AvatarTarget.RightHand, new Vector3(0, 1, 1));

        if (IsInState(stateInfo, "Braced Hang Hop Up"))
            ApplyMatchTarget(Vector3.forward * hopUpForward + Vector3.up * hopUpUp,
                AvatarTarget.RightHand, new Vector3(1, 1, 1));

        if (IsInState(stateInfo, "Braced Hang Hop Right"))
            ApplyMatchTarget(Vector3.forward * rightForward + Vector3.up * rightUp,
                AvatarTarget.LeftHand, new Vector3(1, 1, 1));

        if (IsInState(stateInfo, "Braced Hang Hop Left"))
            ApplyMatchTarget(Vector3.forward * leftForward + Vector3.up * leftUp,
                AvatarTarget.RightHand, new Vector3(1, 1, 1));

        if (IsInState(stateInfo, "Braced Hang Hop Down"))
            ApplyMatchTarget(Vector3.forward * hopDownForward + Vector3.up * hopDownUp,
                AvatarTarget.RightHand, new Vector3(0, 1, 1));

        if (IsInState(stateInfo, "Hanging Idle"))
        {
            animator.MatchTarget(
               climbTarget + transform.TransformDirection(Vector3.forward * rayZHandCorrection + Vector3.up * rayYHandCorrection),
               targetRot,
               AvatarTarget.RightHand,
               new MatchTargetWeightMask(new Vector3(0, 1, 1), 0),
               0f, .1f
           );
        }
    }

    IEnumerator GrabLedge()
    {
        playerState = PlayerState.ClimbingState;
        isHopping = true;
        isClimbing = true;

        transform.rotation = targetRot;

        animator.CrossFade("Idle To Braced Hang", 0.1f);

        //float dur = animator.runtimeAnimatorController.animationClips
        //    .First(c => c.name == "Idle To Braced Hang").length;

        yield return new WaitForSeconds(1f);

        isHopping = false;
    }

    public IEnumerator DropLedge()
    {
        animator.CrossFade("Braced To Drop", 0.1f);
        yield return new WaitForSeconds(0.5f);

        playerState = PlayerState.NormalState;
        isClimbing = false;
        isHopping = false;
    }

    IEnumerator HopUp()
    {
        isHopping = true;
        animator.CrossFade("Braced Hang Hop Up", 0.1f);
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        isHopping = false;
    }

    IEnumerator HopRight()
    {
        isHopping = true;
        animator.CrossFade("Braced Hang Hop Right", 0.1f);
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        isHopping = false;
    }

    IEnumerator HopLeft()
    {
        isHopping = true;
        animator.CrossFade("Braced Hang Hop Left", 0.1f);
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
        

        Gizmos.color = canGrabLedge ? Color.green: Color.red;
        Vector3 sphereOrigin = transform.position + Vector3.up * sphereOffset;
        Gizmos.DrawWireSphere(sphereOrigin, sphereRadius);

        
        


        if (canGrabLedge && climbTarget != Vector3.zero)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(climbTarget, 0.05f);
        }
    }
}
