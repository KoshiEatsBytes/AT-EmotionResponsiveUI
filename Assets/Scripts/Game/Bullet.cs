using UnityEngine;

public enum BulletType
{
    Player,
    Enemy,
    Boss
}

public class Bullet : MonoBehaviour
{
    public BulletType bulletType;
    public int bulletDamage;

    private float _bulletSpeed;
    private Vector2 _bulletDirection;
    private static readonly Vector2 _screenBounds = new Vector2(900f, 1200f);

    public void InitBullet(BulletType owner, int damage, float speed, Vector2 localSpawnPoint, Vector2 direction)
    {
        gameObject.SetActive(true);

        bulletType = owner;
        bulletDamage = damage;
        _bulletSpeed = speed;
        _bulletDirection = direction;
        transform.localPosition = localSpawnPoint;
        float angle = Mathf.Atan2(_bulletDirection.y, _bulletDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    public void OnHitTarget()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        transform.localPosition = (Vector2)transform.localPosition + _bulletDirection * _bulletSpeed * Time.deltaTime;

        // Bullet hits map bounds
        if (transform.localPosition.x < -_screenBounds.x / 2 ||
            transform.localPosition.x > _screenBounds.x / 2 ||
            transform.localPosition.y < -_screenBounds.y / 2 ||
            transform.localPosition.y > _screenBounds.y / 2)
        {
            gameObject.SetActive(false);
        }
    }
}
