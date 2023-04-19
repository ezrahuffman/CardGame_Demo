using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CardData", menuName = "ScriptableObjects/CardData", order = 1)]
public class CardData : ScriptableObject
{
    public string cardName;
    public CardType cardType;
    public GamePlayer owningPlayer { get; private set; }
    [SerializeField]
    internal float effectAmnt;
    [SerializeField]
    internal float health;

    internal void SetOwningPlayer(GamePlayer owningPlayer)
    {
        this.owningPlayer = owningPlayer;
    }
}

[System.Serializable]
public enum CardType { Dmg, Heal }
