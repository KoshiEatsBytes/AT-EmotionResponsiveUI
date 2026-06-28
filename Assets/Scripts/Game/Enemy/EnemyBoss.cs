using System.Collections;
using UnityEngine;

public class EnemyBoss : EnemyBase
{
    [SerializeField] private GameObject bulletPrefab;

    private void Awake()
    {
        CircularBulletPattern(20, 200f);
    }

    protected override void Update()
    {

    }

    protected override IEnumerator ShootAtPlayer()
    {
        yield return new WaitForSeconds(1f + Random.Range(0f, 0.2f)); // Wait before shooting after spawning

        while (gameObject.activeSelf)
        {
            if (isShooting)
            {

                yield return new WaitForSeconds(bulletFireCooldown);
            }

            yield return null;
        }
    }

    private void CircularBulletPattern(int numberOfBullets, float radius)
    {
        for (int i = 0; i < numberOfBullets; i++)
        {
            float angle = Mathf.Deg2Rad * (360 / numberOfBullets * i);
            Vector2 newPosition = new Vector2();
            newPosition.x = transform.localPosition.x + radius * Mathf.Sin(angle);
            newPosition.y = transform.localPosition.y - radius * Mathf.Cos(angle);

            GameObject newBullet = Instantiate(bulletPrefab, transform);
            newBullet.transform.localPosition = newPosition;   
        }
    }
}
