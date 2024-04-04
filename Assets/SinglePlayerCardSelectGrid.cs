using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SinglePlayerCardSelectGrid : MonoBehaviour, ICardSelectGrid
{
    [SerializeField] List<CardData> _availableCards;
    [SerializeField] Transform _panelTrans;
    [SerializeField] GameObject _unlockedCardSelectElementPrefab;
    [SerializeField] GameObject _lockedCardSelectElementPrefab;
    [SerializeField] GameObject _cardListPrefab;
    [SerializeField] int maxDeckSize = 10;
    [SerializeField] TMP_Text _cardCount;

    private CardList _cardList;

    // TODO: remove serialization, only serialized for easier debugging
    [SerializeField] List<int> _playerCards;

    public bool isDeckFull { get; private set;}

    private GameObject _selectedDeckGO;
    private int _currWins = 0;
    private SinglePlayerGameController _gameController;

    // Get Cards From Collection
    private void Awake()
    {
        // TODO: Replace this with cloud data
        _availableCards = new List<CardData>() { };
        _availableCards = Resources.LoadAll<CardData>("ScriptableCards/").ToList();
        _cardCount.text = $"0/{maxDeckSize}";

        _gameController = SinglePlayerGameController.instance;

#if !DEDICATED_SERVER
        // This also shows the cards after awaited call is returned 
        SetCurrentWins();
#endif
        InstantiateAndSpawnCardList();
    }

    //TODO: check that waiting for a return from cloudSave isn't completely detrimental to the games performance
    private void SetCurrentWins()
    {
        _currWins = GetCurrentWins();


        ShowCards();
    }

    //TODO: imlement local save
    private int GetCurrentWins()
    {
        return 0;
    }



    //[ServerRpc(RequireOwnership = false)]
    public void InstantiateAndSpawnCardList()
    {
        _selectedDeckGO = Instantiate(_cardListPrefab);

        _cardList = _selectedDeckGO.GetComponent<CardList>();
        _cardList.isSinglePlayer = true;
        _cardList.Assign(_availableCards);
        _cardList.UpdateList(_playerCards.ToArray(), true);


    }

    // Display Cards
    public void ShowCards()
    {
        Debug.Log($"currWins = {_currWins}");

        for (int i = 0; i < _availableCards.Count; i++)
        {
            CardData cardData = _availableCards[i];
            bool cardIsLocked = cardData.winsToUnlock > _currWins;
            GameObject cardPrefab = cardIsLocked ? _lockedCardSelectElementPrefab : _unlockedCardSelectElementPrefab;
            GameObject cardUIElement = Instantiate(cardPrefab, _panelTrans);
            CardSelectElement cardElement = cardUIElement.GetComponent<CardSelectElement>();
            cardElement.grid = this;
            cardElement.cardIsLocked = cardIsLocked;
            cardData.selectionIndex = i;
            cardElement.PopulateData(cardData);
        }
    }

    // Track Player Cards

    public void ToggleCard(int cardIndex, GameObject cardObject)
    {
        if (_playerCards.Count >= maxDeckSize && !_playerCards.Contains(cardIndex))
        {
            return;
        }


        if (_playerCards.Contains(cardIndex))
        {
            RemoveCard(cardIndex, cardObject);
        }
        else
        {
            AddCard(cardIndex, cardObject);
        }
    }

    // Add cards to deck
    public void AddCard(int cardIndex, GameObject cardObject)
    {
        // TODO: it might be a good idea to have multiples of some cards 
        // TODO: (Optimization) dictionary/hashset would have faster look ups
        // Don't do anything if we already have the card
        if (_playerCards.Contains(cardIndex))
        {
            return;
        }

        Debug.Log($"add card: {_availableCards[cardIndex].cardName}");

        _playerCards.Add(cardIndex);

        ShowCardAsSelected(cardObject);

        CheckForFullDeck();
    }


    // TODO: Make the colors actual Color attributes
    private void ShowCardAsSelected(GameObject cardObject)
    {
        cardObject.GetComponent<Image>().color = new Color32(0x4D, 0x4D, 0x4D, 0xFF);
    }

    private void ShowCardAsUnselected(GameObject cardObject)
    {
        cardObject.GetComponent<Image>().color = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
    }

    // Remove cards from deck
    public void RemoveCard(int cardIndex, GameObject cardObject)
    {
        // TODO: it might be a good idea to have multiples of some cards *this might not apply here
        // TODO: (Optimization) dictionary/hashset would have faster look ups
        // Don't do anything if we don't have the card
        if (!_playerCards.Contains(cardIndex))
        {
            return;
        }
        Debug.Log($"remove card: {_availableCards[cardIndex].cardName}");
        _playerCards.Remove(cardIndex);

        ShowCardAsUnselected(cardObject);

        CheckForFullDeck();
    }

    // Send out update when deck is complete
    void CheckForFullDeck()
    {
        _cardCount.text = $"{_playerCards.Count}/{maxDeckSize}";


        if (_playerCards.Count == maxDeckSize)
        {
            _gameController.UpdateCardList(_playerCards);
            isDeckFull = true;
            return;
        }

        isDeckFull = false;
    }

    public List<CardData> AvailableCards
    {
        get { return _availableCards; }
    }
}
