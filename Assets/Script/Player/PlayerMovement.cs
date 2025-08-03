using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    
    public Transform orientation;
    
    public Animator anm;

    public GameObject playerObj;
    
    public float walkSpeed = 3f;

    public float runSpeed = 6f;

    private float moveSpeed;

    private Vector3 moveDirection;
    
    public float gravity = -9.81f;

    public bool isGrounded;

    public Transform groundCheck;

    public float groundCheckDistance = 0.4f;

    public LayerMask groundMask;

    public float rotationSpeed = 10f;
    
    public float jumpHeight = 5f;
    
    public float groundDrag = 4f;

    public float airDrag = 0f;

    public Rigidbody rb;

    public static PlayerMovement Instance;

    public Grappling gp;

    public bool freeze;

    public bool isWalking;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();        
        rb.freezeRotation = true;
        
        gp= GetComponent<Grappling>();
    }
    

    private void FixedUpdate()
    {        
        Move();        
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
        anm.SetBool("IsGrounded", isGrounded && !freeze);
        anm.SetBool("IsRunning", isRunning && !freeze);
        anm.SetBool("IsJumping", !isGrounded && !freeze);
        anm.SetBool("IsWalking", isWalking && !freeze);
    }

    private void ReadInput()
    {
        if (freeze && isGrounded) return;
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        isWalking = horizontalInput != 0 || verticalInput != 0;
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        moveDirection.y = 0f;
        moveDirection.Normalize();
        // Animation: Speed for Idle/Run
        float moveAmount = new Vector2(horizontalInput, verticalInput).magnitude;
        //anm.SetFloat("Speed", moveAmount);
    }

    private void Move()
    {
        if(freeze && isGrounded) return;
        Vector3 targetVelocity = moveDirection.normalized * moveSpeed;
        Vector3 movement = targetVelocity * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);
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

        Vector3 lookDir = new Vector3(moveDirection.x, 0, moveDirection.z).normalized;
        playerObj.transform.forward = Vector3.Slerp(playerObj.transform.forward, lookDir, Time.deltaTime * rotationSpeed);
       
    }


    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {        
        Vector3 offset = new Vector3(0, 2f, 0);
        Vector3 velocityToSet = CalculateJumpVelocity(transform.position, targetPosition + offset, trajectoryHeight);
        rb.linearVelocity = velocityToSet;
        //Vector3 dir= CalculateJumpVelocity(transform.position, targetPosition + offset, trajectoryHeight);
        //rb.AddForce(dir, ForceMode.Impulse);
        gp.headAimConstraint.weight = 0f;
        gp.bodyAimConstraint.weight = 0f;
        gp.grappling = false;
        anm.SetBool("Fire", false);
        freeze = false;
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
