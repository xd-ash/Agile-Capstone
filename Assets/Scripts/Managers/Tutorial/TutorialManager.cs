using System;
using UnityEngine;
using CardSystem;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [SerializeField] private TutorialUI _tutorialUI;
    public static TutorialInputMode CurrentInputMode { get; private set; } = TutorialInputMode.None;

    private int _currentStep = 0;
    private bool _stepWaiting = false;
    private CardCategory _expectedCategory;

    public CardCategory GetExpectedCatagory => _expectedCategory;

    [SerializeField] private Unit[] _tutorialUnits;
    [SerializeField] private UnitMovementController _playerMoveController;

    public enum TutorialInputMode
    {
        None,
        MoveOnly,
        CardsOnly,
        MoveAndCards,
        EndTurnOnly
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

        if (TransitionScene.IsTutorial)
            TurnManager.OnGameStart += LateStart;
        else
        {
            CurrentInputMode = TutorialInputMode.None;
            gameObject.SetActive(false);
            return;
        }

        PauseMenu.isPaused = false;
        var pauseMenu = GameObject.Find("PauseMenu");
        if (pauseMenu != null) pauseMenu.SetActive(false);
    }
    private void Start()
    {
        AdvanceStep();

        _tutorialUnits = TurnManager.GetUnitTurnOrder.ToArray() ?? new Unit[0];
        foreach (var unit in _tutorialUnits)
        {
            if (unit == null || unit.GetTeam == Team.Enemy) continue;
            _playerMoveController = unit.GetComponent<UnitMovementController>();
        }
    }

    private void LateStart()
    {
        if (!TransitionScene.IsTutorial)
        {
            TurnManager.OnGameStart -= LateStart;
            return;
        }

        _tutorialUnits = TurnManager.GetUnitTurnOrder.ToArray() ?? new Unit[0];
        foreach (var unit in _tutorialUnits)
        {
            if (unit == null || unit.GetTeam == Team.Enemy) continue;
            _playerMoveController = unit.GetComponent<UnitMovementController>();
        }

        if (_playerMoveController != null)
            _playerMoveController.onComplete += RestorePlayerAP;
    }

    private void RestorePlayerAP() => RestoreAP(Team.Friendly);
    private void RestorePlayerHP() => RestoreHP(Team.Friendly);
    private void RestoreEnemytAP() => RestoreAP(Team.Enemy);
    private void RestoreEnemyAP() => RestoreAP(Team.Enemy);

    private void RestoreAP(Team unitTeam)
    {
        foreach (var unit in _tutorialUnits)
        {
            if (unit == null || unit.GetTeam != unitTeam) continue;
            int diff = unit.GetMaxAP - unit.GetAP;
            unit.RestoreAP(diff);
            unit.GetFloatingText.SpawnFloatingText($"+{diff} AP", TextPresetType.RestoreAP);
        }
    }

    private void RestoreHP(Team unitTeam)
    {
        foreach (var unit in _tutorialUnits)
        {
            if (unit == null || unit.GetTeam != unitTeam) continue;
            int diff = unit.GetMaxHealth - unit.GetHealth;
            unit.ChangeHealth(diff, true);
            unit.GetFloatingText.SpawnFloatingText($"+{diff} HP", TextPresetType.HealPreset);
        }
    }

    private void AdvanceStep()
    {
        //CleanUpCardState();
        _stepWaiting = false;
        _currentStep++;

        switch (_currentStep)
        {
            // --- Round 1: Move + Ranged ---
            case 1:
                CurrentInputMode = TutorialInputMode.MoveOnly;
                _tutorialUI.Show("Click a tile to move your unit.");
                ByteMapController.TileEntered += OnTileEntered;
                break;

            case 2:
                CurrentInputMode = TutorialInputMode.MoveAndCards;
                _tutorialUI.Show("Select a ranged card and attack the enemy.");
                _expectedCategory = CardCategory.Ranged;
                AbilityEvents.OnAbilityUsedDetailed += OnAbilityUsedExpecting;
                break;

            case 3:
                CurrentInputMode = TutorialInputMode.EndTurnOnly;
                _tutorialUI.Show("End your turn.");
                TurnManager.Instance.OnTurnEnd += OnFriendlyTurnEnd;
                break;

            case 4:
                CurrentInputMode = TutorialInputMode.None;
                _tutorialUI.Show("The enemy takes their turn. Watch out!");
                TurnManager.Instance.OnTurnEnd += OnEnemyTurnEnd;
                break;

            // --- Round 2: Melee ---
            case 5:
                CurrentInputMode = TutorialInputMode.MoveAndCards;
                _tutorialUI.Show("Now use a melee card on the enemy.");
                _expectedCategory = CardCategory.Melee;
                AbilityEvents.OnAbilityUsedDetailed += OnAbilityUsedExpecting;
                break;

            case 6:
                CurrentInputMode = TutorialInputMode.EndTurnOnly;
                _tutorialUI.Show("End your turn.");
                TurnManager.Instance.OnTurnEnd += OnFriendlyTurnEnd;
                break;

            case 7:
                CurrentInputMode = TutorialInputMode.None;
                _tutorialUI.Show("The enemy takes their turn.");
                TurnManager.Instance.OnTurnEnd += OnEnemyTurnEnd;
                break;

            // --- Round 3: Heal & Shield ---
            case 8:
                CurrentInputMode = TutorialInputMode.MoveAndCards;
                _tutorialUI.Show("Heal yourself with a heal card.");
                _expectedCategory = CardCategory.Heal;
                AbilityEvents.OnAbilityUsedDetailed += OnAbilityUsedExpecting;
                break;
            case 9:
                CurrentInputMode = TutorialInputMode.MoveAndCards;
                _tutorialUI.Show("Use a shield card to protect yourself.");
                _expectedCategory = CardCategory.Shield;
                AbilityEvents.OnAbilityUsedDetailed += OnAbilityUsedExpecting;
                break;

            case 10:
                CurrentInputMode = TutorialInputMode.EndTurnOnly;
                _tutorialUI.Show("End your turn.");
                TurnManager.Instance.OnTurnEnd += OnFriendlyTurnEnd;
                break;

            case 11:
                CurrentInputMode = TutorialInputMode.None;
                _tutorialUI.Show("The enemy takes their turn.");
                TurnManager.Instance.OnTurnEnd += OnEnemyTurnEnd;
                break;

            /*/ --- Round 4: Shield ---
            case 11:
                CurrentInputMode = TutorialInputMode.MoveAndCards;
                _tutorialUI.Show("Use a shield card to protect yourself.");
                _expectedCategory = CardCategory.Shield;
                AbilityEvents.OnAbilityUsedDetailed += OnAbilityUsedExpecting;
                break;
            case 12:
                CurrentInputMode = TutorialInputMode.EndTurnOnly;
                _tutorialUI.Show("End your turn.");
                TurnManager.Instance.OnTurnEnd += OnFriendlyTurnEnd;
                break;

            case 13:
                CurrentInputMode = TutorialInputMode.None;
                _tutorialUI.Show("The enemy takes their turn.");
                TurnManager.Instance.OnTurnEnd += OnEnemyTurnEnd;
                break;
                        */

            // --- Free play: finish the enemy ---
            case 12:
                CurrentInputMode = TutorialInputMode.None;
                _tutorialUI.Show("Now finish off the enemy!", false);
                GameOverEvents.OnGameOver += OnGameOver;

                if (_playerMoveController != null)
                    _playerMoveController.onComplete -= RestorePlayerAP;
                break;

            // --- Done ---
            case 13:
                CurrentInputMode = TutorialInputMode.None;
                _tutorialUI.Show("Tutorial complete!", false);
                //TransitionScene.ResetTutorialFlag();
                Invoke(nameof(ReturnToMainMenu), 1.5f);
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

    private void OnAbilityUsedExpecting(Team team, CardCategory category)
    {
        if (!_stepWaiting || team != Team.Friendly) return;
        if (category != _expectedCategory) return;

        AbilityEvents.OnAbilityUsedDetailed -= OnAbilityUsedExpecting;
        AdvanceStep();
    }

    private void OnFriendlyTurnEnd(Unit unit)
    {
        if (!_stepWaiting || unit.GetTeam != Team.Friendly) return;

        TurnManager.Instance.OnTurnEnd -= OnFriendlyTurnEnd;
        AdvanceStep();
    }

    private void OnEnemyTurnEnd(Unit unit)
    {
        if (!_stepWaiting || unit.GetTeam != Team.Enemy) return;

        TurnManager.Instance.OnTurnEnd -= OnEnemyTurnEnd;
        AdvanceStep();
    }

    private void OnGameOver(bool didWin)
    {
        if (!_stepWaiting) return;

        GameOverEvents.OnGameOver -= OnGameOver;

        if (didWin)
            AdvanceStep();
    }

    private void ReturnToMainMenu()
    {
        TransitionScene.Instance.StartTransition("MainMenu");
    }

    private void OnDestroy()
    {
        ByteMapController.TileEntered -= OnTileEntered;
        AbilityEvents.OnAbilityUsedDetailed -= OnAbilityUsedExpecting;
        GameOverEvents.OnGameOver -= OnGameOver;

        if (TurnManager.Instance != null)
        {
            TurnManager.OnGameStart -= LateStart;

            TurnManager.Instance.OnTurnEnd -= OnFriendlyTurnEnd;
            TurnManager.Instance.OnTurnEnd -= OnEnemyTurnEnd;
        }

        if (_playerMoveController != null)
            _playerMoveController.onComplete -= RestorePlayerAP;
    }

    private void CleanUpCardState()
    {
        if (DeckAndHandManager.Instance == null) return;

        var selectedCard = DeckAndHandManager.Instance.GetSelectedCard;
        if (selectedCard != null && selectedCard.GetCardTransform != null)
        {
            var cardSelect = selectedCard.GetCardTransform.GetComponent<CardSelect>();
            cardSelect?.ReturnCardToHand();
        }

        if (AbilityEvents.IsTargeting)
            AbilityEvents.TargetingStopped();

        CardSplineManager.Instance?.ArrangeCardGOs();
    }
}