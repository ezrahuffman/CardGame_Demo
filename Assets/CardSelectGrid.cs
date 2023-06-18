using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CardSelectGrid : MonoBehaviour
{
    [SerializeField] List<CardData> _availableCards;
    [SerializeField] Transform _panelTrans;
    [SerializeField] GameObject _cardSelectElementPrefab;
    [SerializeField] int maxDeckSize = 10;
    [SerializeField] TMP_Text _cardCount;
    [SerializeField] CharacterSelectReady _characterSelectReady;

    // TODO: remove serialization, only serialized for easier debugging
    [SerializeField] List<CardData> _playerCards;

    public delegate void OnDeckChange(bool isFull);
    public OnDeckChange onDeckChange;

    private GameObject _selectedDeckGO;
    private int _currWins = 0;

    // Get Cards From Collection
    private void Awake()
    {
        // TODO: Replace this with cloud data
        _availableCards = new List<CardData>() {};
        _availableCards = Resources.LoadAll<CardData>("ScriptableCards/").ToList();
        _cardCount.text = $"0/{maxDeckSize}";
        onDeckChange += _characterSelectReady.SetHasPickedDeck;

        CreateSelectedDeckObject();

        // This also shows the cards after awaited call is returned 
        SetCurrentWinsFromCloudSave();
    }


    //TODO: check that waiting for a return from cloudSave isn't completely detrimental to the games performance
    private async void SetCurrentWinsFromCloudSave()
    {
        CloudSaveClient cloudSaveClient = new CloudSaveClient();
        _currWins = await cloudSaveClient.Load<int>("winCount");


        ShowCards();
    }

    private void CreateSelectedDeckObject()
    {
        _selectedDeckGO = new GameObject("selectedDeck");
        _selectedDeckGO.AddComponent<CardList>().cards = _playerCards;
        DontDestroyOnLoad(_selectedDeckGO); 
        
    }

    // Display Cards
    public void ShowCards()
    {
        Debug.Log($"currWins = {_currWins}");

        foreach (var cardData in _availableCards)
        {
            GameObject cardUIElement = Instantiate(_cardSelectElementPrefab, _panelTrans);
            CardSelectElement cardElement = cardUIElement.GetComponent<CardSelectElement>();
            cardElement.grid = this;
            cardElement.PopulateData(cardData); 
        }
    }

    // Track Player Cards

    public void ToggleCard(CardData cardData, GameObject cardObject)
    {
        if (_playerCards.Contains(cardData))
        {
            RemoveCard(cardData, cardObject);
        }
        else
        {
            AddCard(cardData, cardObject);
        }
    }

        // Add cards to deck
    public void AddCard(CardData cardData, GameObject cardObject)
    {
        // TODO: it might be a good idea to have multiples of some cards 
        // TODO: (Optimization) dictionary/hashset would have faster look ups
        // Don't do anything if we already have the card
        if (_playerCards.Contains(cardData))
        {
            return;
        }
        _playerCards.Add(cardData);

        ShowCardAsSelected(cardObject);
        //_availableCards.Remove(cardData);

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
    public void RemoveCard(CardData cardData, GameObject cardObject)
    {
        // TODO: it might be a good idea to have multiples of some cards *this might not apply here
        // TODO: (Optimization) dictionary/hashset would have faster look ups
        // Don't do anything if we don't have the card
        if (!_playerCards.Contains(cardData))
        {
            return;
        }
        _playerCards.Remove(cardData);

        ShowCardAsUnselected(cardObject);

        CheckForFullDeck();
    }

    // Send out update when deck is complete
    void CheckForFullDeck()
    {
        _cardCount.text = $"{_playerCards.Count}/{maxDeckSize}";

        _selectedDeckGO.GetComponent<CardList>().cards = _playerCards;

        if(_playerCards.Count == maxDeckSize)
        {
            onDeckChange?.Invoke(true); 
            return;
        }

        onDeckChange?.Invoke(false);
    }
}
