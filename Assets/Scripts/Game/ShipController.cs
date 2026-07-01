using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShipController : MonoBehaviour
{
    public int health;
    public bool erraticMouseMovement;
    public bool isSpammingDodge;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float bulletFireCooldown;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private int bulletDamage;
    [SerializeField] private GameObject aimCursor;
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private Transform bulletParent;
    [SerializeField] private float hitCooldown;
    [SerializeField] private float dodgeDuration;
    [SerializeField] private float dodgeCooldown;
    [SerializeField] private float dodgeSpeed;
    [SerializeField] private GameObject shieldSprite;

    private Vector2 _moveDir;
    private Vector2 _mousePos;
    private bool _isAttacking;
    private bool _isOnHitCooldown;
    private bool _pressedDodge;
    private bool _canDodge;
    private bool _isImmune;
    private bool _isDashing;

    private static readonly Vector2 _screenBounds = new Vector2(820f, 1000f);
    private InputSystem_Actions _playerInputs;
    private Camera _mainCam;
    private Vector2 _aimVector;
    private List<Bullet> _bulletPool;

    private Queue<float> _mouseDisplacements;
    private Vector2 _lastMousePos;
    private float _lastDodgeTime;
    private float _dodgeSpamTimer;

    private void Awake()
    {
        _playerInputs = new InputSystem_Actions();
        _playerInputs.Enable();
        _mainCam = Camera.main;
        _isOnHitCooldown = false;
        _canDodge = true;
        _isImmune = false;
        _isDashing = false;

        _bulletPool = new List<Bullet>();
        _mouseDisplacements = new Queue<float>();
        _lastMousePos = Vector2.zero;
        StartCoroutine(BulletFireCoroutine());
    }

    private void Update()
    {
        HandleInputs();
        HandleMovement();
        HandleAttack();
        HandleDodge();
    }

    private void FixedUpdate()
    {
        DetectErraticMouse();
    }

    private void HandleInputs()
    {
        _moveDir = _playerInputs.Player.Move.ReadValue<Vector2>();
        _mousePos = _playerInputs.Player.Aim.ReadValue<Vector2>();
        _isAttacking = _playerInputs.Player.Attack.IsPressed();
        _pressedDodge = _playerInputs.Player.Dodge.IsPressed();
    }

    private void HandleMovement()
    {
        Vector2 targetPos;

        if (_isDashing)
        {
            targetPos = (Vector2)transform.localPosition + _moveDir * dodgeSpeed * Time.deltaTime;
            targetPos.x = Mathf.Clamp(targetPos.x, -_screenBounds.x / 2, _screenBounds.x / 2);
            targetPos.y = Mathf.Clamp(targetPos.y, -_screenBounds.y / 2, _screenBounds.y / 2);
            transform.localPosition = targetPos;
            return;
        }

        targetPos = (Vector2)transform.localPosition + _moveDir * moveSpeed * Time.deltaTime;
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

    private void HandleDodge()
    {
        if (_pressedDodge && _canDodge)
        {
            StartCoroutine(DodgeCoroutine());
            DetectDodgeSpam();
        }

        if (_dodgeSpamTimer >= 0)
        {
            _dodgeSpamTimer -= Time.deltaTime;
        }
        else
        {
            isSpammingDodge = false;
        }
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

    private void OnPlayerDied()
    {
        Debug.Log("Player Died!");
    }

    private void OnHit(Bullet bullet)
    {
        bullet.OnHitTarget();

        if (_isOnHitCooldown) return;

        if (_isImmune)
        {
            // Player did a successful dodge
            EmotionResponseManager.Instance.EmotionInput(EmotionInputType.PlayerSuccessfulDodge, null);
            return;
        }

        if (!_canDodge)
        {
            // Player got hit after dodging (failed dodge)
            EmotionResponseManager.Instance.EmotionInput(EmotionInputType.PlayerFailedDodge, null);
        }

        switch (bullet.bulletType)
        {
            case BulletType.Enemy:
                EmotionResponseManager.Instance.EmotionInput(EmotionInputType.PlayerHitByBullet, bullet);
                break;

            case BulletType.Boss:
                EmotionResponseManager.Instance.EmotionInput(EmotionInputType.PlayerHitByBoss, bullet);
                break;
        }

        health -= bullet.bulletDamage;
        StartCoroutine(HitCooldown());

        if (health <= 0)
        {
            OnPlayerDied();
        }
    }

    private void OnHit(EnemyBase enemy)
    {
        if (_isOnHitCooldown) return;

        if (_isImmune)
        {
            // Player did a successful dodge
            EmotionResponseManager.Instance.EmotionInput(EmotionInputType.PlayerSuccessfulDodge, null);
            return;
        }

        if (!_canDodge)
        {
            // Player got hit after dodging (failed dodge)
            EmotionResponseManager.Instance.EmotionInput(EmotionInputType.PlayerFailedDodge, null);
        }

        EmotionResponseManager.Instance.EmotionInput(EmotionInputType.PlayerHitByCollision, enemy);
        health -= enemy.collisionDamage;
        StartCoroutine(HitCooldown());

        if (health <= 0)
        {
            OnPlayerDied();
        }
    }

    private IEnumerator HitCooldown()
    {
        _isOnHitCooldown = true;

        yield return new WaitForSeconds(hitCooldown);

        _isOnHitCooldown = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Bullet bullet))
        {
            if (bullet.bulletType != BulletType.Player)
            {
                OnHit(bullet);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.TryGetComponent(out EnemyBase enemy))
        {
            OnHit(enemy);
        }
    }

    private IEnumerator DodgeCoroutine()
    {
        _canDodge = false;
        _isImmune = true;
        _isDashing = true;
        shieldSprite.SetActive(true);

        yield return new WaitForSeconds(dodgeDuration / 3f);

        _isDashing = false;

        yield return new WaitForSeconds(dodgeDuration * 2f/3f);

        _isImmune = false;
        shieldSprite.SetActive(false);

        yield return new WaitForSeconds(dodgeCooldown);

        _canDodge = true;
    }

    private void DetectErraticMouse()
    {
        float mouseDisplacement = Vector2.Distance(_mousePos, _lastMousePos);
        _lastMousePos = _mousePos;
        _mouseDisplacements.Enqueue(mouseDisplacement);

        if (_mouseDisplacements.Count >= 10)
        {
            _mouseDisplacements.Dequeue();
        }

        float totalDisplacement = 0;
        foreach (float i in _mouseDisplacements)
        {
            totalDisplacement += i;
        }

        if (totalDisplacement > 600f)
        {
            erraticMouseMovement = true;
        }
        else
        {
            erraticMouseMovement = false;
        }
    }

    private void DetectDodgeSpam()
    {
        float timeSinceLastDodge = Time.time - _lastDodgeTime;
        _lastDodgeTime = Time.time;

        if (timeSinceLastDodge <= (dodgeDuration + dodgeCooldown) * 1.1f)
        {
            isSpammingDodge = true;
            _dodgeSpamTimer = (dodgeDuration + dodgeCooldown) * 1.1f;
        }
    }
}
