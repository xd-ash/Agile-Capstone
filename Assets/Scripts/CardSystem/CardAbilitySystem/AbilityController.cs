using UnityEngine;

namespace CardSystem
{
    public class AbilityController : MonoBehaviour
    {
        [SerializeField] protected AbilityDefinition _loadedAbility;
        [SerializeField] protected UnitStatsScript _currentUnit;

        protected virtual void Start()
        {
            UseLoadedAbility();
        }

        public void UseLoadedAbility()
        {
            Debug.Log($"{_loadedAbility.name} used.");

            _loadedAbility?.UseAility(_currentUnit);
        }
    }
}