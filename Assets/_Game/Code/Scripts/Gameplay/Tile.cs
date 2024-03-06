using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Tile : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] Image m_HoverImage;
    [SerializeField] Color m_BaseColor;
    [SerializeField] Color m_MoveableColor;
    [SerializeField] Color m_SelectedColor;
    [SerializeField] Color m_ReachableColor;
    [SerializeField] private bool m_IsReserve = false;

    private Team m_TeamBackRow = Team.None;
    private Pawn m_Pawn;
    private Board m_Board;

    public Team TeamBackRow => m_TeamBackRow;
    public Pawn Pawn 
    { 
        get => m_Pawn; 
        set 
        {
            m_Pawn = value;
            SetPawn();
        }
    }
    public bool IsReserve => m_IsReserve;

    public void Init(Team pTeamBackRow, Board pBoard)
    {
        m_TeamBackRow = pTeamBackRow;
        m_Board = pBoard;
        SetState(TileState.None);
    }

    public void SetState(TileState pType)
    {
        switch (pType)
        {
            case TileState.None:
                m_HoverImage.color = m_BaseColor;
                break;
            case TileState.Highlighted:
                m_HoverImage.color = m_ReachableColor;
                break;
            case TileState.Selected:
                m_HoverImage.color = m_SelectedColor;
                break;
            case TileState.Moveable:
                m_HoverImage.color = m_MoveableColor;
                break;
        }
    }

    public void Clear()
    {
        SetState(TileState.None);
    }

    private void SetPawn()
    {
        if (m_Pawn == null) return;

        m_Pawn.transform.SetParent(gameObject.transform);
        m_Pawn.transform.position = transform.position;
        m_Pawn.transform.SetAsFirstSibling();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        m_Board.OnTileClick(this, m_IsReserve);
    }
}

public enum TileState
{
    None,
    Highlighted,
    Selected,
    Moveable
}