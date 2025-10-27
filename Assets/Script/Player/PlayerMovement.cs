using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    
    public Transform groundCheck; 
    [HideInInspector] public Animator anm;
    [HideInInspector] public Rigidbody rb;
   

    [Header("Movement Settings")] 
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float pushSpeed = 2f;
    public float rotationSpeed = 10f;
    public float climbSpeed = 1f;

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

    
    PlayerClimb playerClimb;
    
    HookManager hookManager;


    private void Awake() => Instance = this;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        hookManager = GetComponent<HookManager>();
        playerClimb = GetComponent<PlayerClimb>();
        rb.freezeRotation = true;
        anm = GetComponent<Animator>();
        
    }

    void Update()
    {
        if(playerClimb.isClimbing) return;
        inputHorizontal = Input.GetAxis("Horizontal");
        inputVertical = Input.GetAxis("Vertical");
        lastRotation = transform.rotation;
        LadderClimb();
        UpdateAnimator();
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            Jump();
        
    }

    private void FixedUpdate()
    {
        if (isInteractingWithNPC || isLadderClimbing || isClimbingEnd || hookManager.isSwinging || playerClimb.isClimbing) return;

        isRunning = Input.GetKey(KeyCode.LeftShift) && isGrounded;
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
    
    Quaternion lastRotation;
    private void UpdatePlayerRotation()
    {
        if (isObjectPushing || hookManager.isGrappling) return;
        
        Vector3 moveDir = new Vector3(inputHorizontal, 0, inputVertical);

        if (moveDir.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
            Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            
        }
        else
        {
            
            transform.rotation = Quaternion.Slerp(transform.rotation, lastRotation, rotationSpeed * Time.deltaTime);
        }
    }


    private void UpdateAnimator()
    {
        anm.SetBool("IsGrounded", isGrounded && !freeze);
        anm.SetBool("IsRunning", isRunning && !freeze && !isObjectPushing);
        anm.SetBool("IsJumping", !isGrounded && !freeze && !isClimbingEnd && !playerClimb.isClimbing);
        anm.SetBool("IsWalking", isWalking && !freeze && !isObjectPushing);
        anm.SetBool("IsPush", isObjectPushing);
        anm.SetFloat("DirX", inputHorizontal);
        anm.SetFloat("DirY", inputVertical);
    }

    

    


    public void LadderClimb()
    {
        if (isLadderClimbing && isGrounded)
        {
            anm.applyRootMotion = true;

            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero; // Fizik hızını sıfırla

            // Yukarı doğru local eksende hareket et
            transform.Translate(Vector3.up * climbSpeed * Time.deltaTime, Space.World);
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

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("LadderStart") && !isLadderClimbing)
        {
            Ladder ladder = other.transform.parent.GetComponent<Ladder>();
            ladder.isPlayer = false;
            
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