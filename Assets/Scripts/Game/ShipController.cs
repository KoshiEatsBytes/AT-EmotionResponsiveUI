using UnityEngine;

public class ShipController : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    private Vector2 _moveDir;
    private static readonly Vector2 _screenBounds = new Vector2(800, 1000f);

    private InputSystem_Actions _playerInputs;

    private void Awake()
    {
        _playerInputs = new InputSystem_Actions();
        _playerInputs.Enable();
    }

    private void Update()
    {
        HandleInputs();
        HandleMovement();
    }

    private void HandleInputs()
    {
        _moveDir = _playerInputs.Player.Move.ReadValue<Vector2>();
    }

    private void HandleMovement()
    {
        Vector2 targetPos = (Vector2)transform.localPosition + _moveDir * moveSpeed * Time.deltaTime;
        targetPos.x = Mathf.Clamp(targetPos.x, -_screenBounds.x / 2, _screenBounds.x / 2);
        targetPos.y = Mathf.Clamp(targetPos.y, -_screenBounds.y / 2, _screenBounds.y / 2);

        transform.localPosition = targetPos;
    }
}
