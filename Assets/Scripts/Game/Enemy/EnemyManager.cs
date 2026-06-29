using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private ShipController playerShip;
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private Bullet bossBulletPrefab;
    [SerializeField] private Transform bulletParent;
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField] private List<EnemyBase> enemyPrefabs;
    [SerializeField] private List<EnemyBase> bossPrefabs;

    private List<EnemyBase> _currentWaveEnemies;
    private List<Bullet> _bulletPool;
    private List<Bullet> _bossBulletPool;
    private int _enemyToSpawn;
    private int _currentWave;
    private int _lastBossWave;
    private int _bossWaveInterval;

    private static Vector2 _enemySpawnBoundsMin = new Vector2(-460f, 80f);
    private static Vector2 _enemySpawnBoundsMax = new Vector2(460f, 540f);
    private static float _nextWaveDelay = 2f;

    private void Start()
    {
        _currentWaveEnemies = new List<EnemyBase>();
        _bulletPool = new List<Bullet>();
        _bossBulletPool = new List<Bullet>();
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

    public Bullet GetBossBullet()
    {
        for (int i = 0; i < _bossBulletPool.Count; i++)
        {
            if (!_bossBulletPool[i].gameObject.activeSelf)
            {
                return _bossBulletPool[i];
            }
        }

        Bullet newBullet = Instantiate(bossBulletPrefab, bulletParent);
        newBullet.gameObject.SetActive(false);
        _bossBulletPool.Add(newBullet);
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
        // Adjust boss wave intervals based on player performance
        if (_lastBossWave == _currentWave)
        {
            var hitRatios = EmotionResponseManager.Instance.GetHitRatio();
            if (hitRatios != null)
            {
                // over 50% of hit done by boss
                if (EmotionResponseManager.Instance.GetHitRatio()[0] >= 0.5)
                {
                    _bossWaveInterval = Mathf.Clamp(_bossWaveInterval++, 1, 5);
                }
                // Less than 30% of hit done by boss
                else if (EmotionResponseManager.Instance.GetHitRatio()[0] <= 0.3)
                {
                    _bossWaveInterval = Mathf.Clamp(_bossWaveInterval--, 1, 5);
                }
            }
        }

        _currentWave++;
        Debug.Log($"Wave {_currentWave}");

        yield return new WaitForSeconds(_nextWaveDelay);

        if (_currentWave - _lastBossWave >= _bossWaveInterval)
        {
            // Spawn normal wave instead of boss in very high stress condition
            if (EmotionResponseManager.Instance.emotionScore <= 90 || _currentWave - _lastBossWave >= 5)
            {
                SpawnBossWave();
                _lastBossWave = _currentWave;
            }
            else
            {
                SpawnNormalWave();
            }
        }
        else
        {
            // Spawn boss wave instead of normal wave in very low stress condition
            if (EmotionResponseManager.Instance.emotionScore <= 10 && _currentWave - _lastBossWave >= 2)
            {
                SpawnBossWave();
                _lastBossWave = _currentWave;
            }
            else
            {
                SpawnNormalWave();
            }
        }
    }

    private void SpawnNormalWave()
    {
        if (EmotionResponseManager.Instance.emotionScore <= 40)
        {
            // Low stress, increase difficulty
            _enemyToSpawn = Mathf.Clamp(_enemyToSpawn + 1, 3, 10);
            var hitRatios = EmotionResponseManager.Instance.GetHitRatio();
            if (hitRatios == null)
            {
                SpawnRandomEnemy(_enemyToSpawn);
                return;
            }
            // Spawn more enemy type the player struggled with
            int rangedEnemyToSpawn = Mathf.RoundToInt(hitRatios[3] * _enemyToSpawn);
            int meleeEnemyToSpawn = _enemyToSpawn - rangedEnemyToSpawn;
            rangedEnemyToSpawn = Mathf.Clamp(rangedEnemyToSpawn, 2, 10);
            meleeEnemyToSpawn = Mathf.Clamp(meleeEnemyToSpawn, 2, 10);
            Debug.Log($"{_enemyToSpawn}, {rangedEnemyToSpawn}, {meleeEnemyToSpawn}");
            SpawnRangedEnemy(rangedEnemyToSpawn);
            SpawnMeleeEnemy(meleeEnemyToSpawn);
        }
        else if (EmotionResponseManager.Instance.emotionScore >= 60)
        {
            // High stress, lower difficulty
            _enemyToSpawn = Mathf.Clamp(_enemyToSpawn - 1, 3, 10);
            var hitRatios = EmotionResponseManager.Instance.GetHitRatio();
            if (hitRatios == null)
            {
                SpawnRandomEnemy(_enemyToSpawn);
                return;
            }
            // Spawn less enemy type the player struggled with
            int rangedEnemyToSpawn = Mathf.RoundToInt(1 - hitRatios[3] * _enemyToSpawn);
            int meleeEnemyToSpawn = _enemyToSpawn - rangedEnemyToSpawn;
            rangedEnemyToSpawn = Mathf.Clamp(rangedEnemyToSpawn, 2, 10);
            meleeEnemyToSpawn = Mathf.Clamp(meleeEnemyToSpawn, 2, 10);
            Debug.Log($"{_enemyToSpawn}, {rangedEnemyToSpawn}, {meleeEnemyToSpawn}");
            SpawnRangedEnemy(rangedEnemyToSpawn);
            SpawnMeleeEnemy(meleeEnemyToSpawn);
        }
        else
        {
            // Flow state
            SpawnRandomEnemy(_enemyToSpawn);
        }
    }

    private void SpawnBossWave()
    {
        EnemyBase newBoss = Instantiate(bossPrefabs[Random.Range(0, bossPrefabs.Count)], transform);
        newBoss.InitEnemy(playerShip.transform, this);
        newBoss.transform.localPosition = bossSpawnPoint.localPosition;
        _currentWaveEnemies.Add(newBoss);
    }

    private Vector2 GetRandomSpawnPos()
    {
        return new Vector2(Random.Range(_enemySpawnBoundsMin.x, _enemySpawnBoundsMax.x), 
                           Random.Range(_enemySpawnBoundsMin.y, _enemySpawnBoundsMax.y));
    }

    private void SpawnMeleeEnemy(int numberToSpawn)
    {
        for (int i = 0; i < numberToSpawn; i++)
        {
            EnemyBase newEnemy = Instantiate(enemyPrefabs[0], transform);
            newEnemy.InitEnemy(playerShip.transform, this);
            newEnemy.transform.localPosition = GetRandomSpawnPos();
            _currentWaveEnemies.Add(newEnemy);
        }
    }

    private void SpawnRangedEnemy(int numberToSpawn)
    {
        for (int i = 0; i < numberToSpawn; i++)
        {
            int random = Random.Range(1, 3);
            EnemyBase newEnemy = Instantiate(enemyPrefabs[random], transform);
            newEnemy.InitEnemy(playerShip.transform, this);
            newEnemy.transform.localPosition = GetRandomSpawnPos();
            _currentWaveEnemies.Add(newEnemy);
        }
    }

    private void SpawnRandomEnemy(int numberToSpawn)
    {
        for (int i = 0; i < numberToSpawn; i++)
        {
            int randomEnemy = Random.Range(0, enemyPrefabs.Count);
            EnemyBase newEnemy = Instantiate(enemyPrefabs[randomEnemy], transform);
            newEnemy.InitEnemy(playerShip.transform, this);
            newEnemy.transform.localPosition = GetRandomSpawnPos();
            _currentWaveEnemies.Add(newEnemy);
        }
    }
}
