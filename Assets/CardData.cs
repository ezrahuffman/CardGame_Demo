using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "CardData", menuName = "ScriptableObjects/CardData", order = 1)]
public class CardData : ScriptableObject, INetworkSerializeByMemcpy
{
    public string cardName;
    public CardType cardType;
    public GamePlayer owningPlayer { get; private set; }
    public int winsToUnlock = 0;
    [SerializeField]
    internal float effectAmnt;
    [SerializeField]
    internal float health;
    
    public Sprite cardSprite;
    public int selectionIndex;

    // TODO: Remove this
    public CardDataStruct dataStruct;

    internal void SetOwningPlayer(GamePlayer owningPlayer)
    {
        this.owningPlayer = owningPlayer;
    }

    private void Awake()
    {
        dataStruct = new CardDataStruct();
    }
}

[System.Serializable]
public enum CardType { Dmg, Heal }
