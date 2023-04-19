using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{


    [SerializeField] private Button playMultiplayerButton;
    [SerializeField] private Button quitButton;


    private void Awake()
    {
        playMultiplayerButton.onClick.AddListener(() => {
            Loader.Load(Loader.Scene.LobbyScene);
        });
        
        quitButton.onClick.AddListener(() => {
            Application.Quit();
        });

        Time.timeScale = 1f;
    }

}