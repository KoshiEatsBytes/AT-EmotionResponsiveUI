using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;

public enum EmotionInputType
{
    PlayerHitByBullet,
    PlayerHitByCollision,
    PlayerHitByBoss,
    PlayerFailedDodge,
    PlayerSuccessfulDodge
}

public class EmotionResponseManager : MonoBehaviour
{
    [SerializeField] private bool outputEmotionLog;

    public static EmotionResponseManager Instance;

    public float emotionScore; // 0 (low stress) to 100 (high stress)

    private float _lastNegativeInputTime;
    private static float _negativeInputChainDuration = 3f; // If player receives multiple negative input within this duration, they are considered as a chain
    private int _negativeInputChainCount;

    private static float _emotionRegenDelay = 5f;
    private static float _emotionRegenRate = 2f;
    private float _emotionRegenCounter;

    private static float _emotionDecayDodgeSpam = 1.5f;
    private static float _emotionDecayErraticMouse = 1.5f;

    private ShipController _shipController;
    private StringBuilder _stringBuilder;

    private int _hitsByBullet;
    private int _hitsByCollision;
    private int _hitsByBoss;

    private void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(this);
            Instance = this;

            emotionScore = 50f;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        _stringBuilder = new StringBuilder();
        _stringBuilder.Append("Game Time,Emotion Input Type,Emotion Score Delta,Emotion Score Before,Emotion Score After");
        _shipController = FindFirstObjectByType<ShipController>();
    }

    private void OnDestroy()
    {
        if (outputEmotionLog)
        {
            ExportEmotionLog();
        }
    }

    private void Update()
    {
        emotionScore = Mathf.Clamp(emotionScore, 0f, 100f);
        EmotionRegen();

        if (_shipController.isSpammingDodge)
        {
            _emotionRegenCounter = 1f;
            emotionScore -= _emotionDecayDodgeSpam * Time.deltaTime;
        }

        if (_shipController.erraticMouseMovement)
        {
            _emotionRegenCounter = 1f;
            emotionScore -= _emotionDecayErraticMouse * Time.deltaTime;
        }
    }

    public void EmotionInput(EmotionInputType inputType, Object data)
    {
        float emotionScoreBefore = emotionScore;

        switch (inputType)
        {
            case EmotionInputType.PlayerHitByBullet:
                _hitsByBullet++;
                float bulletEmotionCoefficient = 10f;
                Bullet bullet = (Bullet)data;
                if (NegativeInputChain())
                {
                    emotionScore += bulletEmotionCoefficient * bullet.bulletDamage * GetNegativeChainMultiplier(_negativeInputChainCount);
                }
                else
                {
                    emotionScore += bulletEmotionCoefficient * bullet.bulletDamage;
                }
                break;

            case EmotionInputType.PlayerHitByCollision:
                _hitsByCollision++;
                float collisionEmotionCoefficient = 12f;
                EnemyBase enemy = (EnemyBase)data;
                if (NegativeInputChain())
                {
                    emotionScore += collisionEmotionCoefficient * enemy.collisionDamage * GetNegativeChainMultiplier(_negativeInputChainCount);
                }
                else
                {
                    emotionScore += collisionEmotionCoefficient * enemy.collisionDamage;
                }
                break;

            case EmotionInputType.PlayerHitByBoss:
                _hitsByBoss++;
                float bossBulletEmotionCoefficient = 15f;
                Bullet bossBullet = (Bullet)data;
                if (NegativeInputChain())
                {
                    emotionScore += bossBulletEmotionCoefficient * bossBullet.bulletDamage * GetNegativeChainMultiplier(_negativeInputChainCount);
                }
                else
                {
                    emotionScore += bossBulletEmotionCoefficient * bossBullet.bulletDamage;
                }
                break;

            case EmotionInputType.PlayerFailedDodge:
                if (NegativeInputChain())
                {
                    emotionScore += 5f * GetNegativeChainMultiplier(_negativeInputChainCount);
                }
                else
                {
                    emotionScore += 5f;
                }
                break;

            case EmotionInputType.PlayerSuccessfulDodge:
                emotionScore -= 5f;
                break;
        }

        float emotionScoreDelta = emotionScore - emotionScoreBefore;
        _stringBuilder.Append($"\n{RoundTo2Dec(Time.time)},{inputType},{RoundTo2Dec(emotionScoreDelta)},{RoundTo2Dec(emotionScoreBefore)},{RoundTo2Dec(emotionScore)}");
    }

    private float RoundTo2Dec(float input)
    {
        return Mathf.Round(input * 100f) / 100f;
    }

    /// <summary>
    /// Checks if negative input is considered as a chain
    /// </summary>
    /// <returns>True if it is a chain, False if it is not</returns>
    private bool NegativeInputChain()
    {
        _emotionRegenCounter = _emotionRegenDelay; // Pauses passive emotion regen due to negative input

        if (_lastNegativeInputTime == 0f)
        {
            _lastNegativeInputTime = Time.time;
            return false;
        }
        else
        {
            float timeSinceLastNegativeInput = Time.time - _lastNegativeInputTime;
            _lastNegativeInputTime = Time.time;
            if (timeSinceLastNegativeInput <= _negativeInputChainDuration)
            {
                // Last negative input happens within chain window
                _negativeInputChainCount++;
                return true;
            }
            else
            {
                // Last negative input happens outside chain window
                _negativeInputChainCount = 0;
                return false;
            }
        }
    }

    private void EmotionRegen()
    {
        if (_emotionRegenCounter <= 0)
        {
            // If no negative input happens for a while, emotionScore will passively drop to 0
            // This means the player is finding the game too easy, should increase challenge to try and get the emotion score around 50
            if (emotionScore > 0f)
            {
                emotionScore -= _emotionRegenRate * Time.deltaTime;
            }
        }
        else
        {
            _emotionRegenCounter -= Time.deltaTime;
        }
    }

    public void ExportEmotionLog()
    {
        string content = _stringBuilder.ToString();
        string date = System.DateTime.Now.ToString().Replace('/', '_').Replace(':', '_');
        string fileName = $"Emotion Score Log {date}.csv";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        using var writer = new StreamWriter(filePath, false);
        writer.Write(content);

        Debug.Log($"Successfully exported file to -> {Application.persistentDataPath}");
    }

    public List<float> GetHitRatio()
    {
        int totalHits = _hitsByBoss + _hitsByBullet + _hitsByCollision;
        int nonBossHits = _hitsByBullet + _hitsByCollision;

        if (totalHits == 0) return null;

        List<float> hitRatios = new List<float>();
        hitRatios.Add(_hitsByBoss / totalHits);
        if (nonBossHits != 0)
        {
            hitRatios.Add(_hitsByBullet / nonBossHits);
            hitRatios.Add(_hitsByCollision / nonBossHits);
        }
        else
        {
            hitRatios.Add(0.5f);
            hitRatios.Add(0.5f);
        }

        return hitRatios;
    }

    private float GetNegativeChainMultiplier(int chainCount)
    {
        return (0.04f * Mathf.Pow(chainCount, 2) + 1);
    }
}
