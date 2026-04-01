using System;
using UnityEngine;

public class CardStateManager : MonoBehaviour
{
    public enum CardState { PackViewer, DeckViewer, Shop, Rewards, Combat }
    private CardState _currCardState = CardState.PackViewer;

    public CardState GetCurrCardState => _currCardState;

    public static Action<CardState> SwapCardStates;

    public static CardStateManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SwapCardStates += SwapState;
    }
    private void OnDestroy()
    {
        SwapCardStates -= SwapState;
    }

    private void SwapState(CardState state)
    {
        if (_currCardState == state) return;
        _currCardState = state;
    }
}
