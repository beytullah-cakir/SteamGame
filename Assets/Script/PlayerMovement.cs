using Unity.Cinemachine;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform player;
    public Transform playerObj;
    public CharacterController controller;
    public CinemachineCamera combatCam;
    public CinemachineCamera thirdPersonCam;

    [Header("Movement")]
    public float moveSpeed = 5f;
    private Vector3 moveDirection;

    [Header("Gravity")]
    public float gravity = -9.81f;
    private float verticalVelocity;
    public Transform groundCheck;
    public float groundCheckDistance = 0.4f;
    public LayerMask groundMask;
    private bool isGrounded;

    public float rotationSpeed = 10f;

    [Header("Aiming")]
    public float aimMoveSpeed = 2f;

    public enum CameraStyle { Basic, Combat }
    public CameraStyle currentStyle;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SwitchCameraStyle(CameraStyle.Basic);
    }

    private void Update()
    {
        HandleGroundCheck();
        UpdateOrientation();
        ReadInput();
        ApplyGravity();
        Move();
        UpdatePlayerRotation();

        // Sað týk ile kamera stilini deðiþtir
        if (Input.GetMouseButton(1))
        {
            if (currentStyle != CameraStyle.Combat)
                SwitchCameraStyle(CameraStyle.Combat);
        }
        else
        {
            if (currentStyle != CameraStyle.Basic)
                SwitchCameraStyle(CameraStyle.Basic);
        }
    }

    private void HandleGroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckDistance, groundMask);
        if (isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;
    }

    private void UpdateOrientation()
    {
        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0f;

        if (cameraForward != Vector3.zero)
            orientation.forward = cameraForward.normalized;
    }

    private void ReadInput()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
    }

    private void ApplyGravity()
    {
        verticalVelocity += gravity * Time.deltaTime;
    }

    private void Move()
    {
        float currentSpeed = currentStyle == CameraStyle.Combat ? aimMoveSpeed : moveSpeed;
        Vector3 velocity = moveDirection.normalized * currentSpeed + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }

    private void UpdatePlayerRotation()
    {
        if (currentStyle == CameraStyle.Combat)
        {
            // FPS modunda karakter yönü kameraya bakar
            Vector3 lookDir = Camera.main.transform.forward;
            lookDir.y = 0f;
            if (lookDir != Vector3.zero)
                playerObj.forward = Vector3.Slerp(playerObj.forward, lookDir.normalized, Time.deltaTime * rotationSpeed);
        }
        else
        {
            // Normal modda hareket yönüne bakar
            if (moveDirection != Vector3.zero)
            {
                Vector3 lookDir = moveDirection.normalized;
                playerObj.forward = Vector3.Slerp(playerObj.forward, lookDir, Time.deltaTime * rotationSpeed);
            }
        }
    }

    private void SwitchCameraStyle(CameraStyle newStyle)
    {
        if (currentStyle == newStyle)
            return;

        combatCam.Priority = 0;
        thirdPersonCam.Priority = 0;

        if (newStyle == CameraStyle.Basic) thirdPersonCam.Priority = 10;
        if (newStyle == CameraStyle.Combat) combatCam.Priority = 10;

        currentStyle = newStyle;
    }
}
