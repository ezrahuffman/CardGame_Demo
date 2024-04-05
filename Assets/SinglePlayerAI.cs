using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinglePlayerAI : MonoBehaviour, IPlayer
{
    private HealthSystem _healthSystem;
    [SerializeField] int _maxHealth = 100;
    public bool canPlay { get; set; } = false;
    [SerializeField] Hand _hand;
    float prevHealth;

    private void Awake()
    {
        _healthSystem = new HealthSystem(_maxHealth);
        _healthSystem.onHealthChange += OnHealthChange;
        prevHealth = _healthSystem.currHealth;
    }

    private void OnHealthChange()
    {
        Debug.Log($"Health changed from {prevHealth} to {_healthSystem.currHealth}");
        prevHealth = _healthSystem.currHealth;
    }

    public HealthSystem HealthSystem => _healthSystem;

    public Hand Hand { get { return _hand; }}
    public HealthSystem.OnDie OnDieEvent { set => _healthSystem.onDie = value; }
    public bool CanDraw { get; }
    public bool HasPlayed
    { get; }

    // TODO: check this
    public void Discard(Card card)
    {
        _hand.OpenSlot(card);
    }

    // TODO: check this
    public void HasPerformedAction(Card card)
    {
        //_hasPlayed = true;
        //CheckTurn();
    }

    public void DealDmg(float amnt)
    {
        Debug.Log($"Dealing {amnt} damage to player");
        SinglePlayerGameController.instance.DealDmg(amnt, isPlayer: false);
    }
}
