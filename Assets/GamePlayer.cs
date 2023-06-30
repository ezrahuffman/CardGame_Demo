using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;


public class GamePlayer : NetworkBehaviour
{
    [SerializeField] protected float _maxHealth = 100f;
    [SerializeField] protected Deck _deck;
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

    protected bool _hasDiscarded;
    protected bool _hasPlayed;
    protected bool _hasSkipped;
    protected bool _hasDrawnCard;

    public delegate void OnTurnOver(GamePlayer player);
    public OnTurnOver onTurnOver;

    public bool canPlay = false;

    private GameController _gameController;

    // This is just for easier debugging
    private float enemyHealth;
    private List<Card> enemyCards;

    public override void OnNetworkSpawn()
    {
        _gameController = GameController.instance;
       
        Initialize();

        _gameController.AddPlayer(this);

        base.OnNetworkSpawn();
    }

    internal void DealDmg(float effectAmnt)
    {
        if (!IsServer && IsOwner)
        {
            DealDmgServerRpc(effectAmnt);
            return;
        }

        if (!IsOwner)
        {
            Debug.Log($"DealDmg ownerClientID: {OwnerClientId}");
        }
    }

    [ServerRpc]
    internal void DealDmgServerRpc(float effectAmnt, ServerRpcParams serverRpcParams = default)
    {
        _gameController.DealDmg(effectAmnt, serverRpcParams.Receive.SenderClientId);
    }

    #region Discard
    internal void Discard(Card card)
    {
        int cardIndex = Array.IndexOf(Hand.GetAllSlots(), card);

        // Don't do anything if we can't play
        if (!canPlay)
        {
            return;
        }
        HideCardAndOpenSlot(card);
        DiscardServerRpc(cardIndex);
    }
    
    [ServerRpc]
    void DiscardServerRpc(int cardIndex, ServerRpcParams serverRpcParams = default)
    {
        HideCardAndOpenSlot(Hand.GetAllSlots()[cardIndex]);

        DiscardClientRpc(cardIndex, serverRpcParams.Receive.SenderClientId);
        
        _gameController.UpdateUI();
        
    }

    internal void SetHasDrawn(bool v)
    {
        _hasDrawnCard = v;
    }

    [ClientRpc]

    void DiscardClientRpc(int cardIndex,ulong senderClientId)
    {
        if(senderClientId == NetworkManager.LocalClientId)
        {
            return;
        }

        // Hide the enemy card for players that are not the local player
        HideCardAndOpenSlot(Hand.GetAllSlots()[cardIndex]);
    }

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

        Debug.Log("INITIALIZE PLAYER");

        if (_healthSystem == null)
        {
            _healthSystem = new HealthSystem(_maxHealth);
            _healthSystem.onHealthChange += OnHealthChange;
            _deck.SetOwningPlayer(this);
        }

        if (enemyStats == null)
        {
            enemyStats = new EnemyStats();
        }


        // Parent player to the canvas
        if (IsSpawned && IsOwner)
        {
            ReparentPlayerToCanvasServerRpc(); //TODO: This might need to be moved to GameController.
        }

        
    }

    internal void UpdateUI(GamePlayer otherPlayer)
    {
        if (!IsLocalPlayer)
        {
            // This Player is the oponent on the client

            // Hide this UI

            _hand.GetUI().SetActive(false);
            _deck.GetUI().SetActive(false);
            _skipButton.SetActive(false);
            _turnIndicator.SetActive(false);
            _background.SetActive(false);
            _panel.SetActive(false);

            return; 
        }

        _turnIndicator.gameObject.SetActive(canPlay);
        _skipButton.SetActive(canPlay);

        SetEnemyHealth(otherPlayer.Health);
        SetPlayerHealth(Health);

        var trueEnemyHand = otherPlayer.Hand.GetAllSlots();
        var enemyHandSlots = _enemyHand.GetAllSlots();
        for (int i = 0; i < trueEnemyHand.Length; i++)
        {
            var enemyCard = trueEnemyHand[i];
            var enemyCardUI = enemyHandSlots[i];
            if (enemyCard.GetCardData() == null)
            {
                Debug.Log($"enemy card is null");
            }
            else
            {
                Debug.Log($"enemyCard_{i}: {enemyCard.GetCardData().cardName}");


                enemyCardUI.PopulateData(enemyCard.GetCardData());
            }
        }
    }

    private void SetEnemyHealth(float health)
    {
        if(health == 0)
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

    [ServerRpc]
    void ReparentPlayerToCanvasServerRpc()
    {
        Debug.Log("Parent Object");
        Transform canvasTrans = FindObjectOfType<Canvas>().transform;
        transform.SetParent(canvasTrans);
        SetPlayerTransformPosition();
        SetPlayerTransformPositionClientRpc();
    }

    void SetPlayerTransformPosition()
    {
        RectTransform rectTrans = transform as RectTransform;
        rectTrans.localPosition = Vector3.zero;
        rectTrans.rect.Set(rectTrans.rect.x, rectTrans.rect.y, Screen.width, Screen.height);
        rectTrans.localScale = Vector3.one;
    }

    [ClientRpc]
    void SetPlayerTransformPositionClientRpc()
    {
        SetPlayerTransformPosition();
    }

   
    private void Start()
    {
        Initialize();
    }

    // Called after player has used a card
    // TODO: FUTURE this could be good place for cards that don't end your turn
    internal void HasPerformedAction(Card card)
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
        if (GetShouldEndTurn())
        {
            GoNextTurnServerRpc();
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
        _hasDiscarded = false;
        _hasPlayed = false;
        _hasSkipped = false;
        _hasDrawnCard = false;
        canPlay = false;
    }
    #endregion

    public void TakeDmg(float amnt)
    {
        Debug.Log($"{OwnerClientId} (local: {GetInstanceID()} has taken {amnt} dmg");
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
        if (IsOwner)
        {
            Debug.Log("Health changed, update UI");
            UpdateUIServerRpc();
        }
        else
        {
            Debug.Log($"UpateUI ownerClientID: {OwnerClientId}");
        }
    }

    [ServerRpc]
    void UpdateUIServerRpc()
    {
        _gameController.UpdateUI();
    }

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
        set { _healthSystem.onDie += value; }
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

}
