using StarterAssets;
using UnityEngine;


public class BoxPullController : MonoBehaviour
{
    [Header("Detection")]
    [Tooltip("BoxPullable objelerin bulunduğu layer")]
    public LayerMask pullLayer;
    [Tooltip("Maksimum kanca menzili")]
    public float maxDistance = 20f;
    [Tooltip("SphereCast yarıçapı")]
    public float detectRadius = 1.2f;

    [Header("Pull Settings")]
    [Tooltip("Q basılı tutulunca kutunun çekilme kuvveti")]
    public float pullForce = 15f;
    [Tooltip("Kutu bu mesafeye gelince otomatik bırakılır")]
    public float stopDistance = 2f;

    [Header("References")]
    [Tooltip("İpin çıkış noktası (silah ucu / el) — BoxGrapplingRope da bu değeri okur")]
    public Transform gunTip;

    [Header("UI")]
    [Tooltip("'E – Kancala' ipucu UI objesi")]
    public GameObject hintHook;
    [Tooltip("'Q – Çek' ipucu UI objesi (kanca takılıyken gösterilir)")]
    public GameObject hintPull;

    // ── State ──────────────────────────────────────────────────────────────
    private BoxPullable _target;
    private BoxPullable _inSight;
    [HideInInspector] public bool isHooked;
    protected RaycastHit _pullHit;
    private ThirdPersonMovementController _movement;

    /// <summary>BoxGrapplingRope'un ip bitiş noktasını sorduğu metot. Her zaman kutunun merkezini döndürür.</summary>
    public Vector3 GetHookPoint() => _target != null ? _target.transform.position : Vector3.zero;

    // ───────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        _movement = GetComponent<ThirdPersonMovementController>();
        if (hintHook != null) hintHook.SetActive(false);
        if (hintPull != null) hintPull.SetActive(false);
    }

    private void Update()
    {
        if (isHooked)
            HandleHookedState();
        else
            HandleScanState();
    }

    
    private void HandleScanState()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f));

        _inSight = null;
        if (Physics.SphereCast(ray, detectRadius, out _pullHit, maxDistance, pullLayer))
            _pullHit.collider.TryGetComponent(out _inSight);

        if (hintHook != null) hintHook.SetActive(_inSight != null);

        if (_inSight != null && Input.GetKeyDown(KeyCode.E))
            AttachHook(_inSight);
    }

    private void HandleHookedState()
    {
        if (_target == null)
        {
            ReleaseHook();
            return;
        }


        float dist = Vector3.Distance(transform.position, _target.transform.position);

        // Kutu çok yaklaştıysa otomatik bırak
        if (dist <= stopDistance)
        {
            ReleaseHook();
            return;
        }

        // Menzil aşıldıysa bırak
        if (dist > maxDistance + 2f)
        {
            ReleaseHook();
            return;
        }

        // E → serbest bırak
        if (Input.GetKeyDown(KeyCode.E))
        {
            ReleaseHook();
            return;
        }


        // Q basılı → kutuyu çek
        if (Input.GetKey(KeyCode.Q))
        {
            Vector3 pullDir = (transform.position - _target.transform.position).normalized;
            pullDir.y = 0f;
            _target.rb.AddForce(pullDir * pullForce, ForceMode.Force);
        }
    }

    // ── Kanca Bağla ────────────────────────────────────────────────────────
    private void AttachHook(BoxPullable box)
    {
        _target  = box;
        isHooked = true;
        _target.OnGrabbed();

        // Karakteri kutuya döndür (tek seferlik)
        Vector3 dir = _target.transform.position - transform.position;
        dir.y = 0f;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);

        // TPS strafe moduna geç
        if (_movement != null) _movement.isStrafeMode = true;

        if (hintHook != null) hintHook.SetActive(false);
        if (hintPull != null) hintPull.SetActive(true);
    }

    // ── Kanca Bırak ────────────────────────────────────────────────────────
    private void ReleaseHook()
    {
        isHooked = false;

        if (_target != null)
        {
            _target.OnReleased();
            _target = null;
        }

        // Normal hareket moduna dön
        if (_movement != null) _movement.isStrafeMode = false;

        if (hintHook != null) hintHook.SetActive(false);
        if (hintPull != null) hintPull.SetActive(false);
    }

    // ── Gizmo ──────────────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Gizmos.color = isHooked ? Color.green : Color.cyan;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        Gizmos.DrawLine(ray.origin, ray.origin + ray.direction * maxDistance);
        Gizmos.DrawWireSphere(ray.origin + ray.direction * maxDistance, detectRadius);
    }
}
