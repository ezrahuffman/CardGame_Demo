using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class TestLobby : MonoBehaviour
{
    public string profileName;

    string _lobbyName = "MyLobby";
    int _maxNumberOfPlayers = 2;

    private Lobby _hostLobby;
    private Lobby _joinedLobby;
    private float _heartBeatTimer;
    private float _heartBeatTimerMax = 15f; //Lobbies will time out if there is no activity for 30s

    private string _lobbyCode;
    private float _pollForUpdateTimer;
    private float _pollForUpdateTimerMax = 1.1f; //Limit for poll for updates is 1s
    private string _upateGameModeTextInput;

    private async void Start()
    {

        var options = new InitializationOptions();
        options.SetProfile(profileName);

        await UnityServices.InitializeAsync(options);
        //AuthenticationService.Instance.ClearSessionToken();
        SignInOptions signInOptions = new SignInOptions();
        signInOptions.CreateAccount = true;
        await AuthenticationService.Instance.SignInAnonymouslyAsync(signInOptions);
        Debug.Log($"profile: {AuthenticationService.Instance.Profile}");
        //await AuthenticationService.Instance.si


        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log($"Signed in {AuthenticationService.Instance.PlayerId}");
        };

        ////TODO: Use authentication
        //await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }
    private void Update()
    {
        HandleLobbyHeartBeat();
        HandleLobbyPollForUpdates();
    }

    private async void HandleLobbyHeartBeat()
    {
        if(_hostLobby == null)
        {
            return;
        }

        _heartBeatTimer -= Time.deltaTime;
        if(_heartBeatTimer <= 0)
        {
            _heartBeatTimer = _heartBeatTimerMax;

            await LobbyService.Instance.SendHeartbeatPingAsync(_hostLobby.Id);
        }
    }
    private async void HandleLobbyPollForUpdates()
    {
        if (_joinedLobby == null)
        {
            return;
        }

        _pollForUpdateTimer -= Time.deltaTime;
        if (_pollForUpdateTimer <= 0)
        {
            _pollForUpdateTimer = _pollForUpdateTimerMax;

            Lobby lobby = await LobbyService.Instance.GetLobbyAsync(_joinedLobby.Id);
            _joinedLobby = lobby;
        }
    }

    public async void CreateLobby()
    {
        try
        {
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = CreatePlayerObject(),
                Data = new Dictionary<string, DataObject> {
                    {"GameMode", new DataObject(DataObject.VisibilityOptions.Public, "Duel", DataObject.IndexOptions.S1)}
                },
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(_lobbyName, _maxNumberOfPlayers, lobbyOptions);

            _hostLobby = lobby;
            _joinedLobby = _hostLobby;

            PrintPlayers(_hostLobby);

            Debug.Log($"Created Lobby! {lobby.Name} with {lobby.MaxPlayers} max players, lobbyId: {lobby.Id}, lobbyCode: {lobby.LobbyCode}");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void ListLobbies()
    {
        try
        {

            // TODO: Might be a good idea to expose some of these options to the player
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
                    new QueryFilter(QueryFilter.FieldOptions.S1, "Duel", QueryFilter.OpOptions.EQ),
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            Debug.Log($"Found {queryResponse.Results.Count} lobbies");
            foreach (var lobby in queryResponse.Results)
            {
                Debug.Log($"lobby: {lobby.Name}, GameMode: {lobby.Data["GameMode"].Value}| maxPlayers: {lobby.MaxPlayers}, currPlayers: {lobby.Players.Count}");

            }
        } 
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void JoinLobbyByCode()
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = CreatePlayerObject()
            };


            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(_lobbyCode, joinLobbyByCodeOptions);
            _joinedLobby = lobby;

            PrintPlayers(lobby);
            Debug.Log($"joinedLoby with: {_lobbyCode}");
            
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    
    public async void QuickJoinLobby()
    {
        try
        {
            QuickJoinLobbyOptions quickJoinLobbyOptions = new QuickJoinLobbyOptions
            {
                Player = CreatePlayerObject()
            };

            //TODO: could use filters for this just like the other lobbies. Maybe not though
            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);
            _joinedLobby = lobby;
            PrintPlayers(lobby);
        } 
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log($"Players in {lobby.Data["GameMode"].Value} lobby: {lobby.Name}");
        foreach (var player in lobby.Players)
        {
            Debug.Log($"id: {player.Id}, name: {player.Data["PlayerName"].Value}");
        }
    }

    public void PrintCurrentLobby()
    {
        PrintPlayers(_joinedLobby);
    }

    private Player CreatePlayerObject()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject> {
                        {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, AuthenticationService.Instance.Profile) }
                    }
        };
    }

    // NOTE: You only need to update the values that are being updated, the rest of the lobby details will automatically be coppied
    // NOTE: Updating the player is almost exactly the same process
    // NOTE: You can update the host this way as well, but it is handled automatically by lobby system anyways
    public async void UpdateLobbyGameMode()
    {
        try
        {
            _hostLobby = await Lobbies.Instance.UpdateLobbyAsync(_hostLobby.Id, new UpdateLobbyOptions
            {
                    Data = new Dictionary<string, DataObject>
                {
                    {"GameMode", new DataObject(DataObject.VisibilityOptions.Public, _upateGameModeTextInput) }
                }
            });
            _joinedLobby = _hostLobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    // Note: Lobby system will auto migrate the host if the host leaves
    public void LeaveLobby()
    {
        if(_joinedLobby == null)
        {
            return;
        }

        try
        {
            RemovePlayerFromLobby(AuthenticationService.Instance.PlayerId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void RemovePlayerFromLobby(string playerId)
    {
        await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id, playerId);
    }

    public void KickPlayer(string playerId)
    {
        try
        {
            // Only the host can kick players from the lobby
            if (_joinedLobby.HostId == AuthenticationService.Instance.PlayerId)
            {
                RemovePlayerFromLobby(playerId);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

    }

    public void SetLobbyCode(string newCode)
    {
        _lobbyCode = newCode;
    }

    public void SetGameModeText(string newGameMode)
    {
        _upateGameModeTextInput = newGameMode;
    }
}
