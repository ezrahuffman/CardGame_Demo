using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;


public class Player : NetworkBehaviour
{
    [SerializeField] protected float _maxHealth = 100f;
    [SerializeField] protected Deck _deck;
    [SerializeField] protected Hand _hand;
    [SerializeField] protected TMP_Text _enemyHealthTxt;
    [SerializeField] protected Hand _enemyHand;
    [SerializeField] protected Vector3 canvasTransformPosition;
    public EnemyStats enemyStats;

    protected HealthSystem _healthSystem;

    protected bool _hasDiscarded;
    protected bool _hasPlay;
    protected bool _hasSkipped;

    public delegate void OnTurnOver(Player player);
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
            ReparentPlayerToCanvasServerRpc();
        }
    }

    internal void UpdateUI(Player otherPlayer)
    {
        if (!IsLocalPlayer)
        {
            // This Player is the oponent on the client

            // Hide this UI

            _hand.GetUI().SetActive(false);
            _deck.GetUI().SetActive(false);

            return; 
        }

        Debug.Log($"ClientId= {NetworkManager.LocalClientId}, OwnerId: {OwnerClientId}");

        SetEnemyHealth(otherPlayer.Health);

        var trueEnemyHand = otherPlayer.Hand.GetAllSlots();
        var enemyHandSlots = _enemyHand.GetAllSlots();
        for (int i = 0; i < trueEnemyHand.Length; i++)
        {
            var enemyCard = trueEnemyHand[i];
            var enemyCardUI = enemyHandSlots[i];
            
            enemyCardUI.PopulateData(enemyCard.GetCardData());
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
    }

    [ClientRpc]
    void SetPlayerTransformPositionClientRpc()
    {
        SetPlayerTransformPosition();
    }

   
    private void Start()
    {
        Initialize();

        if (_gameController.Players.Count == 2 && IsServer)
            _gameController.UpdateUI();
    }

    // Called after player has used a card
    // TODO: FUTURE this could be good place for cards that don't end your turn
    internal void HasPerformedAction(Card card)
    {
        CheckTurn();
    }

    // Called after player has discarded a card
    // TODO: FUTURE this could be good place for cards that don't end your turn
    internal void HasDiscardedCard(Card card)
    {
        CheckTurn();
    }
    

    #region Turns
    void CheckTurn()
    {
        if ((_hasDiscarded && _hasPlay) || (_hasSkipped))
        {
            GoNextTurn();
        }
    }

    void GoNextTurn()
    {
        ResetPlayer();

        if (onTurnOver != null)
        {
            onTurnOver.Invoke(this);
        }
    }

    void ResetPlayer()
    {
        _hasDiscarded = false;
        _hasPlay = false;
        _hasSkipped = false;
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

}
