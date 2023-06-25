using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class CardList : NetworkBehaviour
{
    //TODO: Hide this after debugging
    public List<CardData> cards;

    [HideInInspector] public bool hasBeenAssigned;

    private List<CardData> _availableCards;

    public ulong clientId; // just used on server

    public void Assign(List<CardData> availableCards)
    {   
        hasBeenAssigned = true;
        cards  = new List<CardData>();
        _availableCards = availableCards;
    }

    public void UpdateList(int[] indexArr)
    {
        if(!IsOwner)
        {
            Debug.Log("not owner don't update");
            return;
        }
        Debug.Log("update list");

        UpdateListServerRpc(indexArr);
    }

    [ServerRpc]
    public void UpdateListServerRpc(int[] indexArr)
    {
        // NOTE: There is no _availableCards on server
        //List<CardData> temp = new List<CardData>();
        //foreach (var index in indexArr)
        //{
        //    Debug.Log($"add card: {_availableCards[index]}");

        //    temp.Add(_availableCards[index]);
        //}

        //cards = temp;

        UpdateListClientRpc(indexArr);
    }

    [ClientRpc]
    private void UpdateListClientRpc(int[] indexArr)
    {
        List<CardData> temp = new List<CardData>();
        foreach (var index in indexArr)
        {
            Debug.Log($"add card: {_availableCards[index]}");
            temp.Add(_availableCards[index]);
        }

        cards = temp;
    }
}
