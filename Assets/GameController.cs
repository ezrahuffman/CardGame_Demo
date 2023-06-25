using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Multiplay;
using Unity.Services.Authentication;
using System.Linq;

public class GameController : NetworkBehaviour
{
    private const string PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER = "PlayerNameMultiplayer";
    public const int MAX_PLAYER_AMOUNT = 2; // TODO: this is duplicated in one of the lobby scripts

    [SerializeField] private Card[] _currentCards;
    [SerializeField] private Card[] _deck;

    private List<CardList> _selectedDecks;

    // TODO: it is assumed that there are only 2 players but this need to be enforced somewhere
    private List<GamePlayer> _players;

    #region Singleton
    public static GameController instance;

    private GamePlayer _winningPlayer;

    private NetworkList<PlayerData> playerDataNetworkList;
    private string playerName;

    // TODO: Look into a way to only have the game controller on the Server
    private void Awake()
    {
        //if (!IsServer)
        //{
        //    Destroy(this);
        //    return; 
        //}

        _players = new List<GamePlayer>();

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("TRYING TO MAKE DUPLICATE SINGLETON");
            //Destroy(this);
            return;
        }

        // Use the game controller as a way to maintain state between game and end menu
        // TODO: look into better ways to maintain states
        DontDestroyOnLoad(this);

        playerName = PlayerPrefs.GetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, "PlayerName" + UnityEngine.Random.Range(100, 1000));

        playerDataNetworkList = new NetworkList<PlayerData>();
        _selectedDecks = new List<CardList>();
        playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;
    }
    #endregion

    public void SetPlayerName(string playerName)
    {
        this.playerName = playerName;

        PlayerPrefs.SetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, playerName);
    }

    public void StartServer()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartServer();
    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
    {
        if (SceneManager.GetActiveScene().name != Loader.Scene.CharacterSelectScene.ToString())
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game has already started";
            return;
        }

        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYER_AMOUNT)
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game is full";
            return;
        }

        connectionApprovalResponse.Approved = true;
    }

    public bool HasAvailablePlayerSlots()
    {
        return NetworkManager.Singleton.ConnectedClientsIds.Count < MAX_PLAYER_AMOUNT;
    }

    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
    }
    private async void ReadyServer()
    {
        await MultiplayService.Instance.ReadyServerForPlayersAsync();
    }

    public bool IsPlayerIndexConnected(int playerIndex)
    {
        return playerIndex < playerDataNetworkList.Count;
    }

    public PlayerData GetPlayerDataFromPlayerIndex(int playerIndex)
    {
        return playerDataNetworkList[playerIndex];
    }

    // Logic to run when we load into a particular scene
    private void OnLevelWasLoaded(int level)
    {
        // Start the game when we load into the game scene
        if (SceneManager.GetActiveScene().name == Loader.Scene.GameScene.ToString() && IsServer)
        {
            foreach (var playerData in playerDataNetworkList) {
                ulong clientId = playerData.clientId; 

                //Spawn Player and assign owner to each client
                GameObject go = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
                go.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
            }


            // Update the UI and set the first turn to start the game
            // TODO: This might just be in the wrong order
            UpdateUI();
            SetFirstTurn();
        }
    }


    public void KickPlayer(ulong clientId)
    {
        NetworkManager.Singleton.DisconnectClient(clientId);
        NetworkManager_Server_OnClientDisconnectCallback(clientId);
    }

    private void NetworkManager_Server_OnClientDisconnectCallback(ulong clientId)
    {
        Debug.Log("Client Disconnected " + clientId);

        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            PlayerData playerData = playerDataNetworkList[i];
            if (playerData.clientId == clientId)
            {
                // Disconnected!
                playerDataNetworkList.RemoveAt(i);
            }
        }

        Debug.Log("THERE SHOULD BE A LOG MESSAGE BELOW THIS ON THE DEDICATED SERVER\n-------------------------------------\n------------------------------------");
#if DEDICATED_SERVER
        Debug.Log("playerDataNetworkList.Count " + playerDataNetworkList.Count);
        if (SceneManager.GetActiveScene().name == Loader.Scene.GameScene.ToString()) {
            // Player leaving during GameScene
            if (playerDataNetworkList.Count <= 0) {
                // All players left the game
                Debug.Log("All players left the game");
                Debug.Log("Shutting Down Network Manager");
                NetworkManager.Singleton.Shutdown();
                Application.Quit();
                //Debug.Log("Going Back to Main Menu");
                //Loader.Load(Loader.Scene.MainMenuScene);
            }
        }
#endif
    }

    private void NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        Debug.Log("Client Connected " + " " + clientId);

        playerDataNetworkList.Add(new PlayerData
        {
            clientId = clientId,
            colorId = -1, // TODO: Remove colors completely, or implement them
        });

        CreateCardList(clientId);

        if (playerDataNetworkList.Count >= 2)
        {
            SpawnCardLists();
            AssignCardListsClientRpc();
        }

#if !DEDICATED_SERVER
        SetPlayerNameServerRpc(GetPlayerName());
        SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);
#endif
    }

    [ClientRpc]
    private void AssignCardListsClientRpc()
    {
        _selectedDecks = FindObjectsOfType<CardList>().ToList();

        foreach (var cardList in _selectedDecks)
        {
            List<CardData> availalbeCards = Resources.LoadAll<CardData>("ScriptableCards/").ToList();
            cardList.Assign(availalbeCards);
        }
    }

    private void SpawnCardLists()
    {
        foreach (var cardList in _selectedDecks)
        {
            Debug.Log("SPAWN CARD LIST");
            cardList.GetComponent<NetworkObject>().SpawnWithOwnership(cardList.clientId, false);
        }
    }

    public void CreateCardList(ulong clientId)
    {
        Debug.Log("Create Card List");
        //Spawn Player and assign owner to each client
        GameObject go = Instantiate(cardListPrefab, Vector3.zero, Quaternion.identity);
        //go.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);

        CardList cardList = go.GetComponent<CardList>();
        cardList.clientId = clientId;

        // TODO see if resources load still works
        //CardData[] availableCards = Resources.LoadAll<CardData>("ScriptableCards/");
        // TODO: this should be cached instead of pulling from resource folder
        //cardList.Assign(clientId, availableCards.ToList());
        _selectedDecks.Add(cardList);

    }

    public void AddPlayer(GamePlayer player)
    {
        Debug.Log("ADD PLAYER TO GAME CONTROLLER");

        _players.Add(player);
        if (IsServer)
        {
            player.OnDieEvent = OnPlayerDie;
            player.onTurnOver += OnPlayerTurnOver;
        }
    }

    

    void OnPlayerTurnOver(GamePlayer player)
    {
        GamePlayer nextPlayer = GetOtherPlayer(player.NetworkObject);
        nextPlayer.canPlay = true;

        TurnOverClientRpc(nextPlayer.OwnerClientId);
    }

    [ClientRpc]
    void TurnOverClientRpc(ulong nextPlayerOwnerClientID)
    {
        Debug.Log($"Set player can play: {nextPlayerOwnerClientID}");
        GamePlayer nextPlayer = _players[0].OwnerClientId == nextPlayerOwnerClientID ? _players[0] : _players[1];
        nextPlayer.canPlay = true;

        if (IsOwner)
        {
            UpdateUIServerRpc();
        }
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
    GamePlayer GetOtherPlayer(NetworkObject sender)
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

        Debug.Log("Update UI");

        CallUpdateUIOnAllPlayers();

        UpdateUIClientRpc();
       
    }

    [ServerRpc]
    void UpdateUIServerRpc()
    {
        UpdateUI();
    }

    void CallUpdateUIOnAllPlayers()
    {
        foreach (var player in _players)
        {
            GamePlayer otherPlayer = GetOtherPlayer(player.NetworkObject);
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


        GamePlayer reciever = _players[0].OwnerClientId == senderClientId ? _players[1] : _players[0];

        reciever.TakeDmg(effectAmnt);

        DealDmgClientRpc(senderClientId, effectAmnt); 
    }

    [ClientRpc]
    public void DealDmgClientRpc(ulong senderClientId, float effectAmnt)
    {
        GamePlayer reciever = _players[0].OwnerClientId == senderClientId ? _players[1] : _players[0];

        reciever.TakeDmg(effectAmnt);
    }

    #endregion

    public List<GamePlayer> Players
    {
        get
        {
            return _players;
        }
    }

    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject cardListPrefab;

    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailedToJoinGame;
    public event EventHandler OnPlayerDataNetworkListChanged;

    private async void UnreadyServer()
    {
        await MultiplayService.Instance.UnreadyServerAsync();
    }

    public void StartClient()
    {
        OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);

        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnClientDisconnectCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Client_OnClientConnectedCallback;
        NetworkManager.Singleton.StartClient();
    }

    private void NetworkManager_Client_OnClientDisconnectCallback(ulong clientId)
    {
        OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
    }

    private void NetworkManager_Client_OnClientConnectedCallback(ulong clientId)
    {
        SetPlayerNameServerRpc(GetPlayerName());
        SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);
    }

    

    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartHost();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerIdServerRpc(string playerId, ServerRpcParams serverRpcParams = default)
    {
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        PlayerData playerData = playerDataNetworkList[playerDataIndex];

        playerData.playerId = playerId;

        playerDataNetworkList[playerDataIndex] = playerData;
    }

    public NetworkList<PlayerData> GetPlayerDataNetworkList()
    {
        return playerDataNetworkList;
    }


    public string GetPlayerName()
    {
        return playerName;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerNameServerRpc(string playerName, ServerRpcParams serverRpcParams = default)
    {
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        PlayerData playerData = playerDataNetworkList[playerDataIndex];

        playerData.playerName = playerName;

        playerDataNetworkList[playerDataIndex] = playerData;
    }
    public int GetPlayerDataIndexFromClientId(ulong clientId)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientId == clientId)
            {
                return i;
            }
        }
        return -1;
    }

    [ServerRpc]
    private void UnreadyServer_ServerRpc()
    {
        UnreadyServer();
    }

    internal void SetFirstTurn()
    {
        Debug.Log("Set First Turn");

        // Randomly select a player
        UnityEngine.Random.InitState((int)Time.time);
        int index = UnityEngine.Random.Range(0, 2);
        GamePlayer startingPlayer = _players[index];

        startingPlayer.canPlay = true;


        //TODO: change name of function or use a different one
        TurnOverClientRpc(startingPlayer.OwnerClientId);
        UpdateUI();

        if (IsServer)
        {
            UnreadyServer();
        }
        else
        {
            UnreadyServer_ServerRpc();
        }
    }

    internal void UpdateCardList(ulong localClientId, List<int> playerCards)
    {
        foreach (var card in _selectedDecks) 
        {
            Debug.Log($"cardList.ownerClientId ({card.OwnerClientId})" +
               $" == localClientId({localClientId})");
            if (card.OwnerClientId == localClientId)
            {
                card.UpdateList(playerCards.ToArray());
            }
        }
    }

    [ServerRpc]
    private void UdateCardListServerRPC(ulong localClientId, int[] playerCards)
    {
        UpdateCardListLogic(localClientId, playerCards);

        UpdateCardListClientRpc(localClientId, playerCards);
    }

    [ClientRpc]
    private void UpdateCardListClientRpc(ulong localClientId, int[] playerCards)
    {
        UpdateCardListLogic(localClientId, playerCards);
    }

    private void UpdateCardListLogic(ulong localClientId, int[] playerCards)
    {
        Debug.Log($"Udate Card List: {String.Join(", ", playerCards)}");

            
        foreach (var cardList in _selectedDecks)
        {
            Debug.Log($"cardList.ownerClientId ({cardList.OwnerClientId})" +
                $" == localClientId({localClientId})");
            if(cardList.OwnerClientId == localClientId)
            {
                Debug.Log("actually update");
                cardList.UpdateList(playerCards);
            }
        }
    }
}
