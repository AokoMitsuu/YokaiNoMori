using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;
using YokaiNoMori.Enumeration;

[CreateAssetMenu(fileName = "PawnSo", menuName = "Gameplay/PawnSo")]
public class PawnSo : ScriptableObject
{
    [SerializeField] private EPawnType m_PawnType;
    [SerializeField] private List<Vector2Int> m_Ranges;
    [SerializeField] private Sprite m_Sprite;
    [SerializeField] private Effect m_Effect;
    [SerializeField][ShowIf("m_Effect", Effect.Promote)] private PawnSo m_PromotedPawn;
    [SerializeField][ShowIf("m_Effect", Effect.Demote)] private PawnSo m_DemotedPawn;

    public EPawnType PawnType => m_PawnType;
    public List<Vector2Int> Ranges => m_Ranges;
    public Sprite Sprite => m_Sprite;
    public PawnSo PromotedPawn => m_PromotedPawn;
    public PawnSo DemotedPawn => m_DemotedPawn;
}

public enum Effect
{
    None,
    Promote,
    Demote
}
