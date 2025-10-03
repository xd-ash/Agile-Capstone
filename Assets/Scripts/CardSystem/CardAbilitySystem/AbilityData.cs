using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardSystem
{
    public class AbilityData
    {
        private UnitStatsScript _unit; //TEMP script ref, replace with real one

        private IEnumerable<GameObject> _targets;

        public UnitStatsScript GetUnit { get { return _unit; } }
        public IEnumerable<GameObject> Targets { get { return _targets; } set { _targets = value; } }
        public AbilityData(UnitStatsScript unit)
        {
            _unit = unit;
        }

        public void StartCoroutine(IEnumerator coroutine)
        {
            _unit?.StartCoroutine(coroutine);
        }
    }
}