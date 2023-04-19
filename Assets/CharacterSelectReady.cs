using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Multiplay;
using UnityEngine;

public class CharacterSelectReady : NetworkBehaviour
{


    public static event EventHandler OnInstanceCreated;

    public static void ResetStaticData()
    {
        OnInstanceCreated = null;
    }


    public static CharacterSelectReady Instance { get; private set; }




    public event EventHandler OnReadyChanged;
    public event EventHandler OnGameStarting;


    private Dictionary<ulong, bool> playerReadyDictionary;


    private void Awake()
    {
        Instance = this;

        playerReadyDictionary = new Dictionary<ulong, bool>();

        OnInstanceCreated?.Invoke(this, EventArgs.Empty);
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    private async void Start()
    {
#if DEDICATED_SERVER
        Debug.Log("DEDICATED_SERVER CHARACTER SELECT");

        Debug.Log("ReadyServerForPlayersAsync");
        await MultiplayService.Instance.ReadyServerForPlayersAsync();

        Camera.main.enabled = false;
#endif
    }

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

    public void SetPlayerReady()
    {
        SetPlayerReadyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        SetPlayerReadyClientRpc(serverRpcParams.Receive.SenderClientId);

        Debug.Log("SetPlayerReadyServerRpc " + serverRpcParams.Receive.SenderClientId);
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                // This player is NOT ready
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady)
        {
            OnGameStarting?.Invoke(this, EventArgs.Empty);
            CardGameLobby.Instance.DeleteLobby();
            Loader.LoadNetwork(Loader.Scene.GameScene);
        }
    }

    [ClientRpc]
    private void SetPlayerReadyClientRpc(ulong clientId)
    {
        playerReadyDictionary[clientId] = true;

        OnReadyChanged?.Invoke(this, EventArgs.Empty);
    }


    public bool IsPlayerReady(ulong clientId)
    {
        return playerReadyDictionary.ContainsKey(clientId) && playerReadyDictionary[clientId];
    }

}