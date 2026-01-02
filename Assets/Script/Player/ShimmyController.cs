using StarterAssets;
using System;
using UnityEngine;

public class ShimmyController : MonoBehaviour
{
    private PlayerClimb playerClimbScript;
    private ThirdPersonInputSystem _inputSystem;

    [Header("Detection Settings")]
    public float shimmySphereGap = 0.4f;
    public float shimmyUpPos = 1.6f;
    public float shimmyForwardPos = 1.0f;
    public float shimmyRadius = 0.5f;
    public float shimmyLedgeRadius = 0.1f;
    private Vector3 center;

    [Header("Movement Settings")] public float shimmySpeed = 2.0f;

    public bool canMoveRight;
    public bool canMoveLeft;
    public bool canMove;

    [HideInInspector] public Vector3 climbPoint;
    private Collider ledge;
    private Collider[] hits;
    public bool isCrouchLedge;
    public string crouchLedge;

    private void Awake()
    {
        _inputSystem = new ThirdPersonInputSystem();
    }

    private void OnEnable() => _inputSystem.Enable();
    private void OnDisable() => _inputSystem.Disable();

    private void Start()
    {
        playerClimbScript = GetComponent<PlayerClimb>();
    }

    private void Update()
    {
        if (!playerClimbScript.isClimbing || playerClimbScript.isHopping)
        {
            playerClimbScript.animator.SetFloat("Shimmy", 0f, 0.05f, Time.deltaTime);
            return;
        }

       
        center = transform.position + transform.forward * shimmyForwardPos + Vector3.up * shimmyUpPos;

        
        hits = Physics.OverlapSphere(center, shimmyRadius, playerClimbScript.ledgeLayer);


        

        if (hits.Length > 0)
        {
            canMove = true;
            ledge = hits[0];
            climbPoint = ledge.ClosestPoint(transform.position);
            isCrouchLedge = hits[0].transform.CompareTag(crouchLedge);
        }
        else
        {
            canMove = false;
            climbPoint = Vector3.zero;
            isCrouchLedge= false;
        }

        
        if (canMove) CheckMoveSphere();
        else playerClimbScript.animator.SetFloat("Shimmy", 0f, 0.05f, Time.deltaTime);


    }

    private void CheckMoveSphere()
    {
               
        canMoveRight = Physics.CheckSphere(climbPoint + transform.right * shimmySphereGap, shimmyLedgeRadius, playerClimbScript.ledgeLayer);
        canMoveLeft=Physics.CheckSphere(climbPoint - transform.right * shimmySphereGap, shimmyLedgeRadius, playerClimbScript.ledgeLayer);
        Move();
    }

    void Move()
    {
        Vector2 moveInput = _inputSystem.Player.Move.ReadValue<Vector2>();
        float h = moveInput.x;
        if (!canMoveLeft && h < -0.1f) h = 0f;
        if (!canMoveRight && h > 0.1f) h = 0f;
        playerClimbScript.animator.SetFloat("Shimmy", h, 0.05f, Time.deltaTime);
        if (Mathf.Abs(h) > 0.1f) transform.Translate(Vector3.right * h * shimmySpeed * Time.deltaTime);
    }

    

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(center, shimmyRadius);

        if (canMove)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(climbPoint + transform.right * shimmySphereGap, shimmyLedgeRadius);
            Gizmos.DrawSphere(climbPoint - transform.right * shimmySphereGap, shimmyLedgeRadius);
        }
        
    }
}