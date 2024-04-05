using System.Diagnostics;
using UnityEngine;

public class HealthSystem
{
    public float maxHealth { private set; get; }
    public float currHealth { private set; get; }

    public delegate void OnDie();
    public OnDie onDie;  
    public delegate void OnHealthChange();
    public OnHealthChange onHealthChange; 

    // Constructor
    public HealthSystem(float maxHealth)
    {
        this.maxHealth = maxHealth;
        currHealth = maxHealth;
    }

    public void Dmg(float amnt)
    {
        float startHealth = currHealth;
        currHealth -= amnt;

        if (currHealth <= 0)
        {
            currHealth = 0;
            onDie?.Invoke();
        }

        if (currHealth != startHealth)
        {
            onHealthChange?.Invoke();
        }
    }

    public void Heal(float amnt)
    {
        float startHealth = currHealth;
        currHealth += amnt;

        if (currHealth > maxHealth)
        {
            currHealth = maxHealth;
        }

        if (currHealth != startHealth)
        {
            onHealthChange?.Invoke();
        }
    }

    public bool IsDead
    {
        get
        {
            return currHealth == 0;
        }
    }
}
