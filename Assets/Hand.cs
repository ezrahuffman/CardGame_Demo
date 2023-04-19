using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    [SerializeField] private GameObject _handUI;
    [SerializeField] private Card[] _currentCards;
    [SerializeField] private GamePlayer _player;

    private List<Card> _availableSlots = new List<Card>();


    // Start is called before the first frame update
    void Start()
    {
        if(_currentCards.Length == 0)
        {
            Debug.LogError("There are no cards in the hand. Make sure to set CurrentCards in the editor.");
        }

        // Update cards on screen by populating UI with data from card
        foreach (var card in _currentCards)
        {
            if (!card.isActiveAndEnabled)
            {
                _availableSlots.Add(card);
                card.SetOwningPlayer(_player);
            }
        }
    }

    internal Card GetOpenSlot()
    {
        if (_availableSlots.Count == 0)
        {
            return null;
        }

        // Update available slots
        Card slot = _availableSlots[0];
        _availableSlots.RemoveAt(0);

        return slot;
    }

    public Card[] GetAllSlots()
    {
        return _currentCards;
    }

    internal GameObject GetUI()
    {
        return _handUI;
    }

    internal void OpenSlot(Card card)
    {
        card.Reset();
        _availableSlots.Add(card);
    }
}
