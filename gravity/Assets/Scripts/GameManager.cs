using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private SceneAsset[] levels;
    private int _currentLevel = 0;
    
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
        
    }

    public void WinGame()
    {
        
    }

    public void NextLevel()
    {
        _currentLevel++;
        SceneManager.LoadScene(levels[_currentLevel].name);
    }

    public void LoadLevel(int posLevel)
    {
        _currentLevel = posLevel;
        SceneManager.LoadScene(levels[_currentLevel].name);
    }

    public void OpenMenu()
    {
        
    }
    
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
