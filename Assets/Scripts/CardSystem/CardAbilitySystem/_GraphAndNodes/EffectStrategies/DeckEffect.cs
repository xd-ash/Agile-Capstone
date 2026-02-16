using System;
using UnityEngine;

namespace CardSystem
{
    // Concrete misc effect class. Currently unimplemented
    [CreateNodeMenu("Misc Effects/Deck Effect")]
    public class DeckEffect : EffectStrategy, IUseEffectValue
    {
        public enum DeckAction
        {
            Draw,       // Draw N cards into the player's hand
            PeekTop,    // Look at (return) the top N card definitions without drawing
            RevealTop   // Reveal the top N cards (non-destructive; can be used to show UI)
        }

        [SerializeField] private DeckAction _action = DeckAction.Draw;
        //[SerializeField] private int _amount = 1;

        // Optional: whether to log results to console (useful for initial testing)
        [SerializeField] private bool _logResults = true;

        public override void StartEffect(AbilityData abilityData, Action onFinished, int effectValueChange = 0)
        {
            if (abilityData.GetUnit.GetTeam != Team.Friendly) return;

            base.StartEffect(abilityData, onFinished, effectValueChange);

            // Defensive checks
            if (DeckAndHandManager.Instance == null)
            {
                Debug.LogWarning("[DeckEffect] CardManager.instance is null - cannot interact with deck.");
                _onFinished?.Invoke();
                return;
            }

            switch (_action)
            {
                case DeckAction.Draw:
                    DeckAndHandManager.Instance.DrawCard(effectValue);
                    break;
                case DeckAction.PeekTop:
                    // Get top definitions without changing deck state
                    var topDefs = DeckAndHandManager.Instance.PeekTopDefinitions(effectValue);
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
                    var revealDefs = DeckAndHandManager.Instance.PeekTopDefinitions(effectValue);
                    // Reveal semantics are UI/game-specific. Here we just log and leave hooks.
                    if (_logResults)
                    {
                        for (int i = 0; i < revealDefs.Length; i++)
                            Debug.Log($"[DeckEffect] RevealTop [{i}] : {revealDefs[i].GetCardName}");
                    }
                    // If you want to spawn temporary reveal prefabs or UI, do it here or call into a UI manager.
                    break;
            }

            // This effect is instant. If you start coroutines or animations, call onFinished after they complete.
            _onFinished?.Invoke();
        }
    }
}