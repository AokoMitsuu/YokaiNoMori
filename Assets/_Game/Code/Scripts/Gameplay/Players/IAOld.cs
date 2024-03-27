using System.Collections.Generic;
using System;
using UnityEngine;
using YokaiNoMori.Enumeration;
using YokaiNoMori.Interface;
using System.Linq;

public class IAOld : MonoBehaviour, ICompetitor
{
    [SerializeField] private Board m_Board;
    [SerializeField] private float m_StartTimer;
    [SerializeField] private float m_MaxThinkingTimer;
    [SerializeField] private int m_DepthMax = 7;
    [SerializeField] private int m_ScoreIaTake = 5;
    [SerializeField] private int m_ScorePlayerTake = -3;
    [SerializeField] private int m_ScoreMove = 1;
    [SerializeField] private int m_ScoreDrop = 2;
    [SerializeField] private int m_ScoreWin = 500;
    [SerializeField] private int m_ScoreLose = 50000;
    [SerializeField] private VictoryRuleSo m_VictoryRule;

    private OldBoardData m_BestBoardData = null;
    private int m_MaxScore = -1;

    private string m_Name = "groupe 5-GRASSE-CORNET";
    private IGameManager m_BoardInterface;
    private ECampType m_ECampType;

    private List<TileData> m_TileList = new();
    private List<TileData> P1_ReserveList = new();
    private List<TileData> P2_ReserveList = new();

    #region IA

    private List<OldBoardData> ListAllPossibilities(OldBoardData pBoardTiles, int pDepth)
    {
        List<OldBoardData> result = new();

        for (int i = 0; i < pBoardTiles.CurrentBoard.Count; i++)
        {
            if (pBoardTiles.CurrentBoard[i].PawnData == null)
                continue;

            if (pDepth % 2 == 1 && pBoardTiles.CurrentBoard[i].PawnData.Team == ECampType.PLAYER_TWO)
                continue;

            if (pDepth % 2 == 0 && pBoardTiles.CurrentBoard[i].PawnData.Team == ECampType.PLAYER_ONE)
                continue;

            List<TileData> ranges = GenerateReachableTileList(pBoardTiles.CurrentBoard[i], pBoardTiles.CurrentBoard);

            foreach (TileData range in ranges)
            {
                List<TileData> newList = DeepCloneTileList(pBoardTiles.CurrentBoard);
                OldBoardData newBoard = new OldBoardData();
                newBoard.CurrentBoard = newList;
                newBoard.P1Reserve = DeepCloneTileList(pBoardTiles.P1Reserve);
                newBoard.P2Reserve = DeepCloneTileList(pBoardTiles.P2Reserve);

                TileData targetTile = newList.Find(x => x.GetPosition().Equals(range.GetPosition()));

                if (pDepth == 0)
                {
                    newBoard.CurrentTileSelect = targetTile;
                    newBoard.CurrentPawnSelect = pBoardTiles.CurrentBoard[i].PawnData;
                    newBoard.DebugSelect = pBoardTiles.CurrentBoard[i];
                }
                else
                {
                    newBoard.CurrentTileSelect = pBoardTiles.CurrentTileSelect;
                    newBoard.CurrentPawnSelect = pBoardTiles.CurrentPawnSelect;
                }

                if (targetTile.PawnData != null)
                {
                    if (pDepth % 2 == 0)
                    {
                        newBoard.P1Reserve.Find(x => x.PawnData == null).SetPawn(targetTile.PawnData, null);
                        newBoard.Score = m_ScoreIaTake;
                    }
                    else
                    {
                        newBoard.P2Reserve.Find(x => x.PawnData == null).SetPawn(targetTile.PawnData, null);
                        newBoard.Score = -m_ScorePlayerTake;

                    }
                }
                else
                {
                    if (pDepth % 2 == 0)
                    {
                        newBoard.Score = m_ScoreMove;
                    }
                    else
                    {
                        newBoard.Score = -m_ScoreMove;
                    }
                }

                targetTile.SetPawn(newBoard.CurrentBoard[i].PawnData, null);
                newBoard.CurrentBoard[i].SetPawn(null, null);

                List<PawnData> P1_Pawn = new();
                List<PawnData> P2_Pawn = new();

                foreach (TileData tiles in newBoard.CurrentBoard)
                {
                    if (tiles.PawnData == null)
                        continue;

                    if (tiles.PawnData.Team == ECampType.PLAYER_ONE)
                    {
                        P1_Pawn.Add(tiles.PawnData);
                    }
                    else
                    {
                        P2_Pawn.Add(tiles.PawnData);
                    }
                }

                ECampType winner = m_VictoryRule.CheckVictory(newBoard.CurrentBoard, P1_Pawn, P2_Pawn);

                if (winner == ECampType.PLAYER_TWO)
                {
                    newBoard.Score += m_ScoreWin;
                }
                else if (winner == ECampType.PLAYER_ONE)
                {
                    newBoard.Score -= m_ScoreLose;
                }

                result.Add(newBoard);
            }
        }

        if (pDepth % 2 == 1)
        {
            for (int i = 0; i < pBoardTiles.P1Reserve.Count; i++)
            {
                if (pBoardTiles.P1Reserve[i].PawnData == null)
                    break;

                List<TileData> ranges = GenerateReachableTileList(pBoardTiles.P1Reserve[i], pBoardTiles.CurrentBoard);

                foreach (TileData range in ranges)
                {
                    List<TileData> newList = DeepCloneTileList(pBoardTiles.CurrentBoard);
                    List<TileData> newReserveP1 = DeepCloneTileList(pBoardTiles.P1Reserve);
                    List<TileData> newReserveP2 = DeepCloneTileList(pBoardTiles.P2Reserve);

                    OldBoardData newBoard = new OldBoardData();

                    newBoard.CurrentBoard = newList;
                    newBoard.P1Reserve = newReserveP1;
                    newBoard.P2Reserve = newReserveP2;
                    newBoard.Score = -m_ScoreDrop;
                    newBoard.ActionType = EActionType.PARACHUTE;
                    TileData targetTile = newList.Find(x => x.GetPosition().Equals(range.GetPosition()));
                    targetTile.SetPawn(newList[i].PawnData, null);

                    newReserveP1[i].SetPawn(null, null);

                    if (pDepth == 0)
                    {
                        newBoard.CurrentTileSelect = targetTile;
                        newBoard.CurrentPawnSelect = pBoardTiles.P1Reserve[i].PawnData;
                    }

                    result.Add(newBoard);
                }
            }
        }
        else
        {
            for (int i = 0; i < pBoardTiles.P2Reserve.Count; i++)
            {
                if (pBoardTiles.P2Reserve[i].PawnData == null)
                    break;

                List<TileData> ranges = GenerateReachableTileList(pBoardTiles.P2Reserve[i], pBoardTiles.CurrentBoard);

                foreach (TileData range in ranges)
                {
                    List<TileData> newList = DeepCloneTileList(pBoardTiles.CurrentBoard);
                    List<TileData> newReserveP2 = DeepCloneTileList(pBoardTiles.P2Reserve);
                    List<TileData> newReserveP1 = DeepCloneTileList(pBoardTiles.P1Reserve);

                    OldBoardData newBoard = new OldBoardData();

                    newBoard.CurrentBoard = newList;
                    newBoard.P2Reserve = newReserveP2;
                    newBoard.P1Reserve = newReserveP1;
                    newBoard.Score = m_ScoreDrop;
                    newBoard.ActionType = EActionType.PARACHUTE;
                    TileData targetTile = newList.Find(x => x.GetPosition().Equals(range.GetPosition()));
                    targetTile.SetPawn(newList[i].PawnData, null);

                    newReserveP2[i].SetPawn(null, null);

                    if (pDepth == 0)
                    {
                        newBoard.CurrentTileSelect = targetTile;
                        newBoard.CurrentPawnSelect = pBoardTiles.P2Reserve[i].PawnData;
                    }

                    result.Add(newBoard);
                }
            }
        }

        return result;
    }
    private List<TileData> DeepCloneTileList(List<TileData> pOriginalList)
    {
        List<TileData> clonedList = new List<TileData>();

        foreach (TileData originalTile in pOriginalList)
        {
            TileData clonedTile = new TileData();

            clonedTile.Board = originalTile.Board;

            if (originalTile.IsReserve)
                clonedTile.SetIsReserve();

            clonedTile.SetTeamBackRow(originalTile.TeamBackRow);
            clonedTile.SetPosition(originalTile.GetPosition());

            clonedTile.SetPosition(new Vector2Int(originalTile.GetPosition().x, originalTile.GetPosition().y));

            if (originalTile.PawnData != null)
            {
                PawnData clonePawn = new PawnData();

                clonePawn.Init(originalTile.PawnData.PawnSo, originalTile.PawnData.Team, originalTile.PawnData.GetCurrentOwner());

                clonedTile.SetPawn(clonePawn, null);
            }

            clonedList.Add(clonedTile);
        }

        return clonedList;
    }
    private List<TileData> GenerateReachableTileList(TileData pTile, List<TileData> pBoardTiles)
    {
        List<TileData> reachableTileList = new();

        if (pTile.IsReserve)
        {
            foreach (TileData tileData in pBoardTiles)
            {
                if (tileData.PawnData == null && !tileData.IsReserve)
                {
                    reachableTileList.Add(tileData);
                }
            }
        }
        else
        {
            Vector2Int targetPos = pTile.GetPosition();

            foreach (Vector2Int range in pTile.PawnData.PawnSo.Ranges)
            {
                Vector2Int newPos;

                if (pTile.PawnData.Team == ECampType.PLAYER_ONE)
                {
                    newPos = new Vector2Int(targetPos.x + range.x, targetPos.y + range.y);
                }
                else
                {
                    newPos = new Vector2Int(targetPos.x + range.x, targetPos.y - range.y);
                }

                TileData tileDisplay = pBoardTiles.Find(x => x.GetPosition().Equals(newPos));

                if (tileDisplay != null && (tileDisplay.GetPawnOnIt() == null || tileDisplay.PawnData.Team != pTile.PawnData.Team))
                {
                    reachableTileList.Add(tileDisplay);
                }
            }
        }

        return reachableTileList;
    }
    private void FindBestPossibility(OldBoardData main)
    {
        m_MaxScore = int.MinValue;
        m_BestBoardData = null;
        List<OldBoardData> allPossibilities = ListAllPossibilities(main, 0);

        foreach (OldBoardData sub in allPossibilities)
        {
            int score = Minimax(sub, 1, m_DepthMax, false);
            if (m_MaxScore < score)
            {
                m_MaxScore = score;
                m_BestBoardData = sub;
            }
        }
    }
    private int Minimax(OldBoardData pBoard, int pDepth, int maxDepth, bool pIsMaximizingPlayer)
    {
        List<OldBoardData> allPossibilities = ListAllPossibilities(pBoard, pDepth);

        if (allPossibilities.Count == 0 || maxDepth >= pDepth || Time.realtimeSinceStartup - m_StartTimer > m_MaxThinkingTimer)
        {
            return pBoard.Score;
        }

        if (pIsMaximizingPlayer)
        {
            int maxEval = int.MinValue;
            foreach (var child in allPossibilities)
            {
                int eval = Minimax(child, pDepth + 1, maxDepth, false);
                maxEval = Math.Max(maxEval, eval);
            }
            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;
            foreach (var child in allPossibilities)
            {
                int eval = Minimax(child, pDepth + 1, maxDepth, true);
                minEval = Math.Min(minEval, eval);
            }
            return minEval;
        }
    }

    #endregion

    //INTERFACE
    public string GetName()
    {
        return m_Name;
    }
    public ECampType GetCamp()
    {
        return m_ECampType;
    }
    public void SetCamp(ECampType pCamp)
    {
        m_ECampType = pCamp;
    }
    public void Init(IGameManager pIGameManager, float timerForAI)
    {
        m_BoardInterface = pIGameManager;
        m_MaxThinkingTimer = timerForAI;
    }
    public void GetDatas()
    {
        m_TileList = m_Board.GetBoardState().Item1;
        P1_ReserveList = m_Board.GetBoardState().Item2;
        P2_ReserveList = m_Board.GetBoardState().Item3;
    }
    public void StartTurn()
    {
        m_StartTimer = Time.realtimeSinceStartup;
        OldBoardData boardData = new();

        boardData.CurrentBoard = DeepCloneTileList(m_TileList);
        boardData.P1Reserve = DeepCloneTileList(P1_ReserveList);
        boardData.P2Reserve = DeepCloneTileList(P2_ReserveList);
        FindBestPossibility(boardData);
        m_BoardInterface.DoAction(m_BestBoardData.CurrentPawnSelect, m_BestBoardData.CurrentTileSelect.GetPosition(), m_BestBoardData.ActionType);
    }
    public void StopTurn()
    {
        throw new System.NotImplementedException();
    }
}

public class OldBoardData
{
    public List<TileData> CurrentBoard;
    public List<TileData> P1Reserve;
    public List<TileData> P2Reserve;
    public PawnData CurrentPawnSelect;
    public TileData DebugSelect;
    public TileData CurrentTileSelect;
    public int Score = 0;

    public List<OldBoardData> Possibilities = new();
    public EActionType ActionType = EActionType.MOVE;
}
