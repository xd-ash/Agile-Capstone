using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardSystem
{
    public class AbilityData
    {
        private Unit _unit; //TEMP script ref, replace with real one

        private IEnumerable<GameObject> _targets;

        public Unit GetUnit { get { return _unit; } }
        public IEnumerable<GameObject> Targets { get { return _targets; } set { _targets = value; } }
        public int GetTargetCount 
        {
            get
            {
                int targetCount = 0;
                if (_targets != null)
                    foreach (GameObject target in _targets)
                        if (target != null)
                            targetCount++;
                return targetCount;
            }
        }

        public AbilityData(Unit unit)
        {
            _unit = unit;
        }

        // Adjust to keep list of active coroutines for easy stopping?
        // move to unit?
        public void StartCoroutine(IEnumerator coroutine)
        {
            _unit?.StartCoroutine(coroutine);
        }
    }
}