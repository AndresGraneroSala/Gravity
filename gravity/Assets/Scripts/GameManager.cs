using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif



public class GameManager : MonoBehaviour
{
    
    public static GameManager Instance;

#if UNITY_EDITOR
    [SerializeField] private SceneAsset[] levels; // Solo visible y usable en el editor
#endif

    [SerializeField, HideInInspector] private string[] levelPaths; // Usado en runtime (builds)

    private int _currentLevel = 0;

    [SerializeField] private GameObject menuUI;

    [SerializeField] private GameObject butNextLevel;
    [SerializeField] private GameObject butResume;

    [SerializeField] private Button butReset;
    private bool isLoose = false;
    
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern int IsMobile();
#endif

    public static bool IsMobileDevice()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return IsMobile() == 1;
#else
        // En plataforma local, puedes usar esto como fallback
        return Application.isMobilePlatform;
#endif
    }
    
    private EventSystem _eventSystem;
    
    
    private void Awake()
    {
        FindEventSystem();
        menuUI.SetActive(false);
        
        Application.targetFrameRate = 145;

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void FindEventSystem()
    {
        _eventSystem = EventSystem.current;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (levels == null)
        {
            levelPaths = null;
            return;
        }

        levelPaths = new string[levels.Length];
        for (int i = 0; i < levels.Length; i++)
        {
            var scene = levels[i];
            levelPaths[i] = scene != null ? AssetDatabase.GetAssetPath(scene) : string.Empty;
        }
    }
#endif

    

    public static bool IsPaused()
    {
        return Time.timeScale == 0;
    }

    enum StateUI
    {
        Pause,
        Loose,
        Win,
        None
    }

    private void ChangeStatusButtons(StateUI state)
    {
        if (_eventSystem == null)
        {
            FindEventSystem();
        }
        
        if (state == StateUI.None)
        {
            menuUI.SetActive(false);
            Time.timeScale = 1;
            return;
        }

        menuUI.SetActive(true);
        Time.timeScale = 0;
        
        butNextLevel.SetActive(false);
        butResume.SetActive(false);

        SetNavigationInReset(state);

        switch (state)
        {
            case StateUI.Pause:
                butResume.SetActive(true);
                _eventSystem.SetSelectedGameObject(butResume);
                return;

            case StateUI.Loose:
                _eventSystem.SetSelectedGameObject(butReset.gameObject);
                return;

            case StateUI.Win:
                butNextLevel.SetActive(true);
                _eventSystem.SetSelectedGameObject(butNextLevel);
                return;
        }
    }

    private void SetNavigationInReset(StateUI state)
    {
        if (state == StateUI.None || state == StateUI.Loose)
        {
            return;
        }

        Navigation resetNavigation = butReset.navigation;

        GameObject but = state == StateUI.Pause ? butResume.gameObject : butNextLevel.gameObject;

        Button tempButUpDown = but.gameObject.GetComponent<Button>();
        resetNavigation.selectOnUp = tempButUpDown;
        resetNavigation.selectOnDown = tempButUpDown;
        butReset.navigation = resetNavigation;
    }

    public void TogglePause()
    {
        if (isLoose)return;
        
        if (Time.timeScale == 0)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    private void Pause()
    {
        ChangeStatusButtons(StateUI.Pause);
        
    }

    private void Resume()
    {
        ChangeStatusButtons(StateUI.None);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        ChangeStatusButtons(StateUI.None);
        isLoose = false;
    }

    public void LooseGame()
    {
        ChangeStatusButtons(StateUI.Loose);
        isLoose = true;
    }

    public void WinGame()
    {
        ChangeStatusButtons(StateUI.Win);
    }

    public void NextLevel()
    {
        _currentLevel++;
        if (_currentLevel < levelPaths.Length)
            LoadLevelByPath(levelPaths[_currentLevel]);
        else
            _currentLevel = 0;
        LoadLevelByPath(levelPaths[_currentLevel]);
        ChangeStatusButtons(StateUI.None);
    }

    public void LoadLevel(int posLevel)
    {
        if (posLevel >= 0 && posLevel < levelPaths.Length)
        {
            _currentLevel = posLevel;
            LoadLevelByPath(levelPaths[_currentLevel]);
        }
        else
        {
            Debug.LogError("Índice de nivel fuera de rango.");
        }
    }

    private void LoadLevelByPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("Ruta de escena vacía.");
            return;
        }

        string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
        SceneManager.LoadScene(sceneName);
    }
}