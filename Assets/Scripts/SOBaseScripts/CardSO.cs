using UnityEngine;

namespace CardSystem
{
    [CreateAssetMenu(fileName = "CardSO", menuName = "Deckbuilding System/New Card SO")]
    public class CardSO : ScriptableObject
    {
        //General Data
        [SerializeField] private string _cardName;
        [SerializeField] private string _description;
        [SerializeField] private int _apCost;
        [SerializeField] private bool _isTileTargeted;
        [SerializeField] private GameObject _cardPrefab;
        // add in fields for any art assets needed?

        public enum CardTypes
        {
            None,
            Misc,
            Range,
            Melee,
        }
        [SerializeField] private CardTypes _cardType;

        /* Figure out way to hide enum options
         * based on if the enum is in an array/list
         * through editorscript (multiple subtypes but no dupes)
         * 
        public enum CardSubtypes
        {
            DealDamage,
            Utility,
            AreaOfEffect,
            CauseStatus,
            DelayedEffect
        }
        */
        //Potentially remove in favor of enum above
        [SerializeField] private bool _isDealDamage,
                                      _isAreaOfEffect,
                                      _isCauseStatus,
                                      _isDelayedEffect,
                                      _isUtility;
        //
        public string CardName { get => _cardName; }
        public string Description { get => _description; }
        public int APCost { get => _apCost; }
        public bool IsTileTargeted { get => _isTileTargeted; }
        public CardTypes CardType { get => _cardType; }
        public GameObject CardPrefab { get => _cardPrefab; }

        public string GetCardTypeID()
        {
            int cardTypeInt = (int)CardType;
            char damageBool = _isDealDamage ? '1' : '0',
                 aoeBool = _isAreaOfEffect ? '1' : '0',
                 statusBool = _isCauseStatus ? '1' : '0',
                 delayedBool = _isDelayedEffect ? '1' : '0',
                 utilityBool = _isUtility ? '1' : '0';

            return $"{cardTypeInt}{damageBool}{aoeBool}" +
                $"{statusBool}{delayedBool}{utilityBool}";
        }

        //Range Card Data
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private float _projectileSpeed;
        [SerializeField] private bool _isRepeating;
        [SerializeField] private int _numRepeats;
        [SerializeField] private int _range;
        public GameObject ProjectilePrefab { get => _projectilePrefab; }
        public float ProjectileSpeed { get => _projectileSpeed; }
        public bool IsRepeating { get => _isRepeating; }
        public int NumRepeats { get => _numRepeats; }
        public int Range { get => _range; }

        //Melee Card Data
        [SerializeField] private GameObject _weaponPrefab;
        public GameObject WeaponPrefab { get => _weaponPrefab; }

        //AoE Card Data
        [SerializeField] private IAreaOfEffect.AoETypes _aoeType;
        [SerializeField] private int _aoeRange;
        public IAreaOfEffect.AoETypes AOEType { get => _aoeType; }
        public int AoERange { get => _aoeRange; }

        //Utility Card Data
        [SerializeField] private IUtility.CardUtilityTypes[] _utilityTypes;
        [SerializeField] private bool _hasMultipleUtilities;
        [SerializeField] private int _cardReturnValue;
        [SerializeField] private int _apRestoreValue;
        [SerializeField] private int _healValue;
        [SerializeField] private int _buffValue;
        public IUtility.CardUtilityTypes[] UtilityTypes { get => _utilityTypes; }
        public bool HasMultipleUtilities { get => _hasMultipleUtilities; }
        public int CardReturnValue { get => _cardReturnValue; }
        public int APRestoreValue { get => _apRestoreValue; }
        public int HealValue { get => _healValue; }
        public int BuffValue { get => _buffValue; }

        //DelayedEffect Card Data
        [SerializeField] private int _delayDuration;
        public int DelayDuration { get; }

        //Damage Card Data
        [SerializeField] private IDealDamage.CardDamageTypes[] _damageTypes;
        [SerializeField] private int _damageValue;
        public IDealDamage.CardDamageTypes[] DamageTypes { get => _damageTypes; }
        public int DamageValue { get => _damageValue; }

        //Status Card Data
        [SerializeField] private ICauseStatuses.CardStatusTypes[] _statusTypes;
        [SerializeField] private int _statusDuration;
        public ICauseStatuses.CardStatusTypes[] StatusTypes { get => _statusTypes; }
        public int StatusDuration { get => _statusDuration; }
    }
}
