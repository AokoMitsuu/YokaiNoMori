using UnityEngine;

public class PieceController : MonoBehaviour
{
    public ETeam Team => m_Team;
    [SerializeField] private ETeam m_Team;

    public PieceSo PieceSo => m_PieceSo;
    [SerializeField] private PieceSo m_PieceSo;

    public void Init(ETeam team, PieceSo piece)
    {
        m_Team = team;
        m_PieceSo = piece;
    }

    public void Promote()
    {
        if(m_PieceSo.PromotedPiece != null)
            m_PieceSo = m_PieceSo.PromotedPiece;
    }
}

public enum ETeam
{
    None,
    Player1,
    Player2,
}