using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class CardStructList : INetworkSerializeByMemcpy
{
    List<CardData> cards;
}

public class SerializableCardList : INetworkSerializeByMemcpy
{
    public List<CardData> cards;

    public SerializableCardList(List<CardData> cards)
    {
        this.cards = cards;
    }
}
