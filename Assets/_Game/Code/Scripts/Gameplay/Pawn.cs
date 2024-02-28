using UnityEngine;
using UnityEngine.UI;

public class Pawn : MonoBehaviour
{
    [SerializeField] private Image m_Image;

    private PawnSo m_PawnSo;
    private Team m_Team;

    public PawnSo PawnSo => m_PawnSo;
    public Team Team
    {
        get => m_Team;
        set
        {
            m_Team = value;
            transform.eulerAngles = value == Team.Player1 ? new Vector3(0f, 0f, 0f) : new Vector3(0f, 0f, 180f);
        }
    }

    public void Init(PawnSo data, Team team)
    {
        m_PawnSo = data;
        Team = team;
        m_Image.sprite = data.Sprite;
    }
}

public enum Team
{
    None,
    Player1,
    Player2,
}