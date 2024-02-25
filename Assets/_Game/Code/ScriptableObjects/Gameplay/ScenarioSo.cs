using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScenarioSo", menuName = "Gameplay/ScenarioSo")]
public class ScenarioSo : ScriptableObject
{
    [SerializeField] private Vector2 m_BoardSize;
    [SerializeField] private List<PawnSo> m_Pieces;

    public Vector2 BoardSize => m_BoardSize;
    public List<PawnSo> Pieces => m_Pieces;
}
