using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {

    [SerializeField]
    GameObject UserInterface;

    [SerializeField]
    GameObject Menu;

    [SerializeField]
    Image Background;

    bool menuVisible;

    [SerializeField] GameManager Game;

    [HideInInspector]
    public Controller playerController;

    ShowHideMenu showMenu;

    void Awake()
    {


        if(Game == null)
        {
            Game = FindObjectOfType<GameManager>();
        }
        /*
        if(UserInterface == null)
        {
            UserInterface = FindObjectOfType<HUD>().gameObject;
        }
        //Game.OnGameOver += ExitToMainMenu;
        if(UserInterface)
            UserInterface.SetActive(true);*/
    }

    void Start()
    {
        CreateUI();
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    void CreateUI()
    {
        GameObject _interface = Instantiate(UserInterface, Vector3.zero, Quaternion.identity);
        HUD _hud = _interface.GetComponent<HUD>();
        if (_hud)
        {
            _hud.PlayerController = playerController;
            UserInterface = _interface;
        }

        GameObject _escapeMenu = Instantiate(Menu, Vector3.zero, Quaternion.identity);
        Menu = _escapeMenu;

        showMenu = Menu.GetComponent<ShowHideMenu>();
        showMenu.MenuVisible(menuVisible);
        showMenu.manager = this;

        //Menu.SetActive(menuVisible);

        if(Background)
            Background.CrossFadeAlpha(0, 1f, true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        menuVisible = !menuVisible;
        //Menu.SetActive(menuVisible);
        showMenu.MenuVisible(menuVisible);
        UserInterface.SetActive(!menuVisible);

        Cursor.visible = menuVisible;

        if (menuVisible)
        {
            Background.CrossFadeAlpha(.5f, 1, true);
            Time.timeScale = 0;
        }
        else
        {
            Background.CrossFadeAlpha(0, 1f, true);
            Time.timeScale = 1;
        }
    }

    public void ExitToMainMenu()
    {
        SceneLoader.LoadMainMenu();
        /*
        //Debug.Log("Button was clicked");
        Time.timeScale = 1;
        SceneManager.LoadScene(0);*/
    }
}
