using System;
using UnityEngine;

public class ShimmyController : MonoBehaviour
{
    private PlayerClimb playerClimbScript;

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

    private bool leftBtn;
    private bool rightBtn;
    private float horizontalValue;

    LedgeToRoofClimb ledgeToRoofClimb;

    private void Start()
    {
        playerClimbScript = GetComponent<PlayerClimb>();
        ledgeToRoofClimb = GetComponent<LedgeToRoofClimb>();
    }

    private void Update()
    {
        // 🔹 Eğer oyuncu tırmanmıyorsa veya hopluyorsa, hiç işlem yapma
        if (!playerClimbScript.isClimbing || IsHopping())
        {
            playerClimbScript.animator.SetFloat("Speed", 0f, 0.05f, Time.deltaTime);
            return;
        }

        // Karakterin önündeki ve yukarıdaki nokta
        center = transform.position + transform.forward * forwardPos + Vector3.up * upPos;

        // Kenar kontrolü
        hits = Physics.OverlapSphere(center, radius, playerClimbScript.ledgeLayer | playerClimbScript.originLedgeLayer);

        if (hits.Length > 0)
        {
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
            playerClimbScript.animator.SetFloat("Speed", 0f, 0.05f, Time.deltaTime);
        }
    }

    private void CheckSphere()
    {
        // 🔹 Hop sırasında hareket etme
        if (IsHopping())
        {
            playerClimbScript.animator.SetFloat("Speed", 0f, 0.05f, Time.deltaTime);
            return;
        }

        // Sağ tarafı kontrol et
        if (Physics.CheckSphere(climbPoint + transform.right * sphereGap, sphereRadius, playerClimbScript.ledgeLayer |  playerClimbScript.originLedgeLayer))
        {
            canMoveRight = true;
            rightBtn = Input.GetKey(KeyCode.D);
        }
        else
        {
            canMoveRight = false;
            rightBtn = false;
        }

        // Sol tarafı kontrol et
        if (Physics.CheckSphere(climbPoint - transform.right * sphereGap, sphereRadius, playerClimbScript.ledgeLayer|  playerClimbScript.originLedgeLayer))
        {
            canMoveLeft = true;
            leftBtn = Input.GetKey(KeyCode.A);
        }
        else
        {
            canMoveLeft = false;
            leftBtn = false;
        }

        // Yatay hareket yönü belirle
        if (leftBtn && canMoveLeft)
            horizontalValue = -1;
        else if (rightBtn && canMoveRight)
            horizontalValue = 1;
        else
            horizontalValue = 0;

        // Animator ve pozisyon
        playerClimbScript.animator.SetFloat("Speed", horizontalValue, 0.05f, Time.deltaTime);
        transform.position += transform.right * horizontalValue * ledgeMoveSpeed * Time.deltaTime;
    }

    // 🔹 PlayerClimb içindeki "isHopping" kontrolü
    private bool IsHopping()
    {
        // Eğer PlayerClimb'te isHopping public değilse burayı public yap
        var field = playerClimbScript.GetType().GetField("isHopping",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            return (bool)field.GetValue(playerClimbScript);
        }

        return false;
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