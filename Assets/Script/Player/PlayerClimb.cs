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
    private Rigidbody _rb;
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
    public float capsuleLength = 0.7f;

    [Header("Shimmy Settings")]
    public float shimmySphereGap = 0.4f;
    public float shimmyUpPos = 1.6f;
    public float shimmyForwardPos = 1.0f;
    public float shimmyRadius = 0.5f;
    public float shimmyLedgeRadius = 0.1f;
    public float shimmyLedgeMoveSpeed = 0.5f;

    private Vector3 climbTarget;
    private Vector3 currNormal;
    private Quaternion targetRot;
    private Collider _currentLedge;
    private Collider _detectedLedge;
    private float h, v;
    public bool isHopping;
    private bool isIdleLedgeBusy;

    private LedgeToRoofClimb ledgeToRoofClimb;
    private Vector3 shimmyCenter;
    private bool canMoveRight, canMoveLeft, canMove;
    private Vector3 shimmyPoint;

    private void Start()
    {
        _playerController = GetComponent<ThirdPersonController>();
        _rb = GetComponent<Rigidbody>();
        _input = GetComponent<StarterAssetsInputs>();
        roofLedgeDetection = GetComponent<RoofLedgeDetection>();
        ledgeToRoofClimb = GetComponent<LedgeToRoofClimb>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (isZipped || animator == null) return;

        StateConditionsCheck();

        Inputs();

        CheckingMain();

        if (isClimbing && !isHopping)
        {
            ShimmyLogic();
        }
        else if (animator != null)
        {
            animator.SetFloat("Shimmy", 0f, 0.05f, Time.deltaTime);
        }

        MatchTargetToLedge();

        if (interactUI != null)
            interactUI.SetActive(canGrabLedge && !isClimbing);
    }

    private void CheckingMain()
    {
        if (isIdleLedgeBusy) return;

        // Kapsülün sabit başlangıç noktası (Merkez)
        Vector3 baseOrigin = transform.position + Vector3.up * sphereOffset;

        // point1 SABİT kalıyor
        Vector3 point1 = baseOrigin;
        Vector3 point2 = baseOrigin;

        if (isClimbing)
        {
            // Tırmanıyorken: 8 yönlü inputa göre uzanır
            Vector3 inputDir = transform.right * h + transform.up * v;
            if (inputDir.sqrMagnitude > 0.01f)
            {
                if (inputDir.sqrMagnitude > 1f) inputDir.Normalize();
                point2 = baseOrigin + inputDir * capsuleLength;
            }
            else
            {
                // Tırmanırken duruyorsa (input yoksa) sabit dikey kalsın
                point2 = baseOrigin + transform.up * capsuleLength;
            }
        }
        else
        {
            // Tırmanmıyorken (Ledge ararken): Sabit Dikey yapı
            point2 = baseOrigin + transform.up * capsuleLength;
        }

        Vector3 direction = transform.forward;

        RaycastHit[] hits = Physics.CapsuleCastAll(point1, point2, sphereRadius, direction, maxDistance, ledgeLayer);
        bool foundValidLedge = false;
        RaycastHit bestHit = new RaycastHit();

        // Mesafeye göre sırala ve şu an tutulan kenar dışındakini seç
        var sortedHits = hits.OrderBy(h => h.distance);

        foreach (var hitInfo in sortedHits)
        {
            if (isClimbing && _currentLedge != null && hitInfo.collider == _currentLedge)
                continue;

            bestHit = hitInfo;
            foundValidLedge = true;
            break;
        }

        if (foundValidLedge)
        {
            _detectedLedge = bestHit.collider;
            canGrabLedge = true;

            climbTarget = bestHit.point;
            currNormal = bestHit.normal;
            targetRot = Quaternion.LookRotation(-bestHit.normal);

            if (interactUI != null && !isClimbing)
            {
                Vector3 uiPos = bestHit.point + Vector3.up * 0.5f;
                interactUI.transform.position = Camera.main.WorldToScreenPoint(uiPos);
            }
        }
        else
        {
            _detectedLedge = null;
            canGrabLedge = false;
        }

        Debug.DrawLine(baseOrigin, baseOrigin + (direction * maxDistance), canGrabLedge ? Color.green : Color.black);
    }

    private void ShimmyLogic()
    {
        // Karakterin önündeki ve yukarıdaki nokta (Shimmy tespiti için)
        shimmyCenter = transform.position + transform.forward * shimmyForwardPos + Vector3.up * shimmyUpPos;

        // Kenar kontrolü
        Collider[] hits = Physics.OverlapSphere(shimmyCenter, shimmyRadius, ledgeLayer);

        if (hits.Length > 0)
        {
            if (ledgeToRoofClimb != null)
                ledgeToRoofClimb.foundLedgeToRoofClimb = hits[0].CompareTag("RoofLedge");

            canMove = true;
            shimmyPoint = hits[0].ClosestPoint(transform.position);
        }
        else
        {
            canMove = false;
            canMoveLeft = false;
            canMoveRight = false;
            shimmyPoint = Vector3.zero;
        }

        if (canMove)
        {
            // Sağ tarafı kontrol et
            canMoveRight = Physics.CheckSphere(shimmyPoint + transform.right * shimmySphereGap, sphereRadius, ledgeLayer);
            if (!canMoveRight && h > 0.1f) h = 0;

            // Sol tarafı kontrol et
            canMoveLeft = Physics.CheckSphere(shimmyPoint - transform.right * shimmySphereGap, sphereRadius, ledgeLayer);
            if (!canMoveLeft && h < -0.1f) h = 0;

            // Animator ve pozisyon güncelleme
            animator.SetFloat("Shimmy", h, 0.05f, Time.deltaTime);

            if (Mathf.Abs(h) > 0.1f)
            {
                transform.position += transform.right * h * shimmyLedgeMoveSpeed * Time.deltaTime;
            }
        }
        else
        {
            animator.SetFloat("Shimmy", 0f, 0.05f, Time.deltaTime);
        }
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

        // Tırmanışa giriş veya tırmanırken zıplama (Space / Jump)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Girişi hemen tüket
            if (_input != null) _input.jump = false;

            if (!isClimbing && canGrabLedge)
            {
                Vector3 lookDir = (climbTarget - transform.position).normalized;
                lookDir.y = 0;
                if (lookDir != Vector3.zero)
                    transform.rotation = Quaternion.LookRotation(lookDir);

                StartCoroutine(GrabLedge());
            }
            else if (isClimbing && canGrabLedge)
            {
                // Input yönüne göre uygun zıplama animasyonunu tetikle
                if (v > 0.3f) StartCoroutine(HopUp());
                else if (v < -0.3f) StartCoroutine(HopDown());
                else if (h > 0.3f) StartCoroutine(HopRight());
                else if (h < -0.3f) StartCoroutine(HopLeft());
                else StartCoroutine(HopUp()); // Varsayılan yukarı zıpla
            }
        }

        // Bırakma kontrolü (C tuşu veya Aşağı + Space)
        if (Input.GetKeyDown(KeyCode.C) && isClimbing)
            StartCoroutine(DropLedge());
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
    private bool IsInState(AnimatorStateInfo stateInfo, string stateName)
    {
        return stateInfo.IsName(stateName) && !animator.IsInTransition(0);
    }

    private void ApplyMatchTarget(AvatarTarget target, Vector3 maskWeights, float start, float end)
    {
        // Safety check
        if (animator.isMatchingTarget) return;

        animator.MatchTarget(
            climbTarget,
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
            ApplyMatchTarget(AvatarTarget.RightHand, new Vector3(0, 1, 1), 0.36f, 0.57f);

        if (IsInState(stateInfo, "Hanging Idle"))
            ApplyMatchTarget(AvatarTarget.RightHand, new Vector3(0, 1, 1), 0.1f, 0.9f);

        if (IsInState(stateInfo, "Braced Hang Hop Up"))
            ApplyMatchTarget(AvatarTarget.RightHand, new Vector3(1, 1, 1), 0.1f, 0.9f);

        if (IsInState(stateInfo, "Braced Hang Hop Right"))
            ApplyMatchTarget(AvatarTarget.LeftHand, new Vector3(1, 1, 1), 0.1f, 0.9f);

        if (IsInState(stateInfo, "Braced Hang Hop Left"))
            ApplyMatchTarget(AvatarTarget.RightHand, new Vector3(1, 1, 0), 0.1f, 0.9f);

        if (IsInState(stateInfo, "Braced Hang Hop Down"))
            ApplyMatchTarget(AvatarTarget.RightHand, new Vector3(0, 1, 1), 0.1f, 0.9f);

    }

    IEnumerator GrabLedge()
    {
        playerState = PlayerState.ClimbingState;
        isHopping = true;
        isClimbing = true;

        // Şu an tutunduğumuz collider'ı kaydet
        _currentLedge = _detectedLedge;

        transform.rotation = targetRot;

        animator.CrossFade("Idle To Braced Hang", 0.1f);

        yield return new WaitForSeconds(0.5f); // Reduced wait time for responsiveness

        isHopping = false;
    }

    public IEnumerator DropLedge()
    {
        animator.CrossFade("Braced To Drop", 0.1f);
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
        animator.CrossFade("Braced Hang Hop Up", 0.1f);
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        _currentLedge = _detectedLedge; // Atladığımız yeni kenarı aktif kenar yap
        isHopping = false;
    }

    IEnumerator HopRight()
    {
        isHopping = true;
        animator.CrossFade("Braced Hang Hop Right", 0.1f);
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        _currentLedge = _detectedLedge;
        isHopping = false;
    }

    IEnumerator HopLeft()
    {
        isHopping = true;
        animator.CrossFade("Braced Hang Hop Left", 0.1f);
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        _currentLedge = _detectedLedge;
        isHopping = false;
    }

    IEnumerator HopDown()
    {
        isHopping = true;
        animator.CrossFade("Braced Hang Hop Down", 0.2f);
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        _currentLedge = _detectedLedge;
        isHopping = false;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying && h == 0 && v == 0) h = 0;

        Vector3 baseOrigin = transform.position + Vector3.up * sphereOffset;
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

        Gizmos.DrawLine(baseOrigin, baseOrigin + transform.forward * maxDistance);

        if (canGrabLedge)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(climbTarget, 0.05f);
            Gizmos.DrawRay(climbTarget, currNormal * 0.4f);
        }

        // Shimmy Gizmos
        if (isClimbing)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(shimmyCenter, shimmyRadius);

            if (canMove)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(shimmyPoint + transform.right * shimmySphereGap, shimmyLedgeRadius);
                Gizmos.DrawSphere(shimmyPoint - transform.right * shimmySphereGap, shimmyLedgeRadius);
            }
        }
    }
}

