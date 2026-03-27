using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputReader _input;

    [Header("Camera")]
    [SerializeField] private Transform _cameraRoot;
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private float _mouseSensitivity = 2f;
    [SerializeField] private float _aimSensitivity = 0.8f;
    [SerializeField] private float _pitchMin = -80f;
    [SerializeField] private float _pitchMax = 80f;

    [Header("Physics")]
    [SerializeField] private float _gravity = -20f;
    [SerializeField] private float _groundCheckDist = 0.15f;
    [SerializeField] private LayerMask _groundLayer;

    [Header("Aim")]
    [SerializeField] private float _aimFOV = 40f;
    [SerializeField] private float _normalFOV = 70f;
    [SerializeField] private float _aimFOVSpeed = 10f;

    private CharacterController _cc;
    private PlayerStats _stats;
    private PlayerShooter _shooter;

    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private float _pitch;
    private float _yaw;
    private float _verticalVelocity;

    private bool _isGrounded;
    private bool _isAiming;
    private bool _canMove = true;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _shooter = GetComponent<PlayerShooter>();
        _stats = GetComponent<PlayerHealth>().Stats;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        var health = GetComponent<PlayerHealth>();
        if (health != null)
            _stats = health.Stats;
    }

    private void OnEnable()
    {
        _input.OnMove += HandleMove;
        _input.OnLook += HandleLook;
        _input.OnJumpStarted += HandleJump;
        _input.OnShootStarted += HandleShootStart;
        _input.OnShootCanceled += HandleShootStop;
        _input.OnAimStarted += HandleAimStart;
        _input.OnAimCanceled += HandleAimStop;
        _input.OnReloadStarted += HandleReload;

        EventManager.OnUpgradeMenuOpened += DisableInput;
        EventManager.OnUpgradeMenuClosed += EnableInput;
        EventManager.OnPlayerDied += DisableInput;
    }

    private void OnDisable()
    {
        _input.OnMove -= HandleMove;
        _input.OnLook -= HandleLook;
        _input.OnJumpStarted -= HandleJump;
        _input.OnShootStarted -= HandleShootStart;
        _input.OnShootCanceled -= HandleShootStop;
        _input.OnAimStarted -= HandleAimStart;
        _input.OnAimCanceled -= HandleAimStop;
        _input.OnReloadStarted -= HandleReload;

        EventManager.OnUpgradeMenuOpened -= DisableInput;
        EventManager.OnUpgradeMenuClosed -= EnableInput;
        EventManager.OnPlayerDied -= DisableInput;
    }

    private void Update()
    {
        if (_stats == null) return;
        if (!_canMove) return;
        CheckGround();
        ApplyMovement();
        ApplyLook();
        UpdateFOV();
    }

    private void CheckGround()
    {
        _isGrounded = Physics.CheckSphere(
            transform.position + Vector3.down * (_cc.height * 0.5f),
            _groundCheckDist,
            _groundLayer
        );

        if (_isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = -2f;
    }

    private void ApplyMovement()
    {
        float spd = _stats.MoveSpeed;
        Vector3 dir = transform.right * _moveInput.x + transform.forward * _moveInput.y;

        _verticalVelocity += _gravity * Time.deltaTime;

        Vector3 move = dir * spd;
        move.y = _verticalVelocity;

        _cc.Move(move * Time.deltaTime);
    }

    private void ApplyLook()
    {
        float sens = _isAiming ? _aimSensitivity : _mouseSensitivity;
        _yaw += _lookInput.x * sens;
        _pitch -= _lookInput.y * sens;
        _pitch = Mathf.Clamp(_pitch, _pitchMin, _pitchMax);

        transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
        _cameraRoot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    private void UpdateFOV()
    {
        float targetFOV = _isAiming ? _aimFOV : _normalFOV;
        _playerCamera.fieldOfView = Mathf.Lerp(
            _playerCamera.fieldOfView, targetFOV, Time.deltaTime * _aimFOVSpeed);
    }

    private void HandleMove(Vector2 v) => _moveInput = v;
    private void HandleLook(Vector2 v) => _lookInput = v;

    private void HandleJump()
    {
        if (!_isGrounded) return;
        _verticalVelocity = Mathf.Sqrt(_stats.JumpForce * -2f * _gravity);
    }

    private void HandleShootStart() => _shooter?.StartShooting();
    private void HandleShootStop() => _shooter?.StopShooting();

    private void HandleAimStart()
    {
        _isAiming = true;
        _shooter?.SetAiming(true);
    }

    private void HandleReload() => _shooter?.RequestReload();

    private void HandleAimStop()
    {
        _isAiming = false;
        _shooter?.SetAiming(false);
    }

    private void EnableInput()
    {
        _canMove = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void DisableInput()
    {
        _canMove = false;
        _moveInput = Vector2.zero;
        _shooter?.StopShooting();
    }

    public void AddRecoil(float pitchKick, float yawKick)
    {
        _pitch -= pitchKick;
        _yaw += yawKick;
        _pitch = Mathf.Clamp(_pitch, _pitchMin, _pitchMax);
    }

    private void OnDrawGizmosSelected()
    {
        if (_cc == null) _cc = GetComponent<CharacterController>();
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(
            transform.position + Vector3.down * (_cc.height * 0.5f),
            _groundCheckDist);
    }
}
