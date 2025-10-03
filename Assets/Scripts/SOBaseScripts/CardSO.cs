using UnityEngine;
using UnityEngine.U2D.IK;

namespace CardSystem
{
    [CreateAssetMenu(fileName = "CardSO", menuName = "Deckbuilding System/New Card SO")]
    public class CardSO : ScriptableObject
    {
        //General Data
        [SerializeField] private string _cardName;
        [TextArea(1,3)]//min & max lines for test area
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
        public enum CardSubtypes
        {
            None,
            DealDamage,
            Utility,
            AreaOfEffect,
            CauseStatus,
            DelayedEffect
        }*/
        //[SerializeField] private CardSubtypes[] _cardSubTypes;

        [SerializeField] private bool _isDealDamage,
                                      _isAreaOfEffect,
                                      _isCauseStatus,
                                      _isDelayedEffect,
                                      _isUtility;
        public string CardName { get => _cardName; }
        public string Description { get => _description; }
        public int APCost { get => _apCost; }
        public bool IsTileTargeted { get => _isTileTargeted; }
        public CardTypes CardType { get => _cardType; }
        public GameObject CardPrefab { get => _cardPrefab; }

        public string GetCardTypeID()
        {
            int cardTypeInt = (int)CardType;
            char damageChar = _isDealDamage ? '1' : '0',
                 aoeChar = _isAreaOfEffect ? '1' : '0',
                 statusChar = _isCauseStatus ? '1' : '0',
                 delayedChar = _isDelayedEffect ? '1' : '0',
                 utilityChar = _isUtility ? '1' : '0';
            
            /* Potential option for using enum
             * 
            foreach (CardSubtypes cardSubType in _cardSubTypes)
            {
                switch(cardSubType)
                {
                    case CardSubtypes.DealDamage:
                        damageChar = '1';
                        break;
                    case CardSubtypes.Utility:
                        aoeChar = '1';
                        break;
                    case CardSubtypes.AreaOfEffect:
                        aoeChar = '1';
                        break;
                    case CardSubtypes.CauseStatus:
                        statusChar = '1';
                        break;
                    case CardSubtypes.DelayedEffect:
                        delayedChar = '1';
                        break;
                }
            }
            */

            return $"{cardTypeInt}{damageChar}{aoeChar}" +
                $"{statusChar}{delayedChar}{utilityChar}";
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
