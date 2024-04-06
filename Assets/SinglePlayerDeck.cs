using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class SinglePlayerDeck : MonoBehaviour, IDeck
{
    [SerializeField] private List<CardData> _remainingCards;
    [SerializeField] private SinglePlayerPlayer _owningPlayer;
    [SerializeField] private SinglePlayerDeck _deck;
    [SerializeField] private Button _deckUI;
    private IPlayer _playerInterface;

    SinglePlayerGameController _gameController;
    [SerializeField] private Hand _hand;

    public void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        _owningPlayer = GetComponent<SinglePlayerPlayer>();
        if (_owningPlayer == null)
        {
           _playerInterface = GetComponent<SinglePlayerAI>();
        }
        else
        {
            _playerInterface = _owningPlayer;
        }
        if (_hand == null)
        { 
            _hand = GetComponent<Hand>();
        }
    }

    public void DrawCard()
    {
        if (_remainingCards.Count == 0)
        {
            // TODO: handle this somehow
            Debug.LogWarning("Attempting to draw card from empty deck");
            return;
        }

        // Attempt to get open space from game controller
        Card card = _hand.GetOpenSlot();
        if (card != null)
        {
            var cardData = GetNextCard();
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

        _playerInterface.UpdateUI(null);

        _playerInterface.SetHasDrawn(true);
    }

    private CardData GetNextCard()
    {
        var card = _remainingCards[0];
        _remainingCards.RemoveAt(0);
        return card;
    }

    public void SetDeck(List<CardData> cards)
    {
        _remainingCards = cards;
    }
}
