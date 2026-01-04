using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(Rigidbody))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonMovementController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float MoveSpeed = 4.0f;
        public float SprintSpeed = 6.0f;
        public float RotationSmoothTime = 0.12f;
        public float SpeedChangeRate = 10.0f;

        [Header("Jump & Gravity")]
        public float JumpForce = 5.0f;
        public float Gravity = -15.0f;
        public float FallTimeout = 0.15f;
        public float GroundedRadius = 0.3f;
        public float GroundedOffset = 0.1f;
        public LayerMask GroundLayers;

        [Header("Cinemachine Camera")]
        public GameObject CinemachineCameraTarget;
        public float TopClamp = 70.0f;
        public float BottomClamp = -30.0f;
        public float CameraAngleOverride = 0.0f;
        public float MouseSensitivity = 1.0f;
        public bool LockCameraPosition = false;

        // Internal State
        private float _speed;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;
        public bool isGrounded;
        public bool freezeMovement;        

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

        // References
        private Rigidbody _rb;
        private Animator _animator;
        private ThirdPersonInputSystem _inputSystem;
        private PlayerInput _playerInput;
        private GameObject _mainCamera;
        private PlayerClimb _playerClimb;

        private const float _threshold = 0.01f;

        private void Awake()
        {
            if (_mainCamera == null) _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            _rb = GetComponent<Rigidbody>();
            _animator = GetComponent<Animator>();
            _inputSystem = new ThirdPersonInputSystem();
            _playerInput = GetComponent<PlayerInput>();
            _playerClimb = GetComponent<PlayerClimb>();

            AssignAnimationIDs();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void OnEnable() => _inputSystem.Enable();
        private void OnDisable() => _inputSystem.Disable();

        private void Start()
        {
            // Cursor Kilitleme
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            _rb.freezeRotation = true;
        }

        private void Update()
        {
            if (freezeMovement || HookManager.Instance.canGrapple) return;

            GroundedCheck();
            JumpAndGravity();
        }

        private void FixedUpdate()
        {
            if (freezeMovement || HookManager.Instance.canGrapple) return;
            Move();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            isGrounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

            if (_animator != null)
            {
                _animator.SetBool(_animIDGrounded, isGrounded);
            }
        }

        private void CameraRotation()
        {
            Vector2 lookInput = _inputSystem.Player.Look.ReadValue<Vector2>();
            
            if (lookInput.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                // Mouse input check
                bool isMouse = _playerInput.currentControlScheme == "KeyboardMouse";
                float deltaTimeMultiplier = isMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += lookInput.x * deltaTimeMultiplier * MouseSensitivity*Time.deltaTime;
                _cinemachineTargetPitch += lookInput.y * deltaTimeMultiplier * MouseSensitivity*Time.deltaTime;
            }

            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            Vector2 moveInput = _inputSystem.Player.Move.ReadValue<Vector2>();
            bool sprintInput = _inputSystem.Player.Sprint.IsPressed();

            float targetSpeed = sprintInput ? SprintSpeed : MoveSpeed;
            if (moveInput == Vector2.zero) targetSpeed = 0.0f;

            float currentHorizontalSpeed = new Vector3(_rb.linearVelocity.x, 0.0f, _rb.linearVelocity.z).magnitude;
            float speedOffset = 0.1f;
            float inputMagnitude = moveInput.magnitude;

            // Simple acceleration/deceleration
            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.fixedDeltaTime * SpeedChangeRate);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            // Normalise input direction
            Vector3 inputDirection = new Vector3(moveInput.x, 0.0f, moveInput.y).normalized;

            if (moveInput != Vector2.zero)
            {
                // Kameranın yönüne göre hareket hedef açısını belirle
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);
                
                // Karakteri hareket yönüne döndür
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
            
            // Fizik tabanlı hareketi uygula (Sadece X-Z düzleminde)
            Vector3 velocity = targetDirection.normalized * _speed;
            _rb.linearVelocity = new Vector3(velocity.x, _rb.linearVelocity.y, velocity.z);

            if (_animator != null)
            {
                _animator.SetFloat(_animIDSpeed, _speed);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void JumpAndGravity()
        {
            bool jumpInput = _inputSystem.Player.Jump.WasPressedThisFrame();

            if (isGrounded)
            {
                if (_animator != null)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // Normal zıplama kontrolü
                if (jumpInput && (_playerClimb == null || (!_playerClimb.canGrabLedge && !_playerClimb.isClimbing)))
                {
                    _rb.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
                    if (_animator != null) _animator.SetBool(_animIDJump, true);
                }
            }
            else
            {
                // Havada isek ve aşağı doğru -1.0 hızını geçtiysek düşme animasyonuna geç (toleransı kaldırdık)
                if (_rb.linearVelocity.y < -1.0f)
                {
                    if (_animator != null) _animator.SetBool(_animIDFreeFall, true);
                }
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (isGrounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // Zemin kontrol küresi (Karakterin tam ayak hizasında olmalı)
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }
    }
}
