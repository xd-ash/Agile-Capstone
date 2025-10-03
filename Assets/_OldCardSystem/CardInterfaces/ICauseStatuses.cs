using UnityEngine;

namespace OldCardSystem
{
    public interface ICauseStatuses
    {
        public enum CardStatusTypes
        {
            None,
            Bleed,
            Burn,
            Slow
        }
        public CardStatusTypes[] StatusTypes { get; }
        public int StatusDuration { get; }

        public abstract void ApplyStatus();
    }
}
