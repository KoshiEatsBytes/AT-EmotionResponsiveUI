using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private ShipController playerShip;
    [SerializeField] private EnemyBase enemyPrefab;
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private Transform bulletParent;

    private List<EnemyBase> _spawnedEnemies;
    private List<Bullet> _bulletPool;

    private void Awake()
    {
        _spawnedEnemies = new List<EnemyBase>();
        _bulletPool = new List<Bullet>();

        EnemyBase newEnemy = Instantiate(enemyPrefab, transform);
        newEnemy.InitEnemy(playerShip.transform, this);
        _spawnedEnemies.Add(newEnemy);
    }

    public Bullet GetBullet()
    {
        for (int i = 0; i < _bulletPool.Count; i++)
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
}
