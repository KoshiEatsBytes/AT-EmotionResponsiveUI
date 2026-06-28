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
    public static EmotionResponseManager Instance;

    public float emotionScore; // 0 (low stress) to 100 (high stress)

    private float _lastNegativeInputTime;
    private float _negativeInputChainDuration; // If player receives multiple negative input within this duration, they are considered as a chain
    private int _negativeInputChainCount;
    private static float _negativeInputChainCoefficient = 0.3f;

    private static float _emotionRegenDelay = 5f;
    private static float _emotionRegenRate = 2f;
    private float _emotionRegenCounter;

    private StringBuilder _stringBuilder;

    private void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(this);
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        emotionScore = 50f; // Start at 50 as baseline

        _stringBuilder = new StringBuilder();
        _stringBuilder.Append("Game Time,Emotion Input Type,Emotion Score Delta,Emotion Score Before,Emotion Score After");
    }

    private void OnDestroy()
    {
        ExportEmotionLog();
    }

    private void Update()
    {
        emotionScore = Mathf.Clamp(emotionScore, 0f, 100f);
        EmotionRegen();
    }

    public void EmotionInput(EmotionInputType inputType, Object data)
    {
        float emotionScoreBefore = emotionScore;

        switch (inputType)
        {
            case EmotionInputType.PlayerHitByBullet:
                float bulletEmotionCoefficient = 5f;
                Bullet bullet = (Bullet)data;
                if (NegativeInputChain())
                {
                    emotionScore += bulletEmotionCoefficient * bullet.bulletDamage * _negativeInputChainCount * _negativeInputChainCoefficient;
                }
                else
                {
                    emotionScore += bulletEmotionCoefficient * bullet.bulletDamage;
                }
                break;

            case EmotionInputType.PlayerHitByCollision:
                float collisionEmotionCoefficient = 7.5f;
                EnemyBase enemy = (EnemyBase)data;
                if (NegativeInputChain())
                {
                    emotionScore += collisionEmotionCoefficient * enemy.collisionDamage * _negativeInputChainCount * _negativeInputChainCoefficient;
                }
                else
                {
                    emotionScore += collisionEmotionCoefficient * enemy.collisionDamage;
                }
                break;

            case EmotionInputType.PlayerHitByBoss:
                break;

            case EmotionInputType.PlayerFailedDodge:
                if (NegativeInputChain())
                {
                    emotionScore += 10f * _negativeInputChainCount * _negativeInputChainCoefficient;
                }
                else
                {
                    emotionScore += 10f;
                }
                break;

            case EmotionInputType.PlayerSuccessfulDodge:
                emotionScore -= 10f;
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
}
