using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private ShipController playerShip;
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private Transform bulletParent;
    [SerializeField] private List<EnemyBase> enemyPrefabs;

    private List<EnemyBase> _currentWaveEnemies;
    private List<Bullet> _bulletPool;
    private int _enemyToSpawn;
    private int _currentWave;
    private int _lastBossWave;
    private int _bossWaveInterval;

    private static Vector2 _enemySpawnBoundsMin = new Vector2(-460f, 80f);
    private static Vector2 _enemySpawnBoundsMax = new Vector2(460f, 540f);
    private static float _nextWaveDelay = 2f;

    private void Awake()
    {
        _currentWaveEnemies = new List<EnemyBase>();
        _bulletPool = new List<Bullet>();
        _enemyToSpawn = 5;
        _currentWave = 1;
        _lastBossWave = 1;
        _bossWaveInterval = 3;

        SpawnNormalWave();
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

    public void OnEnemyDied(EnemyBase deadEnemy)
    {
        _currentWaveEnemies.Remove(deadEnemy);
        Destroy(deadEnemy.gameObject);

        if (_currentWaveEnemies.Count == 0)
        {
            StartCoroutine(OnWaveFinished());
        }
    }

    private IEnumerator OnWaveFinished()
    {
        _currentWave++;
        Debug.Log($"Wave {_currentWave}");

        yield return new WaitForSeconds(_nextWaveDelay);

        if (_currentWave - _lastBossWave >= _bossWaveInterval)
        {
            SpawnBossWave();
        }
        else
        {
            SpawnNormalWave();
        }
    }

    private void SpawnNormalWave()
    {
        for (int i = 0; i < _enemyToSpawn; i++)
        {
            int randomEnemy = Random.Range(0, enemyPrefabs.Count);
            EnemyBase newEnemy = Instantiate(enemyPrefabs[randomEnemy], transform);
            newEnemy.InitEnemy(playerShip.transform, this);
            newEnemy.transform.localPosition = GetRandomSpawnPos();
            _currentWaveEnemies.Add(newEnemy);
        }
    }

    private void SpawnBossWave()
    {
        Debug.Log("Spawn Boss");
    }

    private Vector2 GetRandomSpawnPos()
    {
        return new Vector2(Random.Range(_enemySpawnBoundsMin.x, _enemySpawnBoundsMax.x), 
                           Random.Range(_enemySpawnBoundsMin.y, _enemySpawnBoundsMax.y));
    }
}
