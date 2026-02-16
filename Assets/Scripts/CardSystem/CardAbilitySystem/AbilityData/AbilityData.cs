using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardSystem
{
    // Data container class mostly used to pass data between strategies
    [System.Serializable]
    public class AbilityData
    {
        private Unit _unit;
        private IEnumerable<GameObject> _targets;
        private Guid _guid;

        public Unit GetUnit { get { return _unit; } }
        public IEnumerable<GameObject> Targets { get { return _targets; } set { _targets = value; } }
        public Guid GetGUID => _guid;

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

        public AbilityData(Unit unit, Guid guid)
        {
            _unit = unit;
            _guid = guid;

            //Debug.Log($"guid: {_guid}");
        }

        // Adjust to keep list of active coroutines for easy stopping?
        // move to unit?
        public void StartCoroutine(IEnumerator coroutine)
        {
            _unit?.StartCoroutine(coroutine);
        }
    }
}