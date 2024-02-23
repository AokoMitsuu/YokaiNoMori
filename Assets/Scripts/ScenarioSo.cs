using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScenarioSo/ScenarioSo", fileName = "ScenarioSo")]
public class ScenarioSo : ScriptableObject
{
    public Vector2 BoardSize => m_BoardSize;
    public List<PieceSo> Pieces => m_Pieces;

    [SerializeField] private Vector2 m_BoardSize;
    [SerializeField] private List<PieceSo> m_Pieces;
}
