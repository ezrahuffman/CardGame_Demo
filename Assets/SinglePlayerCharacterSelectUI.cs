using UnityEngine;
using UnityEngine.UI;

public class SinglePlayerCharacterSelectUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button readyButton;


    private void Awake()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            Loader.Load(Loader.Scene.MainMenuScene);
        });

        readyButton.onClick.AddListener(() =>
        {
            Loader.Load(Loader.Scene.SinglePlayerGameScene);
        });
    }
}