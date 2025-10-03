using UnityEngine;

namespace OldCardSystem
{
    public interface IUtility
    {
        public enum CardUtilityTypes
        {
            None,
            CardReturn,
            APRestore,
            Heal,
            Buff
        }
        public CardUtilityTypes[] UtilityTypes { get; }
        public bool HasMultipleUtilities { get; }

        //Multiple value vars in case some cards do many effects
        public int CardReturnValue { get; }
        public int APRestoreValue { get; }
        public int HealValue { get; }
        public int BuffValue { get; }

        public abstract void CauseEffect(); //maybe break into multiple functions?
    }
}
