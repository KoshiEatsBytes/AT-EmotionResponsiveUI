using UnityEngine;

public enum EmotionInputType
{
    PlayerHitByBullet,
    PlayerHitByCollision,
    PlayerHitByBoss,
    PlayerDodged,
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
    }

    private void Update()
    {
        EmotionRegen();
    }

    public void EmotionInput(EmotionInputType inputType, Object data)
    {
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

            case EmotionInputType.PlayerDodged:
                break;

            case EmotionInputType.PlayerSuccessfulDodge:
                emotionScore -= 10f;
                break;
        }
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
}
