using System.Collections;
using UnityEngine;

public class EnemyBoss : EnemyBase
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float angleOffsetStep;
    [SerializeField] private int bulletsPerCircle;

    private float _angleOffset;

    private void Awake()
    {
        _angleOffset = 0f;

        float emotionScore = EmotionResponseManager.Instance.emotionScore;

        if (emotionScore <= 25)
        {
            // Increase boss difficulty
            bulletFireCooldown -= 0.2f;
            bulletsPerCircle += 3;
            health += 15;
        }
        else if (emotionScore >= 75)
        {
            // Lower boss difficulty
            bulletFireCooldown += 0.2f;
            bulletsPerCircle -= 3;
            health -= 15;
        }
    }

    protected override void Update()
    {
        MovementUpdate();
    }

    protected override IEnumerator ShootAtPlayer()
    {
        yield return new WaitForSeconds(1f + Random.Range(0f, 0.2f)); // Wait before shooting after spawning

        while (gameObject.activeSelf)
        {
            if (isShooting)
            {
                CircularBulletPattern(bulletsPerCircle, 100f, _angleOffset);
                _angleOffset += angleOffsetStep;
                yield return new WaitForSeconds(bulletFireCooldown);
            }

            yield return null;
        }
    }

    private void CircularBulletPattern(int numberOfBullets, float radius, float offset)
    {
        for (int i = 0; i < numberOfBullets; i++)
        {
            float angle = Mathf.Deg2Rad * (360 / numberOfBullets * i + offset);
            Vector2 spawnPos = new Vector2();
            spawnPos.x = transform.localPosition.x + radius * Mathf.Sin(angle);
            spawnPos.y = transform.localPosition.y - radius * Mathf.Cos(angle);
            Vector2 direction = (spawnPos - (Vector2)transform.localPosition).normalized;

            enemyManager.GetBossBullet().InitBullet(BulletType.Boss, bulletDamage, bulletSpeed, spawnPos, direction);
        }
    }
}
