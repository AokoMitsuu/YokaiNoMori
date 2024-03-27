using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class TitleManager : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown m_GamemodeDropdown;

    private List<string> m_Gamemodes = new();

    private void SelectGamemode(int pIndex)
    {
        App.Instance.SelectedGamemode = (App.Gamemode)pIndex;
    }

    protected void Awake()
    {
        m_GamemodeDropdown.ClearOptions();
        m_Gamemodes = Enum.GetNames(typeof(App.Gamemode)).Select(item => item.Replace('_', ' ')).ToList();
        m_GamemodeDropdown.AddOptions(m_Gamemodes);

        m_GamemodeDropdown.onValueChanged.AddListener(SelectGamemode);
    }
}
