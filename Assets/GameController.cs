using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : NetworkBehaviour
{
    [SerializeField] private Card[] _currentCards;
    [SerializeField] private Card[] _deck;

    // TODO: it is assumed that there are only 2 players but this need to be enforced somewhere
    private List<Player> _players;

    #region Singleton
    public static GameController instance;

    private Player _winningPlayer;

    // TODO: Look into a way to only have the game controller on the Server
    private void Awake()
    {
        //if (!IsServer)
        //{
        //    Destroy(this);
        //    return; 
        //}

        _players = new List<Player>();

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("TRYING TO MAKE DUPLICATE SINGLETON");
            Destroy(this);
            return;
        }

        // Use the game controller as a way to maintain state between game and end menu
        // TODO: look into better ways to maintain states
        DontDestroyOnLoad(this);
    }
    #endregion

    public void AddPlayer(Player player)
    {
        _players.Add(player);
        if (IsServer)
        {
            player.OnDieEvent = OnPlayerDie;
            player.onTurnOver += OnPlayerTurnOver;
        }
    }

    void OnPlayerTurnOver(Player player)
    {
        Player nextPlayer = GetOtherPlayer(player.NetworkObject);
        nextPlayer.canPlay = true;

        TurnOverClientRpc(nextPlayer.OwnerClientId);
    }

    [ClientRpc]
    void TurnOverClientRpc(ulong nextPlayerOwnerClientID)
    {
        Player nextPlayer = _players[0].OwnerClientId == nextPlayerOwnerClientID ? _players[0] : _players[1];
        nextPlayer.canPlay = true;
    }

    public void OnPlayerDie()
    {
        if (!IsServer)
        {
            return;
        }

        //TODO: Player Die
        Debug.Log("player has died");
        _winningPlayer = _players[0].Health == 0 ? _players[1] : _players[0];

        SetWinningPlayerClientRpc(_winningPlayer.OwnerClientId);

        NetworkManager.SceneManager.LoadScene("End Scene", LoadSceneMode.Single);
        NetworkManager.SceneManager.OnLoadComplete += ShowEndGameMessage;
    }

    [ClientRpc]
    void SetWinningPlayerClientRpc(ulong winningClientId)
    {
        _winningPlayer = _players[0].OwnerClientId == winningClientId ? _players[0] : _players[1];
        NetworkManager.SceneManager.OnLoadComplete += ShowEndGameMessage;
    }

    void ShowEndGameMessage(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        FindObjectOfType<EndScreenController>()?.ShowMessage(_winningPlayer.OwnerClientId);
    }

    
   

    // Event called when player ends their turn
    //private void OnPlayerTurnOver(Player player)
    //{
    //    // Lock current player from performing actions
    //    player.canPlay = false;

    //    // Unlock other player to play
    //    GetOtherPlayer(player).canPlay = true;
    //}

    // Get the other player
    Player GetOtherPlayer(NetworkObject sender)
    {
        return sender == _players[0].NetworkObject ? _players[1] : _players[0];
    }

    #region UpdateUI

    public void UpdateUI()
    {
        if (!IsServer)
        {
            Debug.LogWarning("Attempting to call server function on client");
            return;
        }

        CallUpdateUIOnAllPlayers();

        UpdateUIClientRpc();
       
    }

    void CallUpdateUIOnAllPlayers()
    {
        foreach (var player in _players)
        {
            Player otherPlayer = GetOtherPlayer(player.NetworkObject);
            player.UpdateUI(otherPlayer);
        }
    }

    [ClientRpc]
    void UpdateUIClientRpc()
    {
        CallUpdateUIOnAllPlayers();
    }
    #endregion

    #region DealDmg
    internal void DealDmg( float effectAmnt, ulong senderClientId)
    {
        if (!IsServer)
        {
            return;
        }


        Player reciever = _players[0].OwnerClientId == senderClientId ? _players[1] : _players[0];

        reciever.TakeDmg(effectAmnt);

        DealDmgClientRpc(senderClientId, effectAmnt); 
    }

    [ClientRpc]
    public void DealDmgClientRpc(ulong senderClientId, float effectAmnt)
    {
        Player reciever = _players[0].OwnerClientId == senderClientId ? _players[1] : _players[0];

        reciever.TakeDmg(effectAmnt);
    }

    #endregion

    public List<Player> Players
    {
        get
        {
            return _players;
        }
    }

    internal void SetFirstTurn()
    {
        // Randomly select a player
        UnityEngine.Random.InitState((int)Time.time);
        int index = UnityEngine.Random.Range(0, 2);
        Player startingPlayer = _players[index];
        startingPlayer.canPlay = true;


        //TODO: change name of function or use a different one
        TurnOverClientRpc(startingPlayer.OwnerClientId);
    }
}
