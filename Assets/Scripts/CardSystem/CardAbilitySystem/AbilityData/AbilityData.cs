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
        private Vector2Int _abilityTriggerPos;
        private CardRarity _rarity;

        public Unit GetUnit { get { return _unit; } }
        public IEnumerable<GameObject> Targets { get { return _targets; } set { _targets = value; } }
        public Guid GetGUID => _guid;
        public Vector2Int AbilityTriggerPos { get { return _abilityTriggerPos; } set { _abilityTriggerPos = value; } }
        public CardRarity GetCardRarity => _rarity;

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

        public AbilityData(Unit unit, Guid guid, Vector2Int abilityTriggerPos, CardRarity rarity)
        {
            _unit = unit;
            _guid = guid;
            _abilityTriggerPos = abilityTriggerPos;
            _rarity = rarity;
        }
        public AbilityData(AbilityData refAbilityData)
        {
            _unit = refAbilityData.GetUnit;
            _guid = refAbilityData.GetGUID;
            _abilityTriggerPos = refAbilityData.AbilityTriggerPos;
            _rarity = refAbilityData.GetCardRarity;
        }

        // Adjust to keep list of active coroutines for easy stopping?
        // move to unit?
        public void StartCoroutine(IEnumerator coroutine)
        {
            _unit?.StartCoroutine(coroutine);
        }
    }
}