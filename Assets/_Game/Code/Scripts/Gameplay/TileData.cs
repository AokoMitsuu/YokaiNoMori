using NaughtyAttributes;
using System;
using UnityEngine;
using YokaiNoMori.Enumeration;
using YokaiNoMori.Interface;

public class TileData : IBoardCase
{
    private bool m_IsReserve = false;

    private Board m_Board;
    private ECampType m_TeamBackRow = ECampType.NONE;
    private PawnData m_PawnData;
    private Vector2Int m_Position;

    public Board Board { get => m_Board; set => m_Board = value; }
    public ECampType TeamBackRow => m_TeamBackRow;
    public PawnData PawnData => m_PawnData;
    public bool IsReserve => m_IsReserve;

    public event Action<Transform> OnPawnUpdated;

    public void SetPosition(Vector2Int Vector2Int)
    {
        m_Position = Vector2Int;
    }
    public void SetIsReserve()
    {
        m_IsReserve = true;
    }
    public void SetTeamBackRow(ECampType pTeamBackRow)
    {
        m_TeamBackRow = pTeamBackRow;
    }
    public void SetPawn(PawnData pPawnData, Transform pPawnTranform)
    {
        m_PawnData = pPawnData;

        if (pPawnData == null) return;

        pPawnData.CurrentTile = this;
        OnPawnUpdated?.Invoke(pPawnTranform);
    }

    //INTERFACE
    public Vector2Int GetPosition()
    {
        return m_Position;
    }
    public IPawn GetPawnOnIt()
    {
        return m_PawnData;
    }
    public bool IsBusy()
    {
        return m_PawnData != null;
    }
}

public enum TileState
{
    None,
    Highlighted,
    Selected,
    Moveable
}