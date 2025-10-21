namespace Interfaces
{
    public interface IDamagable
    {
        public abstract void ChangeHealth(int amount, bool isGain);
        //public abstract void TakeDamage(int damage);
    }
}
