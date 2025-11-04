using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardSystem
{
    // Data container class mostly used to pass data between strategies
    public class AbilityData
    {
        private Unit _unit;
        private IEnumerable<GameObject> _targets;

        public Unit GetUnit { get { return _unit; } }
        public IEnumerable<GameObject> Targets { get { return _targets; } set { _targets = value; } }
        public AbilityData(Unit unit)
        {
            _unit = unit;
        }

        public void StartCoroutine(IEnumerator coroutine)
        {
            _unit?.StartCoroutine(coroutine);
        }
    }
}