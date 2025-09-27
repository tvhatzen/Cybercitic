using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : SingletonBase<UIManager>
{
    public enum MenuScreen
    {
        MainMenu,
        Pause,
        Upgrade,
        Results,
        WinGame,
        LoseGame,
        Gameplay,
        None
    }

    public MenuScreen screenState;

    [Header("UI Objects")]
    public GameObject mainMenuUI;
    public GameObject pauseUI;
    public GameObject upgradeUI;
    public GameObject resultsUI;
    public GameObject gameplayUI;
    public GameObject winUI;
    public GameObject loseUI;
    public GameObject noneUI;

    private MenuScreen currentScreen = MenuScreen.None;

    protected override void Awake()
    {
        base.Awake();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // By default show gameplay HUD in all gameplay scenes named Level
        if (scene.name.StartsWith("Level")) { ShowScreen(MenuScreen.Gameplay); }
        else if (scene.name == "MainMenu") { ShowScreen(MenuScreen.MainMenu); }
        else if (scene.name == "GameWin") { ShowScreen(MenuScreen.WinGame); }
        else if (scene.name == "GameOver") { ShowScreen(MenuScreen.LoseGame); }
        else { ShowScreen(MenuScreen.None); }
    }
    
    public void ShowScreen(MenuScreen screen)
    {
        // hide all
        HideAllUI(noneUI);

        // show chosen
        switch (screen)
        {
            case MenuScreen.MainMenu: HideAllUI(mainMenuUI); ; break;
            case MenuScreen.Pause: HideAllUI(pauseUI); ; break;
            case MenuScreen.Upgrade: HideAllUI(upgradeUI); ; break;
            case MenuScreen.Results: HideAllUI(resultsUI); ; break;
            case MenuScreen.Gameplay: HideAllUI(gameplayUI); ; break;
            case MenuScreen.WinGame: HideAllUI(winUI); ; break;
            case MenuScreen.LoseGame: HideAllUI(loseUI); ; break;
            case MenuScreen.None: break;
        }

        currentScreen = screen;
    }

    public void TogglePauseMenu()
    {
        if (currentScreen == MenuScreen.Pause)
        {
            ShowScreen(MenuScreen.Gameplay);
            Time.timeScale = 1f;
        }
        else
        {
            ShowScreen(MenuScreen.Pause);
            Time.timeScale = 0f;
        }
    }

    public void HideAllUI(GameObject ActiveUI)
    {
        resultsUI.SetActive(false);
        upgradeUI.SetActive(false);
        mainMenuUI.SetActive(false);
        gameplayUI.SetActive(false);
        pauseUI.SetActive(false);
        loseUI.SetActive(false);
        winUI.SetActive(false);
        noneUI.SetActive(false);
        ActiveUI.SetActive(true);
    }
}