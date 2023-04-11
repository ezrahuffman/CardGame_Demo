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
        _endGameMessageText.text = NetworkManager.Singleton.LocalClientId == winnerClientId ? _winningMessage : _losingMessage;
    }
}
