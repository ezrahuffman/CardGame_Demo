using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class SinglePlayerPlayer : MonoBehaviour, IPlayer
{
    [SerializeField] protected float _maxHealth = 100f;
    [SerializeField] protected IDeck _deck;
    [SerializeField] protected Hand _hand;
    [SerializeField] protected TMP_Text _enemyHealthTxt;
    [SerializeField] protected TMP_Text _playerHealthTxt;
    [SerializeField] protected Hand _enemyHand;
    [SerializeField] protected Vector3 _canvasTransformPosition;
    [SerializeField] protected GameObject _skipButton;
    [SerializeField] protected GameObject _turnIndicator;
    [SerializeField] protected GameObject _background;
    [SerializeField] protected GameObject _panel;
    public EnemyStats enemyStats;

    protected HealthSystem _healthSystem;

    [SerializeField]
    protected bool _hasDiscarded;
    [SerializeField]
    protected bool _hasPlayed;
    [SerializeField]
    protected bool _hasSkipped;
    [SerializeField]
    protected bool _hasDrawnCard;

    public delegate void OnTurnOver(IPlayer player);
    public OnTurnOver onTurnOver;

    public bool canPlay { get; set; } = false;

    private SinglePlayerGameController _gameController;

    // This is just for easier debugging
    private float enemyHealth;
    private List<Card> enemyCards;

    public void DealDmg(float effectAmnt)
    {
        _gameController.DealDmg(effectAmnt, isPlayer: true);
        //if (!IsServer && IsOwner)
        //{
        //    DealDmgServerRpc(effectAmnt);
        //    return;
        //}

        //if (!IsOwner)
        //{
        //    Debug.Log($"DealDmg ownerClientID: {OwnerClientId}");
        //}
    }

    //[ServerRpc]
    //internal void DealDmgServerRpc(float effectAmnt, ServerRpcParams serverRpcParams = default)
    //{
    //    _gameController.DealDmg(effectAmnt, serverRpcParams.Receive.SenderClientId);
    //}

    #region Discard
    public void Discard(Card card)
    {
        int cardIndex = Array.IndexOf(Hand.GetAllSlots(), card);

        // Don't do anything if we can't play
        if (!canPlay)
        {
            return;
        }
        HideCardAndOpenSlot(card);
        //DiscardServerRpc(cardIndex);

        //if (IsOwner)
        //{
        //    UpdateUIServerRpc();
        //}
    }

    //[ServerRpc]
    //void DiscardServerRpc(int cardIndex, ServerRpcParams serverRpcParams = default)
    //{
    //    HideCardAndOpenSlot(Hand.GetAllSlots()[cardIndex]);

    //    DiscardClientRpc(cardIndex, serverRpcParams.Receive.SenderClientId);

    //    _gameController.UpdateUI();

    //}

    public void SetHasDrawn(bool v)
    {
        _hasDrawnCard = v;
    }

    [ClientRpc]

    //void DiscardClientRpc(int cardIndex, ulong senderClientId)
    //{
    //    if (senderClientId == NetworkManager.LocalClientId)
    //    {
    //        return;
    //    }

    //    // Hide the enemy card for players that are not the local player
    //    HideCardAndOpenSlot(Hand.GetAllSlots()[cardIndex]);

    //    // This is the only server rpc that doesn't require the sender to be the owner
    //    _gameController.UpdateUIServerRpc();
    //}

    void HideCardAndOpenSlot(Card card)
    {
        Debug.Log($"hide {card.GetInstanceID()}");
        card.gameObject.SetActive(false);
        Hand.OpenSlot(card);

        HasDiscardedCard(card);
    }

    #endregion

    private void Initialize()
    {
        _gameController = SinglePlayerGameController.instance;

        Debug.Log("INITIALIZE PLAYER");

        if (_healthSystem == null)
        {
            _healthSystem = new HealthSystem(_maxHealth);
            //_deck.SetOwningPlayer(this);
        }
        _healthSystem.onHealthChange += OnHealthChange;

        if (enemyStats == null)
        {
            enemyStats = new EnemyStats();
        }

        foreach(CardData card in _gameController.SelectedCards.cards)
        {
            Debug.Log($"selected card: {card.cardName}");
        }


        // Parent player to the canvas
        //if (IsSpawned && IsOwner)
        //{
        //    ReparentPlayerToCanvasServerRpc(); //TODO: This might need to be moved to GameController.
        //}
        _deck = GetComponent<SinglePlayerDeck>();
        _deck.SetDeck(_gameController.SelectedCards.cards);


        UpdateUI(null);
    }

    public void UpdateUI(GamePlayer otherPlayer)
    {
        //if (!IsLocalPlayer)
        //{
        //    // This Player is the oponent on the client

        //    // Hide this UI

        //    _hand.GetUI().SetActive(false);
        //    _deck.GetUI().SetActive(false);
        //    _skipButton.SetActive(false);
        //    _turnIndicator.SetActive(false);
        //    _background.SetActive(false);
        //    _panel.SetActive(false);

        //    return;
        //}

        _turnIndicator.gameObject.SetActive(canPlay);
        _skipButton.SetActive(canPlay);

        if (_gameController == null)
        {
            _gameController = SinglePlayerGameController.instance;
        }

        IPlayer aiPlayer = _gameController.AI;
        if (aiPlayer == null)
        {
            Debug.Log("AI is null");
            return;
        }
        SetEnemyHealth(aiPlayer.HealthSystem.currHealth);
        SetPlayerHealth(Health);

        //var trueEnemyHand = aiPlayer.Hand.GetAllSlots();
        //var enemyHandSlots = _enemyHand.GetAllSlots();
        //for (int i = 0; i < trueEnemyHand.Length; i++)
        //{
        //    var enemyCard = trueEnemyHand[i];
        //    var enemyCardUI = enemyHandSlots[i];
        //    if (enemyCard.GetCardData() == null)
        //    {
        //        Debug.Log($"enemy card is null");
        //        enemyCardUI.gameObject.SetActive(false);
        //    }
        //    else
        //    {
        //        Debug.Log($"enemyCard_{i}: {enemyCard.GetCardData().cardName}");


        //        enemyCardUI.PopulateData(enemyCard.GetCardData());
        //    }
        //}
    }

    private void SetEnemyHealth(float health)
    {
        if (health == 0)
        {
            _enemyHealthTxt.enabled = false;
            return;
        }

        _enemyHealthTxt.text = $"enemyHP: {health}";
        _enemyHealthTxt.enabled = true;
    }

    private void SetPlayerHealth(float health)
    {
        if (health == 0)
        {
            _playerHealthTxt.enabled = false;
            return;
        }

        _playerHealthTxt.text = $"HP: {health}";
        _playerHealthTxt.enabled = true;
    }


    private void Start()
    {
        Initialize();
    }

    // Called after player has used a card
    // TODO: FUTURE this could be good place for cards that don't end your turn
    public void HasPerformedAction(Card card)
    {
        _hasPlayed = true;
        CheckTurn();
    }

    // Called after player has discarded a card
    // TODO: FUTURE this could be good place for cards that don't end your turn
    internal void HasDiscardedCard(Card card)
    {
        _hasDiscarded = true;
        CheckTurn();
    }

    public void Skip()
    {
        if (!canPlay)
        {
            return;
        }

        _hasSkipped = true;
        CheckTurn();
    }


    #region Turns
    void CheckTurn()
    {
        //Debug.Log($"CheckTurn() on {NetworkManager.LocalClientId}; ownerId: {OwnerClientId}");

        if (GetShouldEndTurn())
        {
            GoNextTurn();
        }

        //if (IsOwner)
        //{
        //    CheckTurnServerRpc();
        //}
    }

    bool GetShouldEndTurn()
    {
        return ((_hasDiscarded && _hasPlayed) || (_hasSkipped));
    }

    [ServerRpc]
    void GoNextTurnServerRpc()
    {
        GoNextTurn();

        GoNextTurnClientRpc();
    }

    [ClientRpc]
    void GoNextTurnClientRpc()
    {
        GoNextTurn();
    }

    void GoNextTurn()
    {
        ResetPlayer();

        onTurnOver?.Invoke(this);
    }

    void ResetPlayer()
    {
        Debug.Log("ResetPlayer()"); 
        _hasDiscarded = false;
        _hasPlayed = false;
        _hasSkipped = false;
        _hasDrawnCard = false;
        canPlay = false;
    }
    #endregion

    public void TakeDmg(float amnt)
    {
        //Debug.Log($"{OwnerClientId} (local: {GetInstanceID()} has taken {amnt} dmg");
        _healthSystem.Dmg(amnt);
    }

    public void Heal(float amnt)
    {
        _healthSystem.Heal(amnt);
    }

    public void OnHealthChange()
    {
        // We probably don't want to run anything else if dead
        // TODO: Check that this is the desired behavior
        if (_healthSystem.IsDead)
        {
            return;
        }

        // Propagate changes
        //if (IsOwner)
        //{
        //    Debug.Log("Health changed, update UI");
        //    UpdateUIServerRpc();
        //}
        //else
        //{
        //    Debug.Log($"UpateUI ownerClientID: {OwnerClientId}");
        //}
    }

    //[ServerRpc]
    //void UpdateUIServerRpc()
    //{
    //    _gameController.UpdateUI();
    //}

    public float Health
    {
        get
        {
            return _healthSystem.currHealth;
        }
    }

    public float MaxHealth
    {
        get
        {
            return _healthSystem.maxHealth;
        }
    }

    public HealthSystem.OnDie OnDieEvent
    {
        set { 
            if (_healthSystem == null)
            {
                _healthSystem = new HealthSystem(_maxHealth);
            }
            _healthSystem.onDie += value; }
    }

    public Hand Hand
    {
        get { return _hand; }
    }

    public bool CanDraw
    {
        get { return !_hasDrawnCard; }
    }

    public bool HasPlayed
    {
        get { return _hasPlayed; }
    }

    // TODO: this should just be stored in variable
    public CardList CardList
    {
        get { return GetComponent<CardList>(); }
    }

    public HealthSystem HealthSystem => _healthSystem;

}
