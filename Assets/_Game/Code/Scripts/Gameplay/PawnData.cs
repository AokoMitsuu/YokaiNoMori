using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using YokaiNoMori.Enumeration;
using YokaiNoMori.Interface;

public class PawnData : IPawn
{
    private PawnSo m_PawnSo;
    private ECampType m_Team;
    private ICompetitor m_Owner;
    private TileData m_CurrentTile;

    public PawnSo PawnSo => m_PawnSo;
    public ECampType Team
    {
        get => m_Team;
        set
        {
            m_Team = value;
            OnTeamChange?.Invoke(value == ECampType.PLAYER_ONE ? new Vector3(0f, 0f, 0f) : new Vector3(0f, 0f, 180f));
        }
    }
    public TileData CurrentTile { get => m_CurrentTile; set => m_CurrentTile = value; }

    public event Action<Sprite> OnPawnSoRefresh;
    public event Action<Vector3> OnTeamChange;
    public event Action OnPromote;

    public void Init(PawnSo pData, ECampType pTeam, ICompetitor pOwner = null)
    {
        m_PawnSo = pData;
        Team = pTeam;
        m_Owner = pOwner;
        OnPawnSoRefresh?.Invoke(pData.Sprite);
    }

    public void Promote()
    {
        if (m_PawnSo.PromotedPawn == null) return;

        OnPromote?.Invoke();
    }

    public void Demote()
    {
        if (m_PawnSo.DemotedPawn == null) return;

        Init(m_PawnSo.DemotedPawn, m_Team, m_Owner);
    }

    //INTERFACE
    public List<Vector2Int> GetDirections()
    {
        return m_PawnSo.Ranges;
    }
    public EPawnType GetPawnType()
    {
        return m_PawnSo.PawnType;
    }
    public IBoardCase GetCurrentBoardCase()
    {
        return m_CurrentTile;
    }
    public ICompetitor GetCurrentOwner()
    {
        return m_Owner;
    }
    public Vector2Int GetCurrentPosition()
    {
        return m_CurrentTile.GetPosition();
    }
}