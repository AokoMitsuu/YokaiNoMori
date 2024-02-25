using UnityEngine;
using UnityEngine.UI;

public class PawnController : MonoBehaviour
{
    [SerializeField] private PawnSo m_PawnSo;
    [SerializeField] private ETeam m_Team;
    [SerializeField] private Image m_Image;

    public PawnSo PawnSo => m_PawnSo;
    public ETeam Team => m_Team;

    public void Init(ETeam team, PawnSo pawn)
    {
        m_Team = team;
        m_PawnSo = pawn;
        m_Image.sprite = pawn.Sprite;
    }

    public void Promote()
    {
        if(m_PawnSo.PromotedPawn == null) return;

        m_PawnSo = m_PawnSo.PromotedPawn;
    }
}

public enum ETeam
{
    None,
    Player1,
    Player2,
}