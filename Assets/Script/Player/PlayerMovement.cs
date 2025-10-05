using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")] public Transform cam; // Main Camera referansı
    public Transform groundCheck; // Karakterin ayak hizasına boş obje koy
    [HideInInspector] public Animator anm;
    [HideInInspector] public Rigidbody rb;
    public Grappling gp; // Grappling scripti (opsiyonel)

    [Header("Movement Settings")] public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float pushSpeed = 2f;
    public float rotationSpeed = 10f;

     public float jumpForce = 5f;
    public float groundCheckDistance = 0.4f;
    public LayerMask groundMask;

    [Header("States")] public bool freeze;
    public bool isWalking;
    public bool isRunning;
    public bool isGrounded;
    public bool isObjectPushing;
    public bool isInteractingWithNPC;

    [Header("IK")] public TwoBoneIKConstraint rightHand, leftHand;


    private float moveSpeed;
    private float gravity = -9.81f;

    float inputHorizontal;
    float inputVertical;
    private float directionX;
    private float directionZ;
    private Vector3 forward;
    private Vector3 right;
    private Vector3 horizontalDirection;
    public static PlayerMovement Instance;
    public bool isLadderClimbing;

    RopeSwing swing;


    private void Awake() => Instance = this;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        swing = GetComponent<RopeSwing>();
        anm = GetComponent<Animator>();
        gp = GetComponent<Grappling>(); // Grappling varsa alır
    }

    void Update()
    {
        inputHorizontal = Input.GetAxis("Horizontal");
        inputVertical = Input.GetAxis("Vertical");
        LadderClimb();
        UpdateAnimator();
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            Jump();
        
    }

    private void FixedUpdate()
    {
        if (isInteractingWithNPC || isLadderClimbing || isClimbingEnd || swing.isSwing) return;

        isRunning = Input.GetKey(KeyCode.LeftShift);
        moveSpeed = isObjectPushing ? pushSpeed : (isRunning ? runSpeed : walkSpeed);
        

        Move();
        HandleGroundCheck();
        UpdatePlayerRotation();
        
    }


    private void Move()
    {
        if (freeze && isGrounded) return;

        // Eğer obje itiliyorsa yalnızca ileri/geri hareket et
        if (isObjectPushing)
        {
            // Oyuncunun kendi yönüne göre hareket etsin (kutunun yönüyle aynı hizadadır)
            Vector3 moveDir =
                (transform.forward * inputVertical + transform.right * inputHorizontal) * moveSpeed * Time.deltaTime;

            isWalking = moveDir.magnitude > 0;
            transform.Translate(moveDir, Space.World);
            return;
        }

        // Normal hareket (kameraya göre)
        directionX = inputHorizontal * moveSpeed * Time.deltaTime;
        directionZ = inputVertical * moveSpeed * Time.deltaTime;
        float directionY = 0;

        forward = Camera.main.transform.forward;
        right = Camera.main.transform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        forward = forward * directionZ;
        right = right * directionX;

        Vector3 verticalDirection = Vector3.up * directionY;
        horizontalDirection = forward + right;

        Vector3 movementNormal = verticalDirection + horizontalDirection;
        isWalking = movementNormal.magnitude > 0;

        transform.Translate(movementNormal, Space.World);
    }



    private void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void HandleGroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckDistance, groundMask);
    }

    private void UpdatePlayerRotation()
    {
        if (isObjectPushing) return;
        
        float angle = Mathf.Atan2(forward.x + right.x, forward.z + right.z) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, angle, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.15f);
        
    }

    private void UpdateAnimator()
    {
        anm.SetBool("IsGrounded", isGrounded && !freeze);
        anm.SetBool("IsRunning", isRunning && !freeze && !isObjectPushing);
        anm.SetBool("IsJumping", !isGrounded && !freeze && !isClimbingEnd);
        anm.SetBool("IsWalking", isWalking && !freeze && !isObjectPushing);
        anm.SetBool("IsPush", isObjectPushing);
        anm.SetFloat("DirX", inputHorizontal);
        anm.SetFloat("DirY", inputVertical);
    }

    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        Vector3 offset = new Vector3(0, 2f, 0); // Biraz yukarıya nişanla
        Vector3 velocityToSet = CalculateJumpVelocity(transform.position, targetPosition + offset, trajectoryHeight);
        rb.linearVelocity = velocityToSet;

        if (gp != null)
        {
            gp.headAimConstraint.weight = 0f;
            gp.grappling = false;
        }

        anm.SetBool("Fire", false);
        freeze = false;
    }

    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float g = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * g * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / g)
                                               + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / g));

        return velocityXZ + velocityY;
    }


    public void LadderClimb()
    {
        if (isLadderClimbing && isGrounded)
        {
            anm.applyRootMotion = true;

            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero; // Fizik hızını sıfırla

            // Yukarı doğru local eksende hareket et
            transform.Translate(Vector3.up * moveSpeed * Time.deltaTime, Space.World);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("LadderEnd") && isLadderClimbing)
        {
            StartCoroutine(EndLadderClimb());
            Ladder ladder = other.transform.parent.GetComponent<Ladder>();
            ladder.isPlayer = false;
        }
        if (other.gameObject.CompareTag("LadderStart") && !isLadderClimbing)
        {
            Ladder ladder = other.transform.parent.GetComponent<Ladder>();
            ladder.isPlayer = true;
            ladder.player = gameObject;
        }
    }

    public bool isClimbingEnd;

    IEnumerator EndLadderClimb()
    {
        anm.SetTrigger("EndClimb");
        GetComponent<BoxCollider>().isTrigger = true;
        isLadderClimbing = false;
        rb.useGravity = false;
        isClimbingEnd = true;
        yield return new WaitForSecondsRealtime(4f);
        rb.useGravity = true;
        isClimbingEnd = false;
        anm.applyRootMotion = false;
        GetComponent<BoxCollider>().isTrigger = false;
    }
}