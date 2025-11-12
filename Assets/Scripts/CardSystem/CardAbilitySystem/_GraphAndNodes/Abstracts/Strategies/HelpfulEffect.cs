
using System;

namespace CardSystem
{
    // Abstract subclass of effect strategy. Created to allow different effect types to have different output port value types
    public abstract class HelpfulEffect : EffectStrategy
    {
        [Input(connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public bool input;

        //public override abstract void StartEffect(AbilityData abilityData, Action onFinished);
    }
}