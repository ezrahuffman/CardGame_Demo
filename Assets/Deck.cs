using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Deck : NetworkBehaviour
{
    [SerializeField] private List<CardData> _remainingCards;
    [SerializeField] private GamePlayer _owningPlayer;
    [SerializeField] private Deck _deck;
    [SerializeField] private Button _deckUI;
   

    GameController _gameController;
    private Hand _hand;

    public void Start()
    {
        Debug.Log("Start");
        Initialize();
    }


    public override void OnNetworkSpawn()
    {
        Debug.Log("OnNetworkSpawn");
        Initialize();
        base.OnNetworkSpawn();
    }

    private void Initialize()
    {
        Debug.Log("Initialize");
        


        if (GameController.instance != null)
        {
            _gameController = GameController.instance;
        }

        if (_owningPlayer == null)
        {
            Debug.LogError("deck is not associated with a player!");
            return;
        }

        _hand = _owningPlayer.Hand;

       // List<CardData> selectedCards = new List<CardData>() ;
        foreach (var cardList in FindObjectsOfType<CardList>())
        {
            if (cardList.OwnerClientId == OwnerClientId)
            {
                _remainingCards = cardList.cards;
            }
        }

        //SetRemainingCards(FindObjectOfType<CardList>().cards);

        
        foreach (var card in _remainingCards)
        {
            card.SetOwningPlayer(_owningPlayer);
        }

        //SetRemainingCardsServerRpc(new SerializableCardList(FindObjectOfType<CardList>().cards));
        
        // TODO: might need to replicate this step across the network
    }

    //[ServerRpc]
    //public void SetRemainingCardsServerRpc(SerializableCardList cardList)
    //{
    //    SetRemainingCards(cardList.cards);

    //    SetRemainingCardsClientRpc(cardList);
    //}

    //[ClientRpc]
    //private void SetRemainingCardsClientRpc(SerializableCardList cardList)
    //{
    //    SetRemainingCards(cardList.cards);
    //}

    public void SetDeck(List<CardData> cards)
    {
        _remainingCards = cards;
    }

    private void SetRemainingCards(List<CardData> cards)
    {
        _remainingCards = cards;

        foreach (var card in _remainingCards)
        {
            card.SetOwningPlayer(_owningPlayer);
        }
    }

    // Draw a card from the deck
    // This needs to be replicated across the server and clients
    public void DrawCard()
    {
        Debug.Log($"Draw Card on {OwnerClientId}");
        if (!_owningPlayer.CanDraw || !_owningPlayer.canPlay)
        {
            return;
        }

        if(_remainingCards.Count == 0)
        {
            // TODO: handle this somehow
            Debug.LogWarning("Attempting to draw card from empty deck");
            return;
        }

        // Attempt to get open space from game controller
        Card card = _hand.GetOpenSlot();
        if(card != null)
        {
            var cardData = GetNextCard();
            Debug.Log($"get next card {OwnerClientId}: {cardData.cardName} (Deck id: {GetInstanceID()})");
            card.PopulateData(cardData);
        }
        else
        {
            Debug.Log("no open slot card found to populate");
        }


        // Hide deck if we are out of cards
        // TODO: Despawn object on network
        if (_remainingCards.Count == 0)
        {
            _deckUI.gameObject.SetActive(false); 
        }

        if (IsOwner)
        {
            DrawCardServerRpc();
        }

        _owningPlayer.SetHasDrawn(true);
    }

    [ServerRpc]
    void DrawCardServerRpc(ServerRpcParams serverRpcParams = default)
    {
        DrawCard();

        DrawCardClientRpc(serverRpcParams.Receive.SenderClientId);

        _gameController.UpdateUI();
    }

    [ClientRpc]
    void DrawCardClientRpc(ulong clientId)
    {

        //Don't run again on calling client
        if(NetworkManager.LocalClientId == clientId)
        {
            return;
        }

        DrawCard();
    }

    private CardData GetNextCard()
    {
        var card = _remainingCards[0];
        _remainingCards.RemoveAt(0);
        return card;
    }

    public void SetOwningPlayer(GamePlayer player)
    {
        _owningPlayer = player;
    }

    internal GameObject GetUI()
    {
        return _deckUI.gameObject;
    }
}
