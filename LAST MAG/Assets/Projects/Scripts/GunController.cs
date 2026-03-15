using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;

public class GunController : MonoBehaviour
{
    [Header("Gun Stats")]
    public int maxAmmo = 30;
    public float fireRate = 0.1f;
    public float damage = 10f;
    public float range = 100f;
    public float reloadTime = 2f;

    [Header("Spread Settings")]
    public float baseSpread = 0.01f;
    public float maxSpread = 0.1f;
    public float spreadIncreaseRate = 0.015f;
    public float spreadRecoveryRate = 0.05f;

    [Header("Recoil Settings")]
    public float recoilUp = 1.5f;
    public float recoilSide = 0.5f;

    [Header("Aiming Settings")]
    public float normalFOV = 60f;
    public float aimFOV = 40f;
    public float aimSpeed = 10f;

    [Header("References")]
    public Camera playerCamera;
    public PlayerController pc;
    public TextMeshProUGUI ammoText;

    [Header("Inputs")]
    public InputActionReference fireAction;
    public InputActionReference aimAction;
    public InputActionReference reloadAction;

    private int currentAmmo;
    private bool isReloading = false;
    private float nextTimeToFire = 0f;
    private float currentSpread = 0f;

    void Start()
    {
        currentAmmo = maxAmmo;
        playerCamera.fieldOfView = normalFOV;
    }

    private void OnEnable()
    {
        fireAction.action.Enable();
        aimAction.action.Enable();
        reloadAction.action.Enable();

        reloadAction.action.performed += ctx => TryReload();
    }

    private void OnDisable()
    {
        fireAction.action.Disable();
        aimAction.action.Disable();
        reloadAction.action.Disable();

        reloadAction.action.performed -= ctx => TryReload();
    }

    void Update()
    {
        if (isReloading) return;

        bool isAiming = aimAction.action.IsPressed();
        bool isShooting = fireAction.action.IsPressed();

        HandleAiming(isAiming);
        HanddleShooting(isAiming, isShooting);

        if (currentAmmo <= 0 && !isReloading)
        {
            StartCoroutine(Reload());
        }

        ammoText.text = currentAmmo + "/" + maxAmmo;
    }

    void HanddleShooting(bool isAiming, bool isShooting)
    {
        if (isShooting && Time.time > nextTimeToFire && currentAmmo > 0)
        {
            nextTimeToFire = Time.time + fireRate;
            Shoot();

            float spreadMultiplier = isAiming ? 0.4f : 1f;
            currentSpread = Mathf.Clamp(currentSpread + (spreadIncreaseRate * spreadMultiplier), baseSpread, maxSpread);
        }
        else if (!isShooting)
        {
            currentSpread = Mathf.MoveTowards(currentSpread, baseSpread, spreadRecoveryRate * Time.deltaTime);
        }
    }

    void Shoot()
    {
        currentAmmo--;

        float xOffset = Random.Range(-currentSpread, currentSpread);
        float yOffset = Random.Range(-currentSpread, currentSpread);
        Vector3 shootDirection = playerCamera.transform.forward + new Vector3(xOffset, yOffset, 0);

        if (Physics.Raycast(playerCamera.transform.position, shootDirection, out RaycastHit hit, range))
        {
            Debug.Log("ยิงโดน: " + hit.transform.name);

            EnemyController enemy = hit.transform.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }

        float randomSideRecoil = Random.Range(-recoilSide, recoilSide);
        if (pc != null)
        {
            pc.AddRecoil(recoilUp, randomSideRecoil);
        }
    }

    void HandleAiming(bool isAiming)
    {
        float targetFOV = isAiming ? aimFOV : normalFOV;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, aimSpeed * Time.deltaTime);
    }

    void TryReload()
    {
        if (!isReloading && currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;
    }
}
