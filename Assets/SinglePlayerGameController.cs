using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SinglePlayerGameController : MonoBehaviour
{
    private const string PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER = "PlayerNameMultiplayer";
    public const int MAX_PLAYER_AMOUNT = 2; // TODO: this is duplicated in one of the lobby scripts

    [SerializeField] private Card[] _currentCards;
    [SerializeField] private Card[] _deck;

    private List<CardList> _selectedDecks;

    // TODO: it is assumed that there are only 2 players but this need to be enforced somewhere
    private List<GamePlayer> _players;

    #region Singleton
    public static SinglePlayerGameController instance;

    private GamePlayer _winningPlayer;

    private string playerName;

    // TODO: Look into a way to only have the game controller on the Server
    private void Awake()
    {
        _players = new List<GamePlayer>();

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("TRYING TO MAKE DUPLICATE SINGLETON");
            return;
        }

        // Use the game controller as a way to maintain state between game and end menu
        // TODO: look into better ways to maintain states
        DontDestroyOnLoad(this);

        playerName = PlayerPrefs.GetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, "PlayerName" + UnityEngine.Random.Range(100, 1000));

        _selectedDecks = new List<CardList>();
    }
    #endregion

    public void SetPlayerName(string playerName)
    {
        this.playerName = playerName;

        PlayerPrefs.SetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, playerName);
    }






    // Logic to run when we load into a particular scene
    private void OnLevelWasLoaded(int level)
    {
        // Start the game when we load into the game scene
        if (SceneManager.GetActiveScene().name == Loader.Scene.GameScene.ToString())
        {
            // Update the UI and set the first turn to start the game
            // TODO: This might just be in the wrong order
            UpdateUI();
            SetFirstTurn();
        }
    }
   
    private void SpawnCardLists()
    {
        foreach (var cardList in _selectedDecks)
        {
            Debug.Log("SPAWN CARD LIST");

            //TODO: Spawn cards locally here
            //cardList.GetComponent<NetworkObject>().SpawnWithOwnership(cardList.clientId, false);
        }
    }

    public void CreateCardList(ulong clientId)
    {
        //Spawn Player and assign owner to each client
        GameObject go = Instantiate(cardListPrefab, Vector3.zero, Quaternion.identity);

        CardList cardList = go.GetComponent<CardList>();
        cardList.clientId = clientId;
        _selectedDecks.Add(cardList);
    }

    public void AddPlayer(GamePlayer player)
    {
        Debug.Log("ADD PLAYER TO GAME CONTROLLER");

        _players.Add(player);

        player.OnDieEvent = OnPlayerDie;
        player.onTurnOver += OnPlayerTurnOver;
    }


    // TODO: refactor this for single player
    void OnPlayerTurnOver(GamePlayer player)
    {
        //GamePlayer nextPlayer = GetOtherPlayer(player);
        //nextPlayer.canPlay = true;

        //TurnOverClientRpc(nextPlayer.OwnerClientId);
    }


    // TODO: refactor this for single player
    public void OnPlayerDie()
    {
        //TODO: Player Die
        Debug.Log("player has died");
        _winningPlayer = _players[0].Health == 0 ? _players[1] : _players[0];

        //SetWinningPlayerClientRpc(_winningPlayer.OwnerClientId);

        //NetworkManager.SceneManager.LoadScene("End Scene", LoadSceneMode.Single);
        //NetworkManager.SceneManager.OnLoadComplete += ShowEndGameMessage;
    }



    //TODO: refactor this for single player
    public void UpdateUI()
    {
        Debug.Log("UPDATE UI");

        //player.UpdateUI();
    }

 



    internal void DealDmg(float effectAmnt, ulong senderClientId)
    {

        GamePlayer reciever = _players[0].OwnerClientId == senderClientId ? _players[1] : _players[0];

        reciever.TakeDmg(effectAmnt);

    }

    public List<GamePlayer> Players
    {
        get
        {
            return _players;
        }
    }

    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject cardListPrefab;






    public string GetPlayerName()
    {
        return playerName;
    }


   



    internal void SetFirstTurn()
    {
        Debug.Log("Set First Turn");

        // Randomly select a player
        UnityEngine.Random.InitState((int)Time.time);
        int index = UnityEngine.Random.Range(0, 2);
        GamePlayer startingPlayer = _players[index];

        startingPlayer.canPlay = true;
        UpdateUI();

    }

    internal void UpdateCardList(List<int> playerCards)
    {
        foreach (var card in _selectedDecks)
        {
            card.UpdateList(playerCards.ToArray());
        }
    }

   

    

    private void UpdateCardListLogic(ulong localClientId, int[] playerCards)
    {
        Debug.Log($"Udate Card List: {String.Join(", ", playerCards)}");


        foreach (var cardList in _selectedDecks)
        {
            Debug.Log($"cardList.ownerClientId ({cardList.OwnerClientId})" +
                $" == localClientId({localClientId})");
            if (cardList.OwnerClientId == localClientId)
            {
                Debug.Log("actually update");
                cardList.UpdateList(playerCards);
            }
        }
    }
}
