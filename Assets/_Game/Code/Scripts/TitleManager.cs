using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class TitleManager : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown m_GamemodeDropdown;

    private List<string> m_Gamemodes = new();

    private void SelectGamemode(int pIndex)
    {
        App.Instance.GameMode = (App.Gamemode)Enum.Parse(typeof(App.Gamemode), m_Gamemodes[pIndex].Replace(' ', '_'));
        UpdateUI();
    }

    private void UpdateUI()
    {
        m_GamemodeDropdown.ClearOptions();
        m_Gamemodes = Enum.GetNames(typeof(App.Gamemode)).Select(item => item.Replace('_', ' ')).ToList();

        m_Gamemodes.Remove(App.Instance.GameMode.ToString());
        m_Gamemodes.Add(App.Instance.GameMode.ToString());

        m_GamemodeDropdown.AddOptions(m_Gamemodes);
    }

    protected void Awake()
    {
        UpdateUI();

        m_GamemodeDropdown.onValueChanged.AddListener(SelectGamemode);
    }
}
