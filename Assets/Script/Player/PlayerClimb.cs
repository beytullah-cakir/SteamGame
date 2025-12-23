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
    private CharacterController _characterController;
    private StarterAssetsInputs _input;
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

    [Header("Climbing Settings")]
    public float shimmySpeed = 2.0f;
    public float sphereOffset = 1.5f;
    public float sphereRadius = 0.3f;
    public float maxDistance = 0.5f;

    [Header("Animation Match Target Offsets")]
    public float rayYHandCorrection = 0.4f;
    public float rayZHandCorrection = 0.2f;

    [Header("Hop Values")]
    public float leftUp = .4f;
    public float leftForward = .1f;
    public float rightUp, rightForward;
    public float hopUpUp, hopUpForward;
    public float hopDownUp, hopDownForward;
    public float jumpUp, jumpForward;

    private Vector3 climbTarget;
    private Vector3 currNormal;
    private Quaternion targetRot;
    private float h, v;
    public bool isHopping;
    private bool isIdleLedgeBusy;

    private void Start()
    {
        _playerController = GetComponent<ThirdPersonController>();
        _characterController = GetComponent<CharacterController>();
        _input = GetComponent<StarterAssetsInputs>();
        roofLedgeDetection = GetComponent<RoofLedgeDetection>();
        animator = GetComponent<Animator>();
        
        // Ensure we start in normal state
        playerState = PlayerState.NormalState;
        isClimbing = false;
        
        // Debug check
        if (_playerController == null) Debug.LogError("PlayerClimb: ThirdPersonController not found!");
        if (_characterController == null) Debug.LogError("PlayerClimb: CharacterController not found!");
    }

    private void Update()
    {
        if (isZipped || animator == null) return;

        StateConditionsCheck();
        
        CheckingMain();
        
        Inputs();

        if (isClimbing)
        {
            // If moving (shimmy), disable root motion and manual correction to allow ShimmyController to move transform
            if (Mathf.Abs(h) > 0.1f)
            {
                animator.applyRootMotion = false;
            }
            else
            {
                // Idle or Hopping
                animator.applyRootMotion = true;
                if (!isHopping)
                {
                   PositionCorrection();
                }
            }
        }

        MatchTargetToLedge();

        if (interactUI != null)
            interactUI.SetActive(canGrabLedge && !isClimbing);
    }
    
    private void CheckingMain()
    {
        if (isIdleLedgeBusy) return;

        Vector3 sphereOrigin = transform.position + Vector3.up * sphereOffset;
        Vector3 direction = transform.forward;
        
        if(Physics.SphereCast(sphereOrigin, sphereRadius, direction, out RaycastHit hit, maxDistance, ledgeLayer))
        {
            canGrabLedge = true;
            
            // If we are not shimmying vigorously, update the target to what's directly in front
            if (!isClimbing || (!isHopping && Mathf.Abs(h) < 0.1f)) 
            {
                climbTarget = hit.point;
                currNormal = hit.normal;
                targetRot = Quaternion.LookRotation(-hit.normal);
            }
            
            if (interactUI != null && !isClimbing)
            {
                Vector3 uiPos = hit.point + Vector3.up * 0.5f;
                interactUI.transform.position = Camera.main.WorldToScreenPoint(uiPos);
            }
        }
        else
        {
            if (!isClimbing) canGrabLedge = false;
        }
        
        Debug.DrawLine(sphereOrigin, sphereOrigin + (direction * maxDistance), canGrabLedge ? Color.green : Color.black);
    }

    private void PositionCorrection()
    {
        if (climbTarget == Vector3.zero || currNormal == Vector3.zero) return;
        
        // Flatten rotation only - Position is handled by MatchTarget
        Vector3 flatNormal = new Vector3(-currNormal.x, 0f, -currNormal.z).normalized;
        if (flatNormal != Vector3.zero)
        {
            Quaternion uprightRot = Quaternion.LookRotation(flatNormal);
            transform.rotation = Quaternion.Slerp(transform.rotation, uprightRot, Time.deltaTime * 10f);
        }
    }

    // Checking logic for Shimmy (moved to ShimmyController but kept if needed for reference, though unused here)
    private bool CheckLedgeInDirection(Vector3 dir, out Vector3 point, out Vector3 normal, out Quaternion rot)
    {
        point = climbTarget;
        normal = currNormal;
        rot = targetRot;
        
        // Check from future position - see if wall still exists
        Vector3 futurePos = transform.position + dir;
        Vector3 checkOrigin = futurePos + Vector3.up * sphereOffset;
        
        // Use -currNormal (direction INTO the wall)
        Vector3 wallDir = -currNormal;
        if (wallDir == Vector3.zero) wallDir = transform.forward;
        
        // SphereCast to detect wall
        if (Physics.SphereCast(checkOrigin, sphereRadius, wallDir, out RaycastHit hit, maxDistance + 0.3f, ledgeLayer))
        {
            point = hit.point;
            normal = hit.normal;
            rot = Quaternion.LookRotation(-hit.normal);
            return true;
        }
        
        return false;
    }

    private void Inputs()
    {
        if (isHopping) return;

        // Use new input system values
        if (_input != null)
        {
            h = _input.move.x;
            v = _input.move.y;
        }
        else
        {
            // Fallback
            h = Input.GetAxisRaw("Horizontal");
            v = Input.GetAxisRaw("Vertical");
        }

        if (_input != null && _input.jump) 
        {
            if (!isClimbing && canGrabLedge)
            {
                 _input.jump = false; 
                Vector3 lookDir = (climbTarget - transform.position).normalized;
                lookDir.y = 0;
                if (lookDir != Vector3.zero)
                    transform.rotation = Quaternion.LookRotation(lookDir);

                StartCoroutine(GrabLedge());
            }
            else if (isClimbing)
            {
                 if (v < -0.5f) {
                      _input.jump = false;
                      StartCoroutine(DropLedge()); 
                 }
                 else if (Mathf.Abs(h) > 0.5f) {
                     _input.jump = false;
                     if(h > 0) StartCoroutine(HopRight());
                     else StartCoroutine(HopLeft());
                 }
                 else {
                      // Default hop up
                      _input.jump = false;
                      StartCoroutine(HopUp());
                 }
            }
        }

        if (Input.GetKeyDown(KeyCode.C) && isClimbing)
            StartCoroutine(DropLedge());
    }

    // ShimmyLogic removed, functionality moved to ShimmyController.cs

    private void StateConditionsCheck()
    {
        if (_playerController == null || _characterController == null) return;
        
        if (playerState == PlayerState.NormalState)
        {
            if (!_playerController.enabled) _playerController.enabled = true;
            if (!_characterController.enabled) _characterController.enabled = true;
            animator.applyRootMotion = false;
        }
        else if (playerState == PlayerState.ClimbingState)
        {
            if (_playerController.enabled) _playerController.enabled = false;
            // Keep CharacterController enabled - we control movement manually
            animator.applyRootMotion = true;
        }
    }

    private bool IsInState(AnimatorStateInfo stateInfo, string stateName)
    {
        return stateInfo.IsName(stateName) && !animator.IsInTransition(0);
    }

    private void ApplyMatchTarget(Vector3 offset, AvatarTarget target, Vector3 maskWeights)
    {
        // Safety check
        if (animator.isMatchingTarget) return;

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
        
        // Hanging Idle - continuous matching to keep hands on ledge
        // ONLY valid if we are NOT shimmying (moving sideways)
        if (IsInState(stateInfo, "Hanging Idle") && Mathf.Abs(h) < 0.1f)
        {
            ApplyMatchTarget(Vector3.forward * rayZHandCorrection + Vector3.up * rayYHandCorrection,
                AvatarTarget.RightHand, new Vector3(0, 1, 1));
        }

        // Hops use MatchTarget for precision
        if (IsInState(stateInfo, "Braced Hang Hop Up"))
             ApplyMatchTarget(Vector3.forward * hopUpForward + Vector3.up * hopUpUp, AvatarTarget.RightHand, new Vector3(1, 1, 1));

        if (IsInState(stateInfo, "Braced Hang Hop Right"))
            ApplyMatchTarget(Vector3.forward * rightForward + Vector3.up * rightUp, AvatarTarget.LeftHand, new Vector3(1, 1, 1));

        if (IsInState(stateInfo, "Braced Hang Hop Left"))
             ApplyMatchTarget(Vector3.forward * leftForward + Vector3.up * leftUp, AvatarTarget.RightHand, new Vector3(1, 1, 1));

        if (IsInState(stateInfo, "Braced Hang Hop Down"))
             ApplyMatchTarget(Vector3.forward * hopDownForward + Vector3.up * hopDownUp, AvatarTarget.RightHand, new Vector3(0, 1, 1));

    }

    IEnumerator GrabLedge()
    {
        playerState = PlayerState.ClimbingState;
        isHopping = true;
        isClimbing = true;
        
        transform.rotation = targetRot;

        animator.CrossFade("Idle To Braced Hang", 0.1f);

        yield return new WaitForSeconds(0.5f); // Reduced wait time for responsiveness

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
        // Debug visualization
        Vector3 sphereOrigin = transform.position + Vector3.up * sphereOffset;
        Gizmos.color = canGrabLedge ? Color.green : Color.red;
        Gizmos.DrawWireSphere(sphereOrigin, sphereRadius);
        Gizmos.DrawLine(sphereOrigin, sphereOrigin + transform.forward * maxDistance);

        if (canGrabLedge)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(climbTarget, 0.05f);
        }
    }
}
