using System;

namespace CardSystem
{
    // Concrete misc effect class. Currently unimplemented
    [CreateNodeMenu("Misc Effects/Deck Effect")]
    public class DeckEffect : MiscEffect
    {
        public override void StartEffect(AbilityData abilityData, Action onFinished)
        {
            throw new NotImplementedException();
        }
    }
}