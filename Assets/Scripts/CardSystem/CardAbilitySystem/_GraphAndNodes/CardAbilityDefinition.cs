using UnityEngine;
using XNode;
using System;
using System.Linq;
using System.Collections.Generic;

namespace CardSystem
{
    [CreateAssetMenu(fileName = "NewCardAbility", menuName = "Card System/New Card Ability")]
    public class CardAbilityDefinition : NodeGraph
    {
        [Header("Card Info")]
        [TextArea(1, 3)]
        [SerializeField] private string _description;
        [SerializeField] private AudioClip _abilitySFX;

        [Header("Card Data")]
        [SerializeField] private int _apCost;
        [SerializeField] private int _range;
        [SerializeField] private int _shopCost;
        [SerializeField] private int _shopWeight;

        [Header("Combat Balance")]
        [SerializeField, Range(0, 100)] private int _baseHitChance = 80;
        [SerializeField, Range(0, 100)] private int _minHitChance = 10;
        [SerializeField, Range(0, 100)] private int _maxHitChance = 95;

        [SerializeField] private bool _ignoreLOS = false;
        [SerializeField] private int _hitPenaltyPerTile = 5;
        [SerializeField] private float _accuracyMultiplier = 1f;
        [SerializeField] private int _accuracyFlatBonus = 0;

        [Header("Attack Animation")]
        [SerializeField] private string _attackAnimKey;

        private AbilityRootNode _rootNode;

        public string GetCardName => this.name;
        public string GetDescription => _description;
        public int GetApCost => _apCost;
        public int GetRange => _range;
        public int GetShopCost => _shopCost;
        public int GetShopWeight => _shopWeight;
        public AudioClip GetAbilitySFX => _abilitySFX;

        public int GetBaseHitChance => _baseHitChance;
        public int GetMinHitChance => _minHitChance;
        public int GetMaxHitChance => _maxHitChance;
        public int GetHitPenaltyPerTile => _hitPenaltyPerTile;
        public float GetAccuracyMultiplier => _accuracyMultiplier;
        public int GetAccuracyFlatBonus => _accuracyFlatBonus;
        public bool GetIgnoreLOS => _ignoreLOS;

        public string GetAttackAnimKey => _attackAnimKey;

        public AbilityRootNode RootNode
        {
            get
            {
                if (_rootNode == null)
                    foreach (AbilityNodeBase node in nodes)
                        if (node is AbilityRootNode)
                            _rootNode = node as AbilityRootNode;
                return _rootNode;
            }
        }

        public void UseAility(Unit user)
        {
            RootNode?.UseAbility(user);
        }

        public void EndEffects(Guid guid)
        {
            foreach (Node node in nodes)
            {
                if (node is IStoppable)
                    (node as IStoppable).Stop(guid);
            }
        }

        // Card "Reading" Testing
        public enum AbilityTypes
        {
            None,
            Damage,
            Heal,
            Buff,
            OverTime,
            RestoreAP,
            Knockback,
            DeckEffect,
            DiceRoll,
            CoinFlip,
            SpawnObj,
            SelfTarget,
            OtherTarget,
            IsAOE,
            TileTarget,
            UnitTarget
        }
        private AbilityTypes[] _cardAbilityTags;
        public AbilityTypes[] GetCardAbilityTags;

        private void SetAbilityTags()
        {
            List<AbilityTypes> temp = new();
            foreach (var type in Enum.GetValues(typeof(AbilityTypes)) as AbilityTypes[])
                if (CheckForNode(type) && !temp.Contains(type))
                    temp.Add(type);
            _cardAbilityTags = temp.ToArray();
        }
        private bool CheckForNode(AbilityTypes nodeType)
        {
            foreach (Node node in nodes)
                switch (nodeType)
                {
                    case AbilityTypes.Damage:
                        if (node is DamageEffect)
                            return true;
                        break;
                    case AbilityTypes.Heal:
                        if (node is HealEffect)
                            return true;
                        break;
                    case AbilityTypes.Buff:
                        if (node is BuffEffect)
                            return true;
                        break;
                    case AbilityTypes.OverTime:
                        if (node is OverTimeEffect)
                            return true;
                        break;
                    case AbilityTypes.RestoreAP:
                        if (node is RestoreAPEffect)
                            return true;
                        break;
                    case AbilityTypes.Knockback:
                        if (node is KnockBackEffect)
                            return true;
                        break;
                    case AbilityTypes.DeckEffect:
                        if (node is DeckEffect)
                            return true;
                        break;
                    case AbilityTypes.DiceRoll:
                        if (node is IRollDice)
                            return true;
                        break;
                    case AbilityTypes.CoinFlip:
                        if (node is IFlipCoins)
                            return true;
                        break;
                    case AbilityTypes.SpawnObj:
                        if (node is SpawnObjectEffect)
                            return true;
                        break;
                    case AbilityTypes.SelfTarget:
                        if (node is SelfTarget)
                            return true;
                        break;
                    case AbilityTypes.OtherTarget:
                        if (node is OtherTarget)
                            return true;
                        break;
                    case AbilityTypes.IsAOE:
                        if (node is OnAOETarget)
                            return true;
                        break;
                    case AbilityTypes.TileTarget:
                        if (node is OnAOETarget)
                            return true;
                        break;
                    case AbilityTypes.UnitTarget:
                        if (node is OnAOETarget)
                            return true;
                        break;
                }
            return false;
        }

    }
}
