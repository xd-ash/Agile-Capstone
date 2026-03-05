using UnityEngine;
using XNode;
using System;
using System.Collections.Generic;
using CardSystem;

namespace CardSystem
{
    public enum CardRarity
    {
        Common,
        Rare,
        Epic
    }
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

        [SerializeField] private CardRarity _baseCardRarity;
        [SerializeField] private EffectUpgrade[] _onRareUpgradeEffects = new EffectUpgrade[0];
        [SerializeField] private EffectUpgrade[] _onEpicUpgradeEffects = new EffectUpgrade[0];

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

        public CardRarity GetBaseCardRarity => _baseCardRarity;
        public EffectUpgrade GetRareUpgradeEffect(EffectStrategy strat)
        {
            foreach (var effect in _onRareUpgradeEffects)
                if (effect.effectToUpgrade = strat)
                    return effect;
            return null;
        }
        public EffectUpgrade GetEpicUpgradeEffect(EffectStrategy strat)
        {
            foreach (var effect in _onEpicUpgradeEffects)
                if (effect.effectToUpgrade = strat)
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
        public void SetEffectDefForUpgradeCollections()
        {
            foreach (var effect in _onRareUpgradeEffects)
                effect.SetCardDef(this);
            foreach (var effect in _onEpicUpgradeEffects)
                effect.SetCardDef(this);
        }
    }
}
[System.Serializable]
public class EffectUpgrade
{
    public CardAbilityDefinition cardDef;
    public string effectName;

    public int valueToAdd;
    public EffectStrategy effectToUpgrade;

    public void SetCardDef(CardAbilityDefinition def)
    {
        if (effectToUpgrade != null && effectName != effectToUpgrade.name)
            effectName = effectToUpgrade.name;

        if (cardDef == def) return;
        cardDef = def;
    }
}
