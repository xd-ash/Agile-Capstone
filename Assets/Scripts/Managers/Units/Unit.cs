using System;
using Interfaces;
using UnityEngine;

public enum Team {Friendly, Enemy}
public class Unit : MonoBehaviour, IDamagable
{
    [Header("Team and stats")] 
    public Team team;
    public int maxHealth;
    public int health;
    
    [Header("Target for Enemy units")]
    [SerializeField] private Unit _target;
    
    [Header("Action Points")] 
    public int maxAP;
    public int ap;
    
    public event Action<Unit> OnApChanged; 

    private void Awake()
    {
        health = maxHealth;
        ap = maxAP;
        RaiseHealthEvent();
    }

    public void ChangeHealth(int amount, bool isGain)
    {
        int uAmount = Math.Abs(amount);
        health += isGain ? amount : -amount;

        if (health >= maxHealth)
            health = maxHealth;
        else if (health <= 0)
        {
            health = 0;
            Destroy(gameObject); // make ondeath method and/or event
        }

        RaiseHealthEvent();
    }
    /* Removed in favor of ChangeHealth to condense 
    public void GetHealed(int healAmount)
    {
        health += healAmount;
        RaiseHealthEvent();
        if (health >  maxHealth)
            health = maxHealth;
    }
    public void TakeDamage(int damage)
    {
        health -= damage;
        RaiseHealthEvent();
        Debug.Log($"[{team}] " + this.name + " took " + damage + " damage, remaining health: " + health);
        if (health <= 0)
        {
            Destroy(this.gameObject);
            Debug.Log($"[{team}] " + this.name + " unit died");
        }
    }
    */
    public void DealDamage(int damage = 2)
    {
        if (team == Team.Enemy && _target != null)
            _target.ChangeHealth(damage, false);
    }

    private void RaiseHealthEvent()
    {
        if (team == Team.Friendly)
            DamageEvents.RaisePlayerDamaged(health,maxHealth);
        else
            DamageEvents.RaiseEnemyDamaged(health, maxHealth);
    }
    
    public void RefreshAP()
    {
        ap = maxAP;
        OnApChanged?.Invoke(this);
    }
   
    public bool CanSpend(int cost) => ap >= cost;

    public bool SpendAP(int cost)
    {
        if (!CanSpend(cost))
            return false;
        ap -= cost;
        OnApChanged?.Invoke(this);

        return true;
    }
}
