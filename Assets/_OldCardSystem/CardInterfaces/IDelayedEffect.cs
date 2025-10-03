using UnityEngine;

namespace OldCardSystem
{
    public interface IDelayedEffect
    {
        public int DelayDuration { get; }

        public abstract void BeginDelay();
        public abstract void DelayEnd();
    }
}
