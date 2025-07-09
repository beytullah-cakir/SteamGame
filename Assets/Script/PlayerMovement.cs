using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Image crosshair;
    public Animator anm;
    public GameObject playerObj;

    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    private float moveSpeed;
    private Vector3 moveDirection;

    [Header("Gravity")]
    public float gravity = -9.81f;
    public bool isGrounded;
    public Transform groundCheck;
    public float groundCheckDistance = 0.4f;
    public LayerMask groundMask;

    public float rotationSpeed = 10f;

    [Header("Jumping")]
    public float jumpHeight = 5f;

    [Header("Physics")]
    public float groundDrag = 4f;
    public float airDrag = 0f;

    private Rigidbody rb;
    public bool freeze = false;    
    private bool isAiming = false;
    private Vector3 velocityToSet;
    private Grappling grappling;


    public static PlayerMovement Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        grappling = GetComponent<Grappling>();
        print(transform.position);

    }

    private void Update()
    {
        HandleGroundCheck();
        

        ReadInput();
        UpdatePlayerRotation();

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        moveSpeed = isRunning ? runSpeed : walkSpeed;

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }
        isAiming = Input.GetMouseButton(1);
        anm.SetBool("IsGrounded", isGrounded);
        anm.SetBool("IsRunning", isRunning);


        anm.SetBool("IsJumping", !isGrounded);

    }




    private void FixedUpdate()
    {
        Move();
    }

    private void ReadInput()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Animation: Speed for Idle/Run
        float moveAmount = new Vector2(horizontalInput, verticalInput).magnitude;
        anm.SetFloat("Speed", moveAmount);
    }

    private void Move()
    {
        if (freeze) return;
        Vector3 targetVelocity = moveDirection.normalized * moveSpeed;
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float jumpVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        rb.AddForce(Vector3.up * jumpVelocity, ForceMode.VelocityChange);


    }


    private void HandleGroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckDistance, groundMask);
    }

    private void UpdatePlayerRotation()
    {
        if (freeze) return;

        if (isAiming)
        {
            Vector3 lookDirection = orientation.forward;
            lookDirection.y = 0f;
            if (lookDirection != Vector3.zero)
            {
                playerObj.transform.forward = Vector3.Slerp(playerObj.transform.forward, lookDirection.normalized, Time.deltaTime * rotationSpeed);
            }
        }
        else if (moveDirection != Vector3.zero)
        {
            Vector3 lookDir = new Vector3(moveDirection.x, 0, moveDirection.z).normalized;
            playerObj.transform.forward = Vector3.Slerp(playerObj.transform.forward, lookDir, Time.deltaTime * rotationSpeed);
        }
    }


    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {        
        Vector3 offset = new Vector3(0, 2f, 0);
        velocityToSet = CalculateJumpVelocity(transform.position, targetPosition + offset, trajectoryHeight);
        rb.linearVelocity = velocityToSet;
        
    }
    
    
    

    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity)
            + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }




}
