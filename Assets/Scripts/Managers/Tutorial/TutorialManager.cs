using System;
using UnityEngine;
using CardSystem;

// Drives the tutorial step-by-step using events already present in the codebase.
// Attach to a GameObject in TutorialScene alongside a TutorialUI component.
public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [SerializeField] private TutorialUI _tutorialUI;
    public static TutorialInputMode CurrentInputMode { get; private set; } = TutorialInputMode.None;

    private int _currentStep = 0;
    private bool _stepWaiting = false; // guard to avoid advancing multiple times per step
    
    public enum TutorialInputMode
    {
        None,         // tutorial inactive, allow everything
        MoveOnly,     // step 1
        NoInput,      // step 2 (just a message, auto-advances)
        CardsOnly,    // step 3
        EndTurnOnly   // step 4
    }
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        if (!TransitionScene.IsTutorial)
        {
            gameObject.SetActive(false);
            return;
        }

        // Force pause menu off on tutorial load
        PauseMenu.isPaused = false;
        var pauseMenu = GameObject.Find("PauseMenu");
        if (pauseMenu != null) pauseMenu.SetActive(false);

        TransitionScene.ResetTutorialFlag();
    }

    private void Start()
    {
        AdvanceStep();
    }

    private void AdvanceStep()
    {
        _stepWaiting = false;
        _currentStep++;

        switch (_currentStep)
        {
            case 1:
                CurrentInputMode = TutorialInputMode.MoveOnly;
                _tutorialUI.Show("Click a tile to move.");
                ByteMapController.TileEntered += OnTileEntered;
                break;

            case 2:
                CurrentInputMode = TutorialInputMode.CardsOnly;
                _tutorialUI.Show("Play a card to attack.");
                AbilityEvents.OnAbilityUsed += OnAbilityUsed;
                break;

            case 3:
                CurrentInputMode = TutorialInputMode.EndTurnOnly;
                _tutorialUI.Show("End your turn.");
                TurnManager.Instance.OnTurnEnd += OnTurnEnd;
                break;

            case 4:
                CurrentInputMode = TutorialInputMode.None;
                _tutorialUI.Show("The enemy takes their turn. Watch out!");
                break;

            default:
                CurrentInputMode = TutorialInputMode.None;
                _tutorialUI.Hide();
                break;
        }

        _stepWaiting = true;
    }

    // --- Event Handlers ---

    private void OnTileEntered(Vector2Int tilePos, Unit unit)
    {
        if (!_stepWaiting || unit.GetTeam != Team.Friendly) return;

        ByteMapController.TileEntered -= OnTileEntered;
        AdvanceStep();
    }

    private void SubscribeToFriendlyAPChange()
    {
        // Find the friendly unit in scene and subscribe to its AP changed event
        foreach (Unit unit in FindObjectsByType<Unit>(FindObjectsSortMode.None))
        {
            if (unit.GetTeam == Team.Friendly)
            {
                unit.OnApChanged += OnFriendlyAPChanged;
                break;
            }
        }
    }

    private void OnFriendlyAPChanged(Unit unit)
    {
        if (!_stepWaiting) return;

        unit.OnApChanged -= OnFriendlyAPChanged;
        AdvanceStep();
    }

    private void OnAbilityUsed(Team team)
    {
        if (!_stepWaiting || team != Team.Friendly) return;

        AbilityEvents.OnAbilityUsed -= OnAbilityUsed;
        AdvanceStep();
    }

    private void OnTurnEnd(Unit unit)
    {
        if (!_stepWaiting || unit.GetTeam != Team.Friendly) return;

        TurnManager.Instance.OnTurnEnd -= OnTurnEnd;
        AdvanceStep();
    }

    private void OnDestroy()
    {
        // Safety cleanup in case TutorialManager is destroyed mid-tutorial
        ByteMapController.TileEntered -= OnTileEntered;
        AbilityEvents.OnAbilityUsed -= OnAbilityUsed;

        if (TurnManager.Instance != null)
            TurnManager.Instance.OnTurnEnd -= OnTurnEnd;
    }
}