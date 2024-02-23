using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PieceSo/PieceSo", fileName = "PieceSo")]
public class PieceSo : ScriptableObject
{
    public List<Vector2> Ranges => m_Ranges;
    [SerializeField] private List<Vector2> m_Ranges;

    public Sprite Sprite => m_Sprite;
    [SerializeField] private Sprite m_Sprite;

    public PieceSo PromotedPiece => m_PromotedPiece;
    [SerializeField] private PieceSo m_PromotedPiece;
}
