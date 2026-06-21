using UnityEngine;
using System.Collections;

public enum MovementType
{
    Straight,
    Sine,
    Chaser
}

public class EnemyBase : MonoBehaviour
{
    public int health;
    public int collisionDamage;

    [SerializeField] protected float moveSpeed;
    [SerializeField] protected int bulletDamage;
    [SerializeField] protected float bulletSpeed;
    [SerializeField] protected float bulletFireCooldown;
    [SerializeField] protected bool isShooting;
    [SerializeField] protected MovementType movementType;

    protected Transform playerTransform;
    protected EnemyManager enemyManager;

    private Vector2 _lookVector;
    private bool _movementDirection;
    private static readonly Vector2 _screenBounds = new Vector2(820f, 1000f);

    public virtual void InitEnemy(Transform _playerTransform, EnemyManager _enemyManager)
    {
        playerTransform = _playerTransform;
        enemyManager = _enemyManager;

        if (isShooting || bulletFireCooldown > 0)
        {
            StartCoroutine(ShootAtPlayer());
        }
    }

    protected virtual IEnumerator ShootAtPlayer()
    {
        yield return new WaitForSeconds(1f + Random.Range(0f, 0.2f)); // Wait before shooting after spawning

        while (gameObject.activeSelf)
        {
            if (isShooting)
            {
                Vector2 aimVector = playerTransform.position - transform.position;
                aimVector.Normalize();
                enemyManager.GetBullet().InitBullet(BulletType.Enemy, bulletDamage, bulletSpeed, transform.localPosition, aimVector);
                yield return new WaitForSeconds(bulletFireCooldown);
            }

            yield return null;
        }
    }

    protected virtual void OnHit(Bullet bullet)
    {
        health -= bullet.bulletDamage;

        if (health <= 0)
        {
            enemyManager.OnEnemyDied(this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Bullet bullet))
        {
            if (bullet.bulletType == BulletType.Player)
            {
                OnHit(bullet);
                bullet.OnHitTarget();
            }
        }
    }

    protected virtual void Update()
    {
        MovementUpdate();
    }

    protected void MovementUpdate()
    {
        LookAtPlayer();

        switch (movementType)
        {
            case MovementType.Straight:
                MovementStraight();
                break;

            case MovementType.Sine:
                MovementSine();
                break;

            case MovementType.Chaser:
                MovementChaser();
                break;
        }
    }

    private void LookAtPlayer()
    {
        _lookVector = playerTransform.position - transform.position;
        _lookVector.Normalize();
        float angle = Mathf.Atan2(_lookVector.y, _lookVector.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + 90f);
    }

    private void MovementStraight()
    {
        if (_movementDirection)
        {
            Vector2 targetPos = (Vector2)transform.localPosition + new Vector2(1f, -0.1f) * moveSpeed * Time.deltaTime;

            if (targetPos.x >= _screenBounds.x / 2)
            {
                _movementDirection = !_movementDirection;
            }

            targetPos.x = Mathf.Clamp(targetPos.x, -_screenBounds.x / 2, _screenBounds.x / 2);
            targetPos.y = Mathf.Clamp(targetPos.y, -_screenBounds.y / 2, _screenBounds.y / 2);
            transform.localPosition = targetPos;
        }
        else
        {

            Vector2 targetPos = (Vector2)transform.localPosition + new Vector2(-1f, -0.1f) * moveSpeed * Time.deltaTime;

            if (targetPos.x <= -_screenBounds.x / 2)
            {
                _movementDirection = !_movementDirection;
            }

            targetPos.x = Mathf.Clamp(targetPos.x, -_screenBounds.x / 2, _screenBounds.x / 2);
            targetPos.y = Mathf.Clamp(targetPos.y, -_screenBounds.y / 2, _screenBounds.y / 2);
            transform.localPosition = targetPos;
        }
    }

    private void MovementSine()
    {
        float sineCoefficient = 0.015f;

        if (_movementDirection)
        {
            Vector2 targetPos = (Vector2)transform.localPosition + new Vector2(1f, Mathf.Sin(transform.localPosition.x * sineCoefficient)) * moveSpeed * Time.deltaTime;

            if (targetPos.x >= _screenBounds.x / 2)
            {
                _movementDirection = !_movementDirection;
            }

            targetPos.x = Mathf.Clamp(targetPos.x, -_screenBounds.x / 2, _screenBounds.x / 2);
            targetPos.y = Mathf.Clamp(targetPos.y, -_screenBounds.y / 2, _screenBounds.y / 2);
            transform.localPosition = targetPos;
        }
        else
        {

            Vector2 targetPos = (Vector2)transform.localPosition + new Vector2(-1f, Mathf.Sin(transform.localPosition.x * sineCoefficient)) * moveSpeed * Time.deltaTime;

            if (targetPos.x <= -_screenBounds.x / 2)
            {
                _movementDirection = !_movementDirection;
            }

            targetPos.x = Mathf.Clamp(targetPos.x, -_screenBounds.x / 2, _screenBounds.x / 2);
            targetPos.y = Mathf.Clamp(targetPos.y, -_screenBounds.y / 2, _screenBounds.y / 2);
            transform.localPosition = targetPos;
        }
    }

    private void MovementChaser()
    {
        Vector2 targetPos = (Vector2)transform.localPosition + _lookVector * moveSpeed * Time.deltaTime;
        targetPos.x = Mathf.Clamp(targetPos.x, -_screenBounds.x / 2, _screenBounds.x / 2);
        targetPos.y = Mathf.Clamp(targetPos.y, -_screenBounds.y / 2, _screenBounds.y / 2);
        transform.localPosition = targetPos;
    }
}
