using UnityEngine;
using System.Collections;

public class EnemyBase : MonoBehaviour
{
    public int health;

    [SerializeField] protected float moveSpeed;
    [SerializeField] protected int bulletDamage;
    [SerializeField] protected float bulletSpeed;
    [SerializeField] protected float bulletFireCooldown;
    [SerializeField] protected bool isShooting;

    protected Transform playerTransform;
    protected EnemyManager enemyManager;

    public void InitEnemy(Transform _playerTransform, EnemyManager _enemyManager)
    {
        playerTransform = _playerTransform;
        enemyManager = _enemyManager;

        StartCoroutine(ShootAtPlayer());
        isShooting = true;
    }

    protected IEnumerator ShootAtPlayer()
    {
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

    protected void OnHit(Bullet bullet)
    {
        health -= bullet.bulletDamage;

        if (health <= 0)
        {
            Destroy(gameObject);
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
}
