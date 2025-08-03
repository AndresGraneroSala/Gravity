using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;

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
    
    
    private void Awake()
    {
        
        Application.targetFrameRate = 145;

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
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

    

    public bool isPaused()
    {
        return Time.timeScale == 0;
    }

    public void TogglePause()
    {
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
        Time.timeScale = 0;
    }

    private void Resume()
    {
        Time.timeScale = 1;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LooseGame()
    {
        // Implementar lógica de derrota
    }

    public void WinGame()
    {
        // Implementar lógica de victoria
    }

    public void NextLevel()
    {
        _currentLevel++;
        if (_currentLevel < levelPaths.Length)
            LoadLevelByPath(levelPaths[_currentLevel]);
        else
            Debug.LogWarning("No hay más niveles.");
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

    public void OpenMenu()
    {
        // Implementar lógica de menú
    }

    private void Update()
    {
        // Opcional: lógica de actualización
    }
}