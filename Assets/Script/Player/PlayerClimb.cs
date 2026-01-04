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
    private ThirdPersonMovementController _playerController;
    private Rigidbody _rb;
    private ThirdPersonInputSystem _inputSystem;
    private ShimmyController shimmyController;
    public Animator animator;
    public GameObject interactUI;
    

    [Header("Climbing State")]
    public PlayerState playerState = PlayerState.NormalState;
    public bool isClimbing;
    public bool canGrabLedge;
    public bool isZipped;

    [Header("Layer Settings")]
    public LayerMask ledgeLayer;

    [Header("Climbing Settings")]
    public Vector3 sphereOffset = new Vector3(0, 1.5f, 0);
    public float sphereRadius = 0.3f;
    public float capsuleLength = 0.7f;
    
    [Header("Airborne Chest Detection")]
    public Vector3 chestDetectionOffset = new Vector3(0, 1.2f, 0);
    public float chestSphereRadius = 0.25f;
    public float chestMaxDistance = 0.6f;

    [Header("Animation Offsets (X: Side, Y: Up, Z: Forward)")]
    public Vector3 idleToHangOffset;
    public Vector3 hangingIdleOffset;
    public Vector3 hopUpOffset;
    public Vector3 hopRightOffset;
    public Vector3 hopLeftOffset;
    public Vector3 hopDownOffset;
    public Vector3 roofLedgeOffset;

    private Vector3 climbTarget;
    private Quaternion targetRot;
    private Collider _currentLedge;
    private Collider _detectedLedge;
    private float h, v;
    public bool isHopping;
    private bool isJumpingEdge;

    private void Awake()
    {
        _inputSystem = new ThirdPersonInputSystem();
    }

    private void OnEnable() => _inputSystem.Enable();
    private void OnDisable() => _inputSystem.Disable();

    private void Start()
    {
        _playerController = GetComponent<ThirdPersonMovementController>();
        _rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        shimmyController = GetComponent<ShimmyController>();
        
    }

    private void Update()
    {
        if (isZipped || animator == null) return;

        StateConditionsCheck();

        Inputs();

        CheckingMain();

        MatchTargetToLedge();

        if (interactUI != null)
            interactUI.SetActive(canGrabLedge && !isClimbing);

        ChecLedgeAirborne();

        LedgeToCrouch();
    }


    void ChecLedgeAirborne()
    {
        
        if (!isClimbing && !isHopping && !_playerController.isGrounded)
        {
            // Göğüs hizasındaki yerel ofseti dünya koordinatına çeviriyoruz
            Vector3 chestOrigin = transform.TransformPoint(chestDetectionOffset);

            // SphereCast yerine OverlapSphere kullanarak doğrudan çarpışma kontrolü yapıyoruz
            Collider[] colliders = Physics.OverlapSphere(chestOrigin, chestSphereRadius, ledgeLayer);

            if (colliders.Length > 0)
            {
                canGrabLedge = true;
                _detectedLedge = colliders[0];
                
                // Kenar üzerindeki en yakın noktayı hedef al
                climbTarget = _detectedLedge.ClosestPoint(chestOrigin);
                
                // Kenarın kendi rotasyonunu ve yönünü baz al (CheckingMain ile aynı mantık)
                targetRot = _detectedLedge.transform.rotation;
                

                

                // Karakterin rotasyonunu anında düzelt
                Vector3 currentRot = targetRot.eulerAngles;
                currentRot.x = 0;
                currentRot.z = 0;
                transform.rotation = Quaternion.Euler(currentRot);

                StartCoroutine(GrabLedgeAirborne());
            }
        }
    }

    private void CheckingMain()
    {
        if (isJumpingEdge) return;

        Vector3 baseOrigin = transform.TransformPoint(sphereOffset);
        Vector3 inputDir = (transform.right * h + transform.up * v).normalized;

        // 1. Kapsül noktalarını belirle (Tek satırda if-else temizliği)
        Vector3 point2 = (isClimbing && inputDir.sqrMagnitude > 0.01f)
            ? baseOrigin + inputDir * capsuleLength
            : baseOrigin + transform.up * capsuleLength;

        // 2. Gereksiz allocation (bellek kullanımı) engellemek için NonAlloc kullanılabilir 
        // Ama basitlik için OverlapCapsule üzerinden devam edip mesafe kontrolünü optimize edelim
        Collider[] colliders = Physics.OverlapCapsule(baseOrigin, point2, sphereRadius, ledgeLayer);

        if (colliders.Length == 0)
        {
            ResetLedgeDetection();
            return;
        }

        // 3. En yakın ledge'i bul (LINQ yerine düz döngü daha performanslıdır)
        Collider bestLedgeCol = null;
        Vector3 bestLedgePoint = Vector3.zero;
        float minDistanceSqr = float.MaxValue; // Square distance kullanmak daha hızlıdır

        foreach (var col in colliders)
        {
            if (isClimbing && col == _currentLedge) continue;

            Vector3 closest = col.ClosestPoint(baseOrigin); // transform.position yerine baseOrigin daha tutarlı
            float distSqr = (baseOrigin - closest).sqrMagnitude;

            if (distSqr < minDistanceSqr)
            {
                minDistanceSqr = distSqr;
                bestLedgeCol = col;
                bestLedgePoint = closest;
            }
        }

        // 4. Sonuçları Uygula
        if (bestLedgeCol != null)
        {
            ApplyLedgeData(bestLedgeCol, bestLedgePoint);
        }
        else
        {
            ResetLedgeDetection();
        }
    }

    private void ApplyLedgeData(Collider col, Vector3 point)
    {
        _detectedLedge = col;
        canGrabLedge = true;

        if (!isClimbing || isHopping)
        {
            climbTarget = point;
            targetRot = col.transform.rotation;
            
        }

        if (interactUI != null && !isClimbing)
        {
            interactUI.SetActive(true);
            Vector3 uiPos = point + Vector3.up * 0.5f;
            interactUI.transform.position = Camera.main.WorldToScreenPoint(uiPos);
        }
    }

    private void ResetLedgeDetection()
    {
        _detectedLedge = null;
        canGrabLedge = false;
        if (interactUI != null) interactUI.SetActive(false);
    }

    private void Inputs()
    {
        if (isHopping) return;

        // Use new input system values
        Vector2 moveInput = _inputSystem.Player.Move.ReadValue<Vector2>();
        h = moveInput.x;
        v = moveInput.y;

        // Tırmanışa giriş veya tırmanırken zıplama (Jump Action)
        if (_inputSystem.Player.Jump.WasPressedThisFrame())
        {
            if (canGrabLedge)
            {
                if (!isClimbing)
                {
                    // Airborne grab ile aynı mantık: Anında dondur ve rotasyonu kilitle
                    playerState = PlayerState.ClimbingState;
                    if (_playerController != null) _playerController.freezeMovement = true;
                    _rb.isKinematic = true;

                    Vector3 rot = targetRot.eulerAngles;
                    rot.x = 0;
                    rot.z = 0;
                    transform.rotation = Quaternion.Euler(rot);

                    StartCoroutine(GrabLedge());
                }
                else if (isClimbing)
                {
                    // Tırmanırken giriş yönüne göre zıplama
                    if (v > 0.3f) StartCoroutine(HopUp());
                    else if (v < -0.3f) StartCoroutine(HopDown());
                    else if (h > 0.3f) StartCoroutine(HopRight());
                    else if (h < -0.3f) StartCoroutine(HopLeft());
                    else StartCoroutine(HopUp());
                }
            }
        }

        // Bırakma kontrolü (Crouch Action - "C" tuşu yerine Crouch aksiyonunu kullanıyoruz)
        if (_inputSystem.Player.Crouch.WasPressedThisFrame() && isClimbing)
        {
            StartCoroutine(DropLedge());
        }
    }



    private void StateConditionsCheck()
    {
        if (_playerController == null || _rb == null) return;

        if (playerState == PlayerState.NormalState)
        {
            _playerController.freezeMovement = false;
            _rb.isKinematic = false;
            animator.applyRootMotion = false;
        }
        else if (playerState == PlayerState.ClimbingState)
        {
            _playerController.freezeMovement = true;

            _rb.isKinematic = true;

            animator.applyRootMotion = true;
        }
    }

    private void LedgeToCrouch()
    {
        if (shimmyController.isCrouchLedge && _inputSystem.Player.Jump.WasPressedThisFrame() && isClimbing)
        {
            StartCoroutine(LedgeToClimb());
        }
    }

    private bool IsInState(AnimatorStateInfo stateInfo, string stateName)
    {
        return stateInfo.IsName(stateName) && !animator.IsInTransition(0);
    }

    private void ApplyMatchTarget(AvatarTarget target, Vector3 maskWeights, float start, float end, Vector3 offset)
    {
        // Safety check
        if (animator.isMatchingTarget) return;

        // Apply offset relative to the target rotation
        Vector3 targetPos = climbTarget + (targetRot * offset);

        animator.MatchTarget(
            targetPos,
            targetRot,
            target,
            new MatchTargetWeightMask(maskWeights, 0),
            start,
            end
        );
    }

    
    

    private void MatchTargetToLedge()
    {
        if (climbTarget == Vector3.zero || !isClimbing) return;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (IsInState(stateInfo, "Idle To Braced Hang"))
            ApplyMatchTarget(AvatarTarget.RightHand, new Vector3(0, 1, 1), 0.36f, 0.57f, idleToHangOffset);

        if (IsInState(stateInfo, "Hanging Idle In Air"))
            ApplyMatchTarget(AvatarTarget.RightHand, new Vector3(0, 1, 1), 0.1f, 0.9f, hangingIdleOffset);

        if (IsInState(stateInfo, "Braced Hang Hop Up"))
            ApplyMatchTarget(AvatarTarget.RightHand, new Vector3(0, 1, 1), 0.1f, 0.9f, hopUpOffset);

        if (IsInState(stateInfo, "Braced Hang Hop Right"))
            ApplyMatchTarget(AvatarTarget.LeftHand, new Vector3(1, 1, 1), 0.1f, 0.9f, hopRightOffset);

        if (IsInState(stateInfo, "Braced Hang Hop Left"))
            ApplyMatchTarget(AvatarTarget.RightHand, new Vector3(1, 1, 1), 0.33f, 0.85f, hopLeftOffset);

        if (IsInState(stateInfo, "Braced Hang Hop Down"))
            ApplyMatchTarget(AvatarTarget.RightHand, new Vector3(0, 1, 1), 0.1f, 0.9f, hopDownOffset);
        if (IsInState(stateInfo, "Braced Hang To Crouch"))
            ApplyMatchTarget(AvatarTarget.RightFoot, new Vector3(0, 1, 1), 0.41f, 0.87f,roofLedgeOffset);
    }

    IEnumerator GrabLedge()
    {
        isHopping = true;
        isClimbing = true;

        _currentLedge = _detectedLedge;
        
        animator.Play("Idle To Braced Hang");

        yield return new WaitForSeconds(0.5f); // Reduced wait time for responsiveness

        isHopping = false;
    }

    IEnumerator GrabLedgeAirborne()
    {
        playerState = PlayerState.ClimbingState;
        _playerController.freezeMovement = true;
        _rb.isKinematic = true;
        isHopping = true;
        isClimbing = true;
        isJumpingEdge = true;

        _currentLedge = _detectedLedge;
        

        animator.Play("Hanging Idle In Air");

        yield return new WaitForSeconds(.5f);

        isJumpingEdge = false;
        isHopping = false;
    }

    IEnumerator DropLedge()
    {
        animator.Play("Braced To Drop");
        yield return new WaitForSeconds(0.5f);

        _currentLedge = null;
        _detectedLedge = null;
        playerState = PlayerState.NormalState;
        isClimbing = false;
        isHopping = false;
    }

    IEnumerator HopUp()
    {
        isHopping = true;
        animator.Play("Braced Hang Hop Up");
        yield return new WaitForSeconds(1);
        _currentLedge = _detectedLedge;
        isHopping = false;
    }

    IEnumerator HopRight()
    {
        isHopping = true;
        animator.Play("Braced Hang Hop Right");
        yield return new WaitForSeconds(1);
        _currentLedge = _detectedLedge;
        isHopping = false;
    }

    IEnumerator HopLeft()
    {
        isHopping = true;
        animator.Play("Braced Hang Hop Left");
        yield return new WaitForSeconds(1);
        _currentLedge = _detectedLedge;
        isHopping = false;
    }

    IEnumerator HopDown()
    {
        isHopping = true;
        animator.Play("Braced Hang Hop Down");
        yield return new WaitForSeconds(1);
        _currentLedge = _detectedLedge;
        isHopping = false;
    }

    IEnumerator LedgeToClimb()
    {
        isHopping = true;
        // Eğer fizikSEL sıçrama oluyorsa aşağıdaki satırı tekrar açmalısın:
        // GetComponent<Collider>().enabled = false;

        animator.Play("Braced Hang To Crouch");
        yield return new WaitForSeconds(1.2f); // Bekleme süresini animasyona göre ayarla

        // GetComponent<Collider>().enabled = true;

        isHopping = false;
        isClimbing = false;
        
        // Rigidbody hızını sıfırla ki fizik devreye girince aniden fırlamasın
        if (_rb != null)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        playerState = PlayerState.NormalState;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying && h == 0 && v == 0) h = 0;

        Vector3 baseOrigin = transform.position + sphereOffset;
        Vector3 p1 = baseOrigin;
        Vector3 p2 = baseOrigin;

        if (isClimbing)
        {
            Vector3 inputDir = transform.right * h + transform.up * v;
            if (inputDir.sqrMagnitude > 0.01f)
            {
                if (inputDir.sqrMagnitude > 1f) inputDir.Normalize();
                p2 = baseOrigin + inputDir * capsuleLength;
            }
            else
            {
                p2 = baseOrigin + transform.up * capsuleLength;
            }
        }
        else
        {
            p2 = baseOrigin + transform.up * capsuleLength;
        }

        Gizmos.color = canGrabLedge ? Color.green : Color.red;

        Gizmos.DrawWireSphere(p1, sphereRadius);
        Gizmos.DrawWireSphere(p2, sphereRadius);

        Vector3 spine = p2 - p1;
        if (spine.sqrMagnitude > 0.001f)
        {
            Vector3 sideDir = Vector3.Cross(spine, transform.forward).normalized;
            if (sideDir.sqrMagnitude < 0.1f) sideDir = transform.right;
            Vector3 forwardDir = Vector3.Cross(spine, sideDir).normalized;

            sideDir *= sphereRadius;
            forwardDir *= sphereRadius;

            Gizmos.DrawLine(p1 + sideDir, p2 + sideDir);
            Gizmos.DrawLine(p1 - sideDir, p2 - sideDir);
            Gizmos.DrawLine(p1 + forwardDir, p2 + forwardDir);
            Gizmos.DrawLine(p1 - forwardDir, p2 - forwardDir);
        }

        if (canGrabLedge)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(climbTarget, 0.05f);            
        }
        // Chest SphereCast Gizmo
        Gizmos.color = Color.magenta;
        Vector3 chestOrigin = transform.TransformPoint(chestDetectionOffset);
        Gizmos.DrawWireSphere(chestOrigin, chestSphereRadius);
        Gizmos.DrawLine(chestOrigin, chestOrigin + transform.forward * chestMaxDistance);
            
        
    }
}

