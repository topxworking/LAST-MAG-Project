using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private RectTransform _crosshairRect;
    [SerializeField] private float _baseSize = 50f;
    [SerializeField] private float _maxSize = 120f;
    [SerializeField] private float _expandPerShot = 15f;
    [SerializeField] private float _shrinkSpeed = 10f;

    private float _currentSize;

    private void Start()
    {
        _currentSize = _baseSize;
        if (_crosshairRect == null) _crosshairRect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        _currentSize = Mathf.Lerp(_currentSize, _baseSize, Time.deltaTime * _shrinkSpeed);

        _crosshairRect.sizeDelta = new Vector2(_currentSize, _currentSize);
    }

    public void OnFire()
    {
        _currentSize = Mathf.Min(_currentSize + _expandPerShot, _maxSize);
    }
}