using TMPro;
using Unity.Netcode;
using UnityEngine;

public class EndScreenController : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _endGameMessageText;

    private string _winningMessage = "YOU WON!";
    private string _losingMessage = "YOU LOST";

    internal void ShowMessage(ulong winnerClientId)
    {
        Debug.Log($"winner client id: {winnerClientId}");
        bool wonGame = NetworkManager.Singleton.LocalClientId == winnerClientId;
        _endGameMessageText.text = wonGame ? _winningMessage : _losingMessage;
        if (wonGame)
        {
            AddWinInCloudSave();
        }
    }

    private async void AddWinInCloudSave()
    {
        CloudSaveClient cloudSaveClient = new CloudSaveClient();
        // NOTE (Ezra): the  Load() method below should return the default value for an int (i.e. zero) if there is not yet a value.
        // Zero as a default value should work as intended.
        int currWins = await cloudSaveClient.Load<int>("winCount");
        await cloudSaveClient.Save("winCount", currWins + 1);
    }

    public void PlayAgainOnClick()
    {
        NetworkManager.Singleton.Shutdown();
        Destroy(FindObjectOfType<NetworkManager>().gameObject);
        Destroy(FindObjectOfType<GameController>().gameObject);
        Destroy(FindObjectOfType<CardGameLobby>().gameObject);
        Loader.Load(Loader.Scene.LobbyScene);
    }

    public void ExitOnClick()
    {
        NetworkManager.Singleton.Shutdown();
        Application.Quit();
    }
}
