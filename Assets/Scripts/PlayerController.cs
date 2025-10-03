using Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Camera _cam;
    [SerializeField] private LayerMask enemyMask;

    [Header("Damage")]
    [SerializeField] private int clickDamage = 2;
    
    [Header("AP")]
    [SerializeField] private int clickCost = 1;

    [SerializeField] private Unit _playerUnit;
    private Vector2 _screenPos;
    

    private void Awake()
    {
        if (_cam == null) _cam = Camera.main;
    }

    // Input System: Point action → Performed → this.OnPoint
    public void OnPoint(InputAction.CallbackContext context)
    {
        _screenPos = context.ReadValue<Vector2>();
    }

    // Input System: Click action → Performed → this.OnClick
    public void OnClick(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (!TurnManager.IsPlayerTurn) return;
        if (_playerUnit != null && !_playerUnit.CanSpend(clickCost)) return;

        Vector3 world = _cam.ScreenToWorldPoint(_screenPos);
        Vector2 worldPoint = new Vector2(world.x, world.y);

        // Only damage objects with IDamageable and on the enemy mask layer
        Collider2D hit = Physics2D.OverlapPoint(worldPoint, enemyMask);
        if (hit == null) return;

        IDamagable dmg = hit.GetComponent<IDamagable>();
        if (dmg != null)
        {
            Debug.Log("bang");
            dmg.TakeDamage(clickDamage);
            
            if (_playerUnit != null) _playerUnit.SpendAP(clickCost);
            TurnManager.instance.UpdateApText();
            // could add a bool in the settings for auto end turn
            // TurnManager.instance.EndPlayerTurn();
        }
    }
}