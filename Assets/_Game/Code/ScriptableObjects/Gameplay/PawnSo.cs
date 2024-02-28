using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PawnSo", menuName = "Gameplay/PawnSo")]
public class PawnSo : ScriptableObject
{
    [SerializeField] private List<Vector2> m_Ranges;
    [SerializeField] private BackRowReachedEffect m_BackRowReachedEffect;
    [SerializeField] private Sprite m_Sprite;
    [SerializeField][ShowIf("m_BackRowReachedEffect", BackRowReachedEffect.Promote)] private PawnSo m_PromotedPawn;

    public List<Vector2> Ranges => m_Ranges;
    public BackRowReachedEffect BackRowReachedEffect => m_BackRowReachedEffect;
    public Sprite Sprite => m_Sprite;
    public PawnSo PromotedPawn => m_PromotedPawn;
}

public enum BackRowReachedEffect
{
    None,
    Promote,
    WinIfNotTakenBack
}
