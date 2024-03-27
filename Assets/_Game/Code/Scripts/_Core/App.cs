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

    private Gamemode m_SelectedGamemode = Gamemode.PvP;

    public AudioManager AudioManager { get { return m_AudioManager; } }
    public SceneManager SceneManager { get { return m_SceneManager; } }
    public TransitionManager TransitionManager { get { return m_TransitionManager; } }

    public Gamemode SelectedGamemode { get => m_SelectedGamemode; set => m_SelectedGamemode = value; }

    private void Load()
    {
        //Random
        UnityEngine.Random.InitState((int)DateTime.Now.Ticks);

        //App settings
        Application.targetFrameRate = 144;
        QualitySettings.vSyncCount = 0;
        m_SelectedGamemode = Gamemode.PvE_Difficile;
    }

    public enum Gamemode
    {
        PvP,
        PvE_Facile,
        PvE_Moyen,
        PvE_Difficile
    }
}