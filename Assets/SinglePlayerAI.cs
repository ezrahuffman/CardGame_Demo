using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinglePlayerAI : MonoBehaviour, IPlayer
{
    private HealthSystem _healthSystem;
    [SerializeField] int _maxHealth = 100;
    public bool canPlay { get; set; } = false;
    [SerializeField] Hand _hand;

    private void Awake()
    {
        _healthSystem = new HealthSystem(_maxHealth);
    }

    public HealthSystem HealthSystem => _healthSystem;

    public Hand Hand { get { return _hand; }}
    public HealthSystem.OnDie OnDieEvent { set => _healthSystem.onDie = value; }
    public bool CanDraw { get; }
    public bool HasPlayed
    { get; }
}
