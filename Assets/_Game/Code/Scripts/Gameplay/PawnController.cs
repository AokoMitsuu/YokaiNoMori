using UnityEngine;

public class PawnController : MonoBehaviour
{
    [SerializeField] private PawnSo m_PawnSo;
    [SerializeField] private ETeam m_Team;
    [SerializeField] private SpriteRenderer m_SpriteRenderer;

    public PawnSo PawnSo => m_PawnSo;
    public ETeam Team => m_Team;

    public void Init(ETeam team, PawnSo pawn)
    {
        m_Team = team;
        m_PawnSo = pawn;
        m_SpriteRenderer.sprite = pawn.Sprite;
        transform.eulerAngles = team == ETeam.Player1 ? new Vector3(0f, 0f, 0f) : new Vector3(0f, 0f, 180f);
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