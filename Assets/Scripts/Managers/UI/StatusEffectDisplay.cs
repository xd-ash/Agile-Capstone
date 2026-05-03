using System.Collections.Generic;
using UnityEngine;

// [STATUS_UI] Spawns and manages status effect icons for a unit
public class StatusEffectDisplay : MonoBehaviour
{
    [SerializeField] private Transform _iconContainer;         // [STATUS_UI] parent for icon instances (set in inspector or found at runtime)
    [SerializeField] private StatusIconEntry _iconPrefab;      // [STATUS_UI] prefab with StatusIconEntry component
    [SerializeField] private bool _usePlayerUI = false;        // [STATUS_UI] if true, grabs container from GameUIManager instead

    private ActiveEffectsTracker _tracker;
    private Dictionary<string, StatusIconEntry> _activeIcons = new();

    private void Awake()
    {
        _tracker = GetComponent<ActiveEffectsTracker>();
    }

    private void OnEnable()
    {
        if (_tracker == null) return;
        _tracker.OnEffectsChanged += RebuildIcons;
    }

    private void OnDisable()
    {
        if (_tracker == null) return;
        _tracker.OnEffectsChanged -= RebuildIcons;
    }

    private void Start()
    {
        // [STATUS_UI] Player grabs the screen-space container from GameUIManager
        if (_usePlayerUI && GameUIManager.instance != null)
            _iconContainer = GameUIManager.instance.GetStatusIconContainer;
    }

    private void RebuildIcons()
    {
        if (_iconContainer == null || _iconPrefab == null) return;

        var currentEffects = _tracker.GetActiveEffectInfos();

        // [STATUS_UI] Build a set of current effect names to detect removals
        HashSet<string> currentNames = new();
        foreach (var info in currentEffects)
        {
            currentNames.Add(info.name);

            if (_activeIcons.TryGetValue(info.name, out StatusIconEntry existing))
            {
                // [STATUS_UI] Already showing this effect, just update turn count
                existing.UpdateTurns(info.turnsRemaining);
            }
            else if (info.icon != null)
            {
                // [STATUS_UI] New effect, spawn an icon entry
                StatusIconEntry entry = Instantiate(_iconPrefab, _iconContainer);
                entry.Setup(info.icon, info.turnsRemaining);
                _activeIcons[info.name] = entry;
            }
        }

        // [STATUS_UI] Remove icons for effects that are no longer active
        List<string> toRemove = new();
        foreach (var kvp in _activeIcons)
        {
            if (currentNames.Contains(kvp.Key)) continue;
            if (kvp.Value != null)
                Destroy(kvp.Value.gameObject);
            toRemove.Add(kvp.Key);
        }
        foreach (var key in toRemove)
            _activeIcons.Remove(key);
    }

    private void OnDestroy()
    {
        // [STATUS_UI] Clean up spawned icons when unit dies
        foreach (var kvp in _activeIcons)
        {
            if (kvp.Value != null)
                Destroy(kvp.Value.gameObject);
        }
        _activeIcons.Clear();
    }
}