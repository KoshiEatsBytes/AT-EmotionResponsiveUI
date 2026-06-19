using UnityEngine;

public class EnemyRanged : EnemyBase
{
    public override void InitEnemy(Transform _playerTransform, EnemyManager _enemyManager)
    {
        playerTransform = _playerTransform;
        enemyManager = _enemyManager;

        StartCoroutine(ShootAtPlayer());
        isShooting = true;
    }

    private void Update()
    {
        
    }
}
