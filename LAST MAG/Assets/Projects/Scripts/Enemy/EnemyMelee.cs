using UnityEngine;

public class EnemyMelee : EnemyBase
{
    [SerializeField] private Renderer _bodyRenderer;
    [SerializeField] private Material _hitFlashMat;
    private Material _originalMat;
    private float _flashTimer;
    private Animator _animator;

    private static readonly int HashSpeed = Animator.StringToHash("Speed");
    private static readonly int HashIsIdle = Animator.StringToHash("IsIdle");

    protected override void Awake()
    {
        base.Awake();
        _animator = GetComponent<Animator>();
        if (_bodyRenderer != null)
            _originalMat = _bodyRenderer.material;
    }

    protected override void Update()
    {
        base.Update();

        if (_animator != null)
        {
            float speed = Agent.isOnNavMesh ? Agent.velocity.magnitude : 0f;
            _animator.SetFloat(HashSpeed, speed);
            _animator.SetBool(HashIsIdle, speed < 0.1f);
        }

        if (_flashTimer > 0f)
        {
            _flashTimer -= Time.deltaTime;
            if (_flashTimer <= 0f && _bodyRenderer != null)
                _bodyRenderer.material = _originalMat;
        }
    }

    protected override void OnHit(float amount)
    {
        if (_bodyRenderer == null || _hitFlashMat == null) return;
        _bodyRenderer.material = _hitFlashMat;
        _flashTimer = 0.1f;
    }
}
