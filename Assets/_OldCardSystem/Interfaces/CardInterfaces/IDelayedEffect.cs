using UnityEngine;

namespace CardSystem
{
    public interface IDelayedEffect
    {
        public int DelayDuration { get; }

        public abstract void BeginDelay();
        public abstract void DelayEnd();
    }
}
