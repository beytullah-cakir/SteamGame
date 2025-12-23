using StarterAssets;
using System;
using UnityEngine;

public class ShimmyController : MonoBehaviour
{
    private PlayerClimb playerClimbScript;
    private StarterAssetsInputs _input;

    [Header("Detection Settings")] public float sphereRadius = 0.3f;
    public float sphereGap = 0.4f;
    public float upPos = 1.6f;
    public float forwardPos = 1.0f;
    public float radius = 0.5f;

    [Header("Movement Settings")] public float ledgeMoveSpeed = 0.5f;

    private bool canMoveRight;
    private bool canMoveLeft;
    public bool canMove;

    [HideInInspector] public Vector3 climbPoint;
    private Collider ledge;
    private Collider[] hits;
    private Vector3 center;

    private float horizontalValue;

    LedgeToRoofClimb ledgeToRoofClimb;

    private void Start()
    {
        playerClimbScript = GetComponent<PlayerClimb>();
        ledgeToRoofClimb = GetComponent<LedgeToRoofClimb>();
        _input = GetComponent<StarterAssetsInputs>();
    }

    private void Update()
    {
        // 🔹 Eğer oyuncu tırmanmıyorsa veya hopluyorsa, hiç işlem yapma
        if (!playerClimbScript.isClimbing || IsHopping())
        {
            playerClimbScript.animator.SetFloat("Shimmy", 0f, 0.05f, Time.deltaTime);
            return;
        }

        // Karakterin önündeki ve yukarıdaki nokta
        center = transform.position + transform.forward * forwardPos + Vector3.up * upPos;

        // Kenar kontrolü
        hits = Physics.OverlapSphere(center, radius, playerClimbScript.ledgeLayer);

        if (hits.Length > 0)
        {
            if (ledgeToRoofClimb != null)
                ledgeToRoofClimb.foundLedgeToRoofClimb = hits[0].CompareTag("RoofLedge");
            canMove = true;
            ledge = hits[0];
            climbPoint = ledge.ClosestPoint(transform.position);
        }
        else
        {
            // ❗ Hiçbir kenar yok → hareket kapalı
            canMove = false;
            canMoveLeft = false;
            canMoveRight = false;
            climbPoint = Vector3.zero;
        }

        // Eğer kenar var ise hareket kontrolünü yap
        if (canMove)
            CheckSphere();
        else
        {
            // Speed parametresini sıfırla
            playerClimbScript.animator.SetFloat("Shimmy", 0f, 0.05f, Time.deltaTime);
        }
    }

    private void CheckSphere()
    {
        // 🔹 Hop sırasında hareket etme
        if (IsHopping())
        {
            playerClimbScript.animator.SetFloat("Shimmy", 0f, 0.05f, Time.deltaTime);
            return;
        }

        // Get Input from StarterAssetsInputs
        float h = _input != null ? _input.move.x : Input.GetAxisRaw("Horizontal");

        // Sağ tarafı kontrol et
        if (Physics.CheckSphere(climbPoint + transform.right * sphereGap, sphereRadius, playerClimbScript.ledgeLayer ))
        {
            canMoveRight = true;
        }
        else
        {
            canMoveRight = false;
            if (h > 0) h = 0; // Sağa gidemiyorsak inputu kes
        }

        // Sol tarafı kontrol et
        if (Physics.CheckSphere(climbPoint - transform.right * sphereGap, sphereRadius, playerClimbScript.ledgeLayer))
        {
            canMoveLeft = true;
        }
        else
        {
            canMoveLeft = false;
            if (h < 0) h = 0; // Sola gidemiyorsak inputu kes
        }

        horizontalValue = h;

        // Animator ve pozisyon
        playerClimbScript.animator.SetFloat("Shimmy", horizontalValue, 0.05f, Time.deltaTime);
        
        if (Mathf.Abs(horizontalValue) > 0.1f)
        {
            transform.position += transform.right * horizontalValue * ledgeMoveSpeed * Time.deltaTime;
        }
    }

    // 🔹 PlayerClimb içindeki "isHopping" kontrolü
    private bool IsHopping()
    {
        return playerClimbScript.isHopping;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(center, radius);

        if (hits is not { Length: > 0 }) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(climbPoint + transform.right * sphereGap, sphereRadius);
        Gizmos.DrawSphere(climbPoint - transform.right * sphereGap, sphereRadius);
    }
}