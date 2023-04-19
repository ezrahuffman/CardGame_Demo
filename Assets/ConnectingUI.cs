using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectingUI : MonoBehaviour {



    private void Start() {
        GameController.instance.OnTryingToJoinGame += KitchenGameMultiplayer_OnTryingToJoinGame;
        GameController.instance.OnFailedToJoinGame += KitchenGameManager_OnFailedToJoinGame;

        Hide();
    }

    private void KitchenGameManager_OnFailedToJoinGame(object sender, System.EventArgs e) {
        Hide();
    }

    private void KitchenGameMultiplayer_OnTryingToJoinGame(object sender, System.EventArgs e) {
        Show();
    }

    private void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

    private void OnDestroy() {
        GameController.instance.OnTryingToJoinGame -= KitchenGameMultiplayer_OnTryingToJoinGame;
        GameController.instance.OnFailedToJoinGame -= KitchenGameManager_OnFailedToJoinGame;
    }

}