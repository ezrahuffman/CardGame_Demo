using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SinglePlayerGameController : MonoBehaviour
{   private CardList _cardList;

    // TODO: it is assumed that there are only 2 players but this need to be enforced somewhere
    private List<GamePlayer> _players;

    #region Singleton
    public static SinglePlayerGameController instance;

    private GamePlayer _winningPlayer;

    private string playerName;
    private SinglePlayerPlayer _player;
    private SinglePlayerAI _ai;
    public SinglePlayerAI AI
    {
        get
        {
            if (_ai == null)
            {
                _ai = FindObjectOfType<SinglePlayerAI>();
            }
            return _ai;
        }
    }

    [SerializeField] private GameObject cardListPrefab;

    // TODO: Look into a way to only have the game controller on the Server
    private void Awake()
    { 
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

    }
    #endregion

    private void Start()
    {
        List<CardData> availableCards = FindObjectOfType<SinglePlayerCardSelectGrid>().AvailableCards;
        _cardList = CreateCardList(availableCards);
        Debug.Log($"availableCards: {availableCards.Count}");
        
    }






    // Logic to run when we load into a particular scene
    private void OnLevelWasLoaded(int level)
    {
        // Start the game when we load into the game scene
        if (SceneManager.GetActiveScene().name == Loader.Scene.SinglePlayerGameScene.ToString())
        {
            //_currentCards = _cardList.c;
            Debug.Log($"cards: {_cardList.cards.Count}");



            _player = FindObjectOfType<SinglePlayerPlayer>();
            _ai = FindObjectOfType<SinglePlayerAI>();

            // TODO: subscribe to events
            //player.OnDieEvent = OnPlayerDie;
            //player.onTurnOver += OnPlayerTurnOver;

            // Update the UI and set the first turn to start the game
            // TODO: This might just be in the wrong order
            //_player.UpdateUI();
            SetFirstTurn();
        }
    }

    

    public CardList CreateCardList(List<CardData> availableCards)
    {
        //Spawn Player and assign owner to each client
        GameObject go = Instantiate(cardListPrefab, Vector3.zero, Quaternion.identity);

        CardList cardList = go.GetComponent<CardList>();
        cardList.clientId = 0;
        cardList.Assign(availableCards);
        cardList.isSinglePlayer = true;
        
        return cardList;
    }



    // TODO: refactor this for single player
    void OnTurnOver(IPlayer player)
    {
        if (player == _player)
        {
            Debug.Log("Player Turn Over");
            AI.canPlay = true;
        }
        else
        {
            Debug.Log("AI Turn Over");
            player.canPlay = true;
        }
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

    internal void DealDmg(float effectAmnt, bool isPlayer)
    {
        // reciver = health system
        HealthSystem reciever;
        if (!isPlayer)
        {
            //TODO: Deal Damage to Player
            // reciever = player.HealthSystem;
            Debug.Log("Deal Damage to Player");
            reciever = _player.HealthSystem;
        }
        else
        {
            // reciever = AI.HealthSystem;
            Debug.Log("Deal Damage to AI");
            reciever = _ai.HealthSystem;
        }

        reciever.Dmg(effectAmnt);

    }

    public List<GamePlayer> Players
    {
        get
        {
            return _players;
        }
    }

    internal void SetFirstTurn()
    {
        Debug.Log("Set First Turn");

        // Randomly select a player
        UnityEngine.Random.InitState((int)Time.time);
        int index = UnityEngine.Random.Range(0, 2);
        //GamePlayer startingPlayer = _players[index];

        if (true)//index == 0)
        {
            Debug.Log("Player Starts");
           _player.canPlay = true;
            //startingPlayer.StartTurn();
        }
        else
        {
            Debug.Log("AI Starts");
            //GamePlayer nextPlayer = GetOtherPlayer(startingPlayer);
            //nextPlayer.canPlay = true;
            //nextPlayer.StartTurn();
        }

    }

    internal void UpdateCardList(List<int> playerCards)
    {
        _cardList.UpdateList(playerCards.ToArray(), true);   
    }

    public CardList SelectedCards
    {
        get
        {
            return _cardList;
        }
    }
}
