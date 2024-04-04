using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SinglePlayerUI : MonoBehaviour
{


    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button mainMenuButton;


    private void Awake()
    {
        newGameButton.onClick.AddListener(() => {
            Loader.Load(Loader.Scene.SinglePlayerGameScene);
        });

        loadGameButton.onClick.AddListener(() => {
            Debug.Log("load game");
            //Loader.Load(Loader.Scene.SinglePlayerStartScene);
        });

        mainMenuButton.onClick.AddListener(() => {
            Loader.Load(Loader.Scene.MainMenuScene);
        });

        Time.timeScale = 1f;
    }

    private void Start()
    {
        Application.targetFrameRate = 30;
    }

}
