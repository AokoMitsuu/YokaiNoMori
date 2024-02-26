using UnityEngine;
using UnityEngine.UI;

public class PawnController : MonoBehaviour
{
    [SerializeField] private PawnSo m_PawnSo;
    [SerializeField] private ETeam m_Team;
    [SerializeField] private Image m_Image;

    private Vector2 m_Position;

    public PawnSo PawnSo => m_PawnSo;
    public ETeam Team => m_Team;
    public Vector2 Position => m_Position;

    public void Init(ETeam team, PawnSo pawn, Vector2 pPosition)
    {
        m_Team = team;
        m_PawnSo = pawn;
        m_Image.sprite = pawn.Sprite;
        transform.eulerAngles = team == ETeam.Player1 ? new Vector3(0f, 0f, 0f) : new Vector3(0f, 0f, 180f);
        SetPosition(pPosition);
    }

    public void SetPosition(Vector2 pPosition)
    {
        m_Position = pPosition;
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