using Core;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneSwitcherSo", menuName = "Core/SceneSwitchSo")]
public class SceneSwitchSo : ScriptableObject
{
    public void LoadLocalScene(string pSceneName)
    {
        App.Instance.SceneManager.SwitchScene(pSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
