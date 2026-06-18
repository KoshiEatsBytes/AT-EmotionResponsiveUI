using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShipController : MonoBehaviour
{
    // Serialized
    [SerializeField] private float moveSpeed;
    [SerializeField] private float bulletFireCooldown;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private int bulletDamage;
    [SerializeField] private GameObject aimCursor;
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private Transform bulletParent;

    // Input Values
    private Vector2 _moveDir;
    private Vector2 _mousePos;
    private bool _isAttacking;

    private static readonly Vector2 _screenBounds = new Vector2(820f, 1000f);
    private InputSystem_Actions _playerInputs;
    private Camera _mainCam;
    private Vector2 _aimVector;
    private List<Bullet> _bulletPool;

    private void Awake()
    {
        _playerInputs = new InputSystem_Actions();
        _playerInputs.Enable();
        _mainCam = Camera.main;

        _bulletPool = new List<Bullet>();
        StartCoroutine(BulletFireCoroutine());
    }

    private void Update()
    {
        HandleInputs();
        HandleMovement();
        HandleAttack();
    }

    private void HandleInputs()
    {
        _moveDir = _playerInputs.Player.Move.ReadValue<Vector2>();
        _mousePos = _playerInputs.Player.Aim.ReadValue<Vector2>();
        _isAttacking = _playerInputs.Player.Attack.IsPressed();
    }

    private void HandleMovement()
    {
        Vector2 targetPos = (Vector2)transform.localPosition + _moveDir * moveSpeed * Time.deltaTime;
        targetPos.x = Mathf.Clamp(targetPos.x, -_screenBounds.x / 2, _screenBounds.x / 2);
        targetPos.y = Mathf.Clamp(targetPos.y, -_screenBounds.y / 2, _screenBounds.y / 2);
        transform.localPosition = targetPos;
    }

    private void HandleAttack()
    {
        // Aim Cursor
        Vector3 mouseWorldPos = _mainCam.ScreenToWorldPoint(_mousePos);
        aimCursor.transform.position = mouseWorldPos;

        // Ship Rotation
        _aimVector = mouseWorldPos - transform.position;
        _aimVector.Normalize();
        float angle = Mathf.Atan2(_aimVector.y, _aimVector.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    private Bullet GetBullet()
    {
        for(int i = 0; i < _bulletPool.Count; i++)
        {
            if (!_bulletPool[i].gameObject.activeSelf)
            {
                return _bulletPool[i];
            }
        }

        Bullet newBullet = Instantiate(bulletPrefab, bulletParent);
        newBullet.gameObject.SetActive(false);
        _bulletPool.Add(newBullet);
        return newBullet;
    }

    private IEnumerator BulletFireCoroutine()
    {
        while (gameObject.activeSelf)
        {
            if (_isAttacking)
            {
                GetBullet().InitBullet(BulletType.Player, bulletDamage, bulletSpeed, transform.localPosition, _aimVector);
                yield return new WaitForSeconds(bulletFireCooldown);
            }

            yield return null;
        }
    }
}
