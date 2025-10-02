using UnityEngine;

namespace CardSystem
{
    public abstract class CardBase : IAreaOfEffect, ICauseStatuses, IDelayedEffect, IUtility
    {
        //initial constructor uses CardSO param to grab relevant
        //card subtype data (determined mostly by CardSO.GetCardTypeID string)
        public CardBase(CardSO so)
        {
            GrabSOData(so);
        }

        //General card fields
        protected string _cardName;
        protected string _description;
        protected int _apCost;
        protected string _cardTypeID;
        protected bool _isTileTargeted; //used for both IRangeAbility & IMeleeAbility properties

        public string CardName { get => _cardName; }
        public string Description { get => _description; }
        public int APCost { get => _apCost; }
        public bool IsTileTargeted { get => _isTileTargeted; }
        public string CardTypeID { get => _cardTypeID; }

        //IRangeAbility relevant fields
        protected GameObject _projectilePrefab;
        protected float _projectileSpeed;
        protected bool _isRepeating;
        protected int _numRepeats;
        protected int _range;

        //IMeleeAbility relevant fields
        protected GameObject _weaponPrefab;

        //IDealDamage relevant fields
        protected IDealDamage.CardDamageTypes[] _damageTypes;
        protected int _damageValue;

        //IAoE relevant fields & properties
        protected IAreaOfEffect.AoETypes _aoeType;
        protected int _aoeRange;
        public IAreaOfEffect.AoETypes AoEType { get => _aoeType; }
        public int AoERange { get => _aoeRange; }
        public virtual void AffectTiles()
        {
            Debug.Log("AoE Affect Tiles Triggered.");
        }

        //ICauseStatuses relevant fields & properties
        protected ICauseStatuses.CardStatusTypes[] _statusTypes;
        protected int _statusDuration;
        public ICauseStatuses.CardStatusTypes[] StatusTypes { get => _statusTypes; }
        public int StatusDuration { get => _statusDuration; }
        public virtual void ApplyStatus()
        {
            Debug.Log("Status Apply Triggered.");
        }

        //IDelayedEffect relevant fields & properties
        protected int _delayDuration;
        public int DelayDuration { get => _delayDuration; }
        public virtual void BeginDelay()
        {
            Debug.Log("Delay Begin Triggered.");
        }
        public virtual void DelayEnd()
        {
            Debug.Log("Delay End Triggered.");
        }

        //IUtility relevant fields & properties
        protected IUtility.CardUtilityTypes[] _utilityTypes;
        protected bool _hasMultipleUtilities;
        protected int _cardReturnValue;
        protected int _apRestoreValue;
        protected int _healValue;
        protected int _buffValue;
        public IUtility.CardUtilityTypes[] UtilityTypes { get => _utilityTypes; }
        public bool HasMultipleUtilities { get => _hasMultipleUtilities; }
        public int CardReturnValue { get => _cardReturnValue; }
        public int APRestoreValue { get => _apRestoreValue; }
        public int HealValue { get => _healValue; }
        public int BuffValue { get => _buffValue; }
        public virtual void CauseEffect()
        {
            Debug.Log("Utility Effect Caused.");
        }

        public virtual void GrabSOData(CardSO so)
        {
            _cardName = so.CardName;
            _description = so.Description;
            _apCost = so.APCost;
            _isTileTargeted = so.IsTileTargeted;

            _cardTypeID = so.GetCardTypeID();

            for (int i = 0; i < _cardTypeID.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        switch (_cardTypeID[i])//Card type
                        {
                            case '0'://None
                                break;
                            case '1'://Misc
                                break;
                            case '2'://Range
                                _projectilePrefab = so.ProjectilePrefab;
                                _projectileSpeed = so.ProjectileSpeed;
                                _isRepeating = so.IsRepeating;
                                _numRepeats = so.NumRepeats;
                                _range = so.Range;
                                break;
                            case '3'://Melee
                                _weaponPrefab = so.WeaponPrefab;
                                break;
                        }
                        break;
                    case 1:
                        if (_cardTypeID[i] == '1')//IsDamageBool
                        {
                            _damageTypes = so.DamageTypes;
                            _damageValue = so.DamageValue;
                        }
                        break;
                    case 2:
                        if (_cardTypeID[i] == '1')//AoEBool
                        {
                            _aoeType = so.AOEType;
                            _aoeRange = so.AoERange;
                        }
                        break;
                    case 3:
                        if (_cardTypeID[i] == '1')//Statusbool
                        {
                            _statusTypes = so.StatusTypes;
                            _statusDuration = so.StatusDuration;
                        }
                        break;
                    case 4:
                        if (_cardTypeID[i] == '1')//DelayedBool
                        {
                            _delayDuration = so.DelayDuration;
                        }
                        break;
                    case 5:
                        if (_cardTypeID[i] == '1')//UtilityBool
                        {
                            _utilityTypes = so.UtilityTypes;
                            _hasMultipleUtilities = so.HasMultipleUtilities;
                            _cardReturnValue = so.CardReturnValue;
                            _apCost = so.APCost;
                            _healValue = so.HealValue;
                            _buffValue = so.BuffValue;
                        }
                        break;

                }
            }
        }

        //potentially make virtual if all cards act the same
        public abstract void Use();
        public abstract void Discard();
        //
    }
}
