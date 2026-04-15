using UnityEngine;

public class DevCheatMenu : MonoBehaviour
{
    private bool _open;
    private int _clickCount;
    private float _lastClickTime;
    private string _moneyInput;
    private string _hpInput;
    private string _apInput;
    private Rect _windowRect = new Rect(20, 20, 280, 320);


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            _open = !_open;
        }
    }

    private void OnGUI()
    {
        if (!_open) return;
        _windowRect = GUI.Window(12345, _windowRect, DrawWindow, "CHEATS");
    }

    private void DrawWindow(int id)
    {
        if (GUILayout.Button("Win level"))
        {
            KillAllEnemies();
        }
        GUILayout.Space(6);

        GUILayout.Label("Add Money");
        _moneyInput = GUILayout.TextField(_moneyInput);
        if (GUILayout.Button("Apply Money"))
        {
            if (int.TryParse(_moneyInput, out int amount))
            {
                AddMoney(amount);
            }
        }
        GUILayout.Space(6);

        GUILayout.Label("HP to all friendly units");
        _hpInput = GUILayout.TextField(_hpInput);
        if (GUILayout.Button("Apply HP"))
        {
            if (int.TryParse(_hpInput, out int delta))
            {
                AddHpToFriendlies(delta);
            }
        }
        
        GUILayout.Label("AP to all friendlies");
        _apInput = GUILayout.TextField(_apInput);
        if (GUILayout.Button("Apply AP"))
        {
            if (int.TryParse(_apInput, out int delta))
            {
                AddApToFriendlies(delta);
            }
        }
        GUILayout.Space(6);

        if (GUILayout.Button("Close"))
        {
            _open = false;
        }
        GUI.DragWindow();
    }

    private void KillAllEnemies()
    {
        var units = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        for (int i = 0; i < units.Length; i++)
        {
            Unit unit = units[i];
            if (unit != null && unit.GetTeam == Team.Enemy)
            {
                unit.ChangeHealth(unit.GetMaxHealth + 9999, false);
            }
        }
    }

    private void AddMoney(int amount)
    {
        if (CurrencyManager.Instance == null) return;
        {
            CurrencyManager.Instance.Add(amount);
        }
    }

    private void AddHpToFriendlies(int delta)
    {
        if (delta == 0) return;

        var units = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        for (int i = 0; i < units.Length; i++)
        {
            Unit u = units[i];
            if (u == null || u.GetTeam != Team.Friendly) continue;

            if (delta > 0)
                u.ChangeHealth(delta, true);
            else
                u.ChangeHealth(-delta, false);
        }

        // Force the health UI to refresh from the current unit
        Unit cur = TurnManager.GetCurrentUnit;
        if (cur != null && cur.GetTeam == Team.Friendly)
            DamageEvents.RaisePlayerDamaged(cur.GetHealth, cur.GetMaxHealth);
    }

    private void AddApToFriendlies(int delta)
    {
        if (delta == 0) return;

        var units = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        for (int i = 0; i < units.Length; i++)
        {
            Unit unit = units[i];
            if (unit != null && unit.GetTeam == Team.Friendly)
                unit.AddAP(delta);
        }

        // Force the AP text to refresh
        if (GameUIManager.instance != null)
            GameUIManager.instance.UpdateApText();
    }
}
