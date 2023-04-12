using UnityEngine;
using TMPro;
using Unity.Netcode;
using System;

[System.Serializable]
public class Card : MonoBehaviour
{
    [SerializeField]protected float _effectAmnt;
    [SerializeField]protected float _health;

    [SerializeField] protected TMP_Text healthText;
    [SerializeField] protected TMP_Text cardName;

    private GameController _gameController;


    [SerializeField]
    private CardData _data = null;
    private CardType _cardType;
    private string _cardName;

    [SerializeField]
    private Player _owningPlayer;

    private void Awake()
    {
        // if no data, hide card
        if (_data == null)
        {
            gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        if(GameController.instance != null)
        {
            _gameController = GameController.instance;
        }

        
    }


    public virtual void Action()
    {
        // Don't do anything if we can't play
        if (!_owningPlayer.canPlay || _owningPlayer.HasPlayed)
        {
            return;
        }

        switch (_cardType)
        {
            case CardType.Dmg:
                _owningPlayer.DealDmg( _effectAmnt);
                break;
            case CardType.Heal:
                break;
            default:
                Debug.LogError($"Unrecognized card type: {_cardType}");
                break;
        }

        _owningPlayer.HasPerformedAction(this);
    }

    public virtual void Discard()
    {
        _owningPlayer.Discard(this);

        //DiscardLogic();

        
        //DiscardServerRpc();
        
    }

    // TODO: create and store actual default values
    internal void Reset()
    {
        _data = null;
        _effectAmnt = 0;
        _cardType = 0;
        _cardName = "";
        _health = 0;
    }

    private void PopulateUI(CardData data)
    {
        cardName.text = data.cardName;
        healthText.text = data.health.ToString();

        gameObject.SetActive(true);
    }

    public void PopulateData(CardData data)
    {
        if(data == null)
        {
            Debug.Log("Trying to update card with empty data");
            gameObject.SetActive(false); 
            return;
        }

        PopulateUI(data);

        _data = data;
        _effectAmnt = data.effectAmnt;
        _cardType = data.cardType;
        _cardName = data.cardName;
        _health = data.health;
    }

    public CardData GetCardData()
    {
        return _data;
    }

    public void SetOwningPlayer(Player player)
    {
        _owningPlayer = player;
    }
}
