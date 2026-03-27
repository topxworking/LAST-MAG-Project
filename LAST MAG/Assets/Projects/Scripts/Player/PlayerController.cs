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

    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip[] _footstepSounds;
    [SerializeField] private float _footstepDistance = 2.5f;

    private float _distanceWalked;

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
        _isGrounded = _cc.isGrounded;

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

        if (_isGrounded && _moveInput.magnitude > 0.1f)
        {
            _distanceWalked += (new Vector3(move.x, 0, move.z).magnitude) * Time.deltaTime;
            if (_distanceWalked > _footstepDistance)
            {
                PlayFootstep();
                _distanceWalked = 0;
            }
        }
        else if (!_isGrounded)
        {
            Debug.Log("Player is NOT Grounded!");
        }
    }

    private void PlayFootstep()
    {
        Debug.Log("Footstep Played!");
        if (_footstepSounds.Length == 0) return;
        int index = Random.Range(0, _footstepSounds.Length);
        _audioSource.PlayOneShot(_footstepSounds[index], 0.4f);
    }

    private void ApplyLook()
    {
        float sens = SettingsManager.Instance != null
            ? (_isAiming ? SettingsManager.Instance.AimSensitivity
            : SettingsManager.Instance.MouseSensitivity)
    :       (_isAiming ? _aimSensitivity : _mouseSensitivity);

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
