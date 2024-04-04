using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{


    [SerializeField] private Button playMultiplayerButton;
    [SerializeField] private Button playSingleplayerButton;
    [SerializeField] private Button quitButton;


    private void Awake()
    {
        playMultiplayerButton.onClick.AddListener(() => {
            Loader.Load(Loader.Scene.LobbyScene);
        });

        playSingleplayerButton.onClick.AddListener(() => {
            Debug.Log("play singleplayer");
            Loader.Load(Loader.Scene.SinglePlayerStartScene);
        });

        quitButton.onClick.AddListener(() => {
            Application.Quit();
        });

        Time.timeScale = 1f;
    }

    private void Start()
    {
        Application.targetFrameRate = 30;
    }

}