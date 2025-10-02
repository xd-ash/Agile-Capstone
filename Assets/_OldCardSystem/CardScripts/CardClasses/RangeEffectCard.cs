using UnityEngine;

namespace OldCardSystem
{
    public class RangeEffectCard : CardBase, IRangeAbility
    {
        public RangeEffectCard(CardSO so) : base(so)
        {

        }

        //relevant fields for these interface properties are inherited from CardBase
        public GameObject ProjectilePrefab { get => _projectilePrefab; }
        public float ProjectileSpeed { get => _projectileSpeed; }
        public bool IsRepeating { get => _isRepeating; }
        public int NumRepeats { get => _numRepeats; }
        public int Range { get => _range; }

        public override void Use()
        {
            //
        }
        public override void Discard()
        {
            //
        }
        public void Shoot(GameObject proj)
        {
            Debug.Log("Range Effect Shoot Triggered.");
        }
    }
}
