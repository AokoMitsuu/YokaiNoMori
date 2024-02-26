using System;
using UnityEngine;
using Managers;

public class App : MonoBehaviour
{
    #region Singleton
    public static App Instance { get; private set; }

    protected void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            Load();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    #endregion

    [Header("Managers")]
    [SerializeField] private AudioManager m_AudioManager;
    [SerializeField] private SceneManager m_SceneManager;
    [SerializeField] private TransitionManager m_TransitionManager;

    public AudioManager AudioManager { get { return m_AudioManager; } }
    public SceneManager SceneManager { get { return m_SceneManager; } }
    public TransitionManager TransitionManager { get { return m_TransitionManager; } }

    private void Load()
    {
        //Random
        UnityEngine.Random.InitState((int)DateTime.Now.Ticks);

        //App settings
        Application.targetFrameRate = 144;
        QualitySettings.vSyncCount = 0;
    }
}