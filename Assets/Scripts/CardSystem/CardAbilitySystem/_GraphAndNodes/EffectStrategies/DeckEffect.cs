using System;
using UnityEngine;

namespace CardSystem
{
    // Concrete misc effect class. Currently unimplemented
    [CreateNodeMenu("Misc Effects/Deck Effect")]
    public class DeckEffect : MiscEffect
    {
        public enum DeckAction
        {
            Draw,       // Draw N cards into the player's hand
            PeekTop,    // Look at (return) the top N card definitions without drawing
            RevealTop,   // Reveal the top N cards (non-destructive; can be used to show UI)
            AddToDeck,   //  Add specified cards to the deck
        }

        [SerializeField] private DeckAction _action;

        [SerializeField] private int _amount = 1;
        [SerializeField] private CardAbilityDefinition _cardToAdd;

        // Optional: whether to log results to console (useful for initial testing)
        [SerializeField] private bool _logResults = true;

        public override void StartEffect(AbilityData abilityData, Action onFinished)
        {
            if (abilityData.GetUnit.GetTeam != Team.Friendly) return;

            base.StartEffect(abilityData, onFinished);

            // Defensive checks
            if (DeckAndHandManager.instance == null)
            {
                Debug.LogWarning("[DeckEffect] CardManager.instance is null - cannot interact with deck.");
                onFinished?.Invoke();
                return;
            }

            switch (_action)
            {
                case DeckAction.Draw:
                    // Draw _amount cards (CardManager.DrawMultiple handles hand size / deck end)
                    DeckAndHandManager.instance.DrawCard(_amount);
                    break;

                case DeckAction.PeekTop:
                    // Get top definitions without changing deck state
                    var topDefs = DeckAndHandManager.instance.PeekTopDefinitions(_amount);
                    if (_logResults)
                    {
                        for (int i = 0; i < topDefs.Length; i++)
                        {
                            Debug.Log($"[DeckEffect] PeekTop [{i}] : {topDefs[i].GetCardName}");
                        }
                    }
                    // We could pass these definitions into UI code or AbilityData if needed.
                    break;

                case DeckAction.RevealTop:
                    var revealDefs = DeckAndHandManager.instance.PeekTopDefinitions(_amount);
                    // Reveal semantics are UI/game-specific. Here we just log and leave hooks.
                    if (_logResults)
                    {
                        for (int i = 0; i < revealDefs.Length; i++)
                            Debug.Log($"[DeckEffect] RevealTop [{i}] : {revealDefs[i].GetCardName}");
                    }
                    // If you want to spawn temporary reveal prefabs or UI, do it here or call into a UI manager.
                    break;

                case DeckAction.AddToDeck:
                    if (_cardToAdd != null)
                    {
                        DeckAndHandManager.instance?.AddDefinitionToRuntimeDeck(_cardToAdd);

                        // Optional: draw 1 immediately to verify presence during testing
                        if (_logResults)
                        {
                            Debug.Log($"[DeckEffect] Requested add: {_cardToAdd.GetCardName}");
                            DeckAndHandManager.instance.LogRuntimeDeck("[DeckEffect] Post-add runtime deck:");
                        }
                    }
                    break;
            }

            // This effect is instant. If you start coroutines or animations, call onFinished after they complete.
            onFinished?.Invoke();
        }
    }
}