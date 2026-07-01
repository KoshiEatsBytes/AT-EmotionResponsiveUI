using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Slider stressSlider;
    [SerializeField] private Slider staticHealthSlider;
    [SerializeField] private Slider shipHealthSlider;
    [SerializeField] private Vector3 shipHealthOffset;

    private ShipController _shipController;
    private EmotionResponseManager _emotionResponseManager;

    private float _displayShipHealthTimer;
    private int _lastPlayerHealth;

    private void Start()
    {
        _shipController = FindAnyObjectByType<ShipController>();
        _emotionResponseManager = EmotionResponseManager.Instance;
        _lastPlayerHealth = _shipController.health;
    }

    private void FixedUpdate()
    {
        UpdateHealthBar();
        UpdateStressBar();
    }

    private void UpdateHealthBar()
    {
        staticHealthSlider.value = _shipController.health / 10f;
        shipHealthSlider.value = _shipController.health / 10f;
        shipHealthSlider.transform.localPosition = _shipController.transform.localPosition + shipHealthOffset;

        bool highStress = false;
        bool hpChanged = false;

        // Always display health on ship during high stress
        if (_emotionResponseManager.emotionScore >= 80f || _shipController.health <= 3)
        {
            highStress = true;
        }

        // Display health on ship for duration after health changed
        if (_lastPlayerHealth != _shipController.health)
        {
            _lastPlayerHealth = _shipController.health;
            _displayShipHealthTimer = 1.5f;
        }

        if (_displayShipHealthTimer > 0)
        {
            _displayShipHealthTimer -= Time.deltaTime;
            hpChanged = true;
        }

        if (highStress || hpChanged)
        {
            shipHealthSlider.GetComponent<CanvasGroup>().alpha = 1f;
        }
        else
        {
            shipHealthSlider.GetComponent<CanvasGroup>().alpha = 0f;
        }
    }

    private void UpdateStressBar()
    {
        stressSlider.value = _emotionResponseManager.emotionScore / 100f;
    }
}
