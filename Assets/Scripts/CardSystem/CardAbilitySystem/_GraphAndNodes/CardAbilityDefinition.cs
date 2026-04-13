using CardSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace CardSystem
{
    public enum CardRarity { Common, Rare, Epic }
    public enum CardCategory { Melee, Ranged, Heal, Shield, Draw, Trap, Gambling }
    public enum CardTag { Damage, Heal, Shield, Gambling, Draw, SelfDamage, Trap, Ranged, Melee, AOE }

    [CreateAssetMenu(fileName = "NewCardAbility", menuName = "Card System/New Card Ability")]
    public class CardAbilityDefinition : NodeGraph
    {
        [Header("Card Info")]
        [TextArea(1, 3)]
        [SerializeField] private string _description;
        [SerializeField] private AudioClip _abilitySFX;
        [SerializeField] private CardCategory _cardCategory;
        [SerializeField] private CardTag[] _cardTags;
        [SerializeField] private int _effectValue;

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
        [SerializeField] private AttackAnimKey _attackAnimKey = AttackAnimKey.None;
        public AttackAnimKey GetAttackAnimKey => _attackAnimKey;

        private AbilityRootNode _rootNode;

        [SerializeField] private CardRarity _baseCardRarity;
        [SerializeField] private EffectUpgrade[] _onRareUpgradeEffects = new EffectUpgrade[0];
        [SerializeField] private EffectUpgrade[] _onEpicUpgradeEffects = new EffectUpgrade[0];

        //prop drawer and enum stuff
        List<EffectStrategy> _effectOptions = new();
        string[] _effectOptionsStrings;
        public EffectStrategy[] GetEffects => _effectOptions.ToArray();
        public string[] GetEffectStrings => _effectOptionsStrings;

        public string GetCardName => this.name;
        public string GetDescription => _description;
        public int GetApCost => _apCost;
        public int GetRange => _range;
        public int GetShopCost => _shopCost;
        public int GetShopWeight => _shopWeight;
        public AudioClip GetAbilitySFX => _abilitySFX;
        public CardCategory GetCardCategory => _cardCategory;
        public int GetEffectValue => _effectValue;

        public int GetBaseHitChance => _baseHitChance;
        public int GetMinHitChance => _minHitChance;
        public int GetMaxHitChance => _maxHitChance;
        public int GetHitPenaltyPerTile => _hitPenaltyPerTile;
        public float GetAccuracyMultiplier => _accuracyMultiplier;
        public int GetAccuracyFlatBonus => _accuracyFlatBonus;
        public bool GetIgnoreLOS => _ignoreLOS;

        public CardRarity GetBaseCardRarity => _baseCardRarity;

        public EffectUpgrade GetUpgradeEffect(EffectStrategy strat, CardRarity rarity)
        {
            if (rarity == CardRarity.Common) return null;
            var effectCollection = rarity == CardRarity.Rare ? _onRareUpgradeEffects : _onEpicUpgradeEffects;
            foreach (var effect in effectCollection)
                if (effect.effectToUpgrade == strat)
                    return effect;
            return null;
        }

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

        public void UseAbility(Unit user, CardRarity rartity = CardRarity.Common)
        {
            RootNode?.UseAbility(user, rartity);
        }

        public void EndEffects(Guid guid)
        {
            foreach (Node node in nodes)
            {
                if (node is IStoppable)
                    (node as IStoppable).Stop(guid);
            }
        }
        public void SetEffectDefForUpgradeCollections()
        {
            for (int i = 0; i < _onRareUpgradeEffects.Length; i++)
                _onRareUpgradeEffects[i].SetCardDef(this);
            for (int i = 0; i < _onEpicUpgradeEffects.Length; i++)
                _onEpicUpgradeEffects[i].SetCardDef(this);
        }

        public List<EffectStrategy> GetEffectOptions()
        {
            // grab all valid effect nodes is card def graph
            _effectOptions = new();
            foreach (var node in nodes)
                if (node is IUseEffectValue)
                    _effectOptions.Add(node as EffectStrategy);
            return _effectOptions;
        }

        public string[] GetEffectOptionsStrings()
        {
            if (_effectOptions == null || _effectOptions.Count == 0) return new string[0];
            // create string array to use for popup content
            _effectOptionsStrings = new string[_effectOptions.Count];
            for (int i = 0; i < _effectOptions.Count; i++)
            {
                var node = _effectOptions[i];
                _effectOptionsStrings[i] = $"{i}-{GetNodePath(node, string.Empty)}";
            }
            return _effectOptionsStrings;
        }
        private string GetNodePath(Node node, string curPath)
        {
            if (node == null || node is AbilityRootNode) return curPath;
            Node parent = null;
            curPath = node.name + (curPath == string.Empty ? "" : $">{curPath}");
            foreach (var port in node.Inputs)
            {
                parent = port.Connection.node;
                if (parent == null) continue;
                break;
            }
            return GetNodePath(parent, curPath);
        }
    }
}
[System.Serializable]
public class EffectUpgrade
{
    public CardAbilityDefinition cardDef;
    public string effectName;

    public int valueToAdd = 0;
    public EffectStrategy effectToUpgrade;

    public void SetCardDef(CardAbilityDefinition def)
    {
        if (effectToUpgrade != null && effectName != effectToUpgrade.name)
            effectName = effectToUpgrade.name;

        if (cardDef == def) return;
        cardDef = def;
    }
}
