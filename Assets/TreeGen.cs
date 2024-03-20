using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using YokaiNoMori.Enumeration;
using YokaiNoMori.Interface;

public class TreeGen : MonoBehaviour
{
    private BoardData BoardData = new();
    [SerializeField] private float m_StartTimer;
    [SerializeField] private float m_TimerMax = 180f;
    [SerializeField] private int m_DepthMax = 7;
    [SerializeField] private int m_Count = 1;
    [SerializeField] private int m_ScoreIaTake = 5;
    [SerializeField] private int m_ScorePlayerTake = -3;
    [SerializeField] private int m_ScoreMove = 1;
    [SerializeField] private VictoryRuleSo m_VictoryRule;

    private int m_MaxScore = -1;
    private BoardData m_BestBoardData = null;

    public void GenerateTree(List<TileData> pBoardData, List<TileData> pReserveP1, List<TileData> pReserveP2)
    {
        m_StartTimer = Time.realtimeSinceStartup;
        BoardData.CurrentBoard = DeepCloneTileList(pBoardData);
        BoardData.P1Reserve = DeepCloneTileList(pReserveP1);
        BoardData.P2Reserve = DeepCloneTileList(pReserveP2);

        GenerateSubNode(BoardData, 0, m_DepthMax);

        FindBestPossibility(BoardData);
        Debug.Log(m_BestBoardData.CurrentPawnSelect.PawnSo.name);
        CountPossibilities(BoardData);
    }

    private void GenerateSubNode(BoardData pBoardData, int pDepth, int pMaxDepth = 10)
    {
        if (pDepth >= pMaxDepth)
        {
            return;
        }

        List<BoardData> allPossibilities = ListAllPossibilities(pBoardData, pDepth);

        pBoardData.Possibilities = allPossibilities;

        foreach (BoardData pData in pBoardData.Possibilities)
        {
            List<PawnData> P1_Pawn = new();
            List<PawnData> P2_Pawn = new();

            foreach (TileData tiles in pData.CurrentBoard)
            {
                if (tiles.PawnData == null)
                    continue;

                if(tiles.PawnData.Team == ECampType.PLAYER_ONE)
                {
                    P1_Pawn.Add(tiles.PawnData);
                }
                else
                {
                    P2_Pawn.Add(tiles.PawnData);
                }
            }

            ECampType winner = m_VictoryRule.CheckVictory(pData.CurrentBoard, P1_Pawn, P2_Pawn);

            if(winner == ECampType.PLAYER_ONE)
            {
                pData.Score += 500;
            }
            else if (winner == ECampType.PLAYER_ONE)
            {
                pData.Score -= 500;
            }
            else
            {
                GenerateSubNode(pData, pDepth + 1, pMaxDepth);
            }
        }
    }

    private List<BoardData> ListAllPossibilities(BoardData pBoardTiles, int pDepth)
    {
        List<BoardData> result = new();

        for (int i = 0; i < pBoardTiles.CurrentBoard.Count; i++)
        {
            if (pBoardTiles.CurrentBoard[i].PawnData == null)
                continue;

            if (pDepth % 2 == 0 && pBoardTiles.CurrentBoard[i].PawnData.Team == ECampType.PLAYER_TWO)
                continue;

            if (pDepth % 2 == 1 && pBoardTiles.CurrentBoard[i].PawnData.Team == ECampType.PLAYER_ONE)
                continue;

            List<TileData> ranges = GenerateReachableTileList(pBoardTiles.CurrentBoard[i], pBoardTiles.CurrentBoard);

            foreach(TileData range in ranges)
            {
                List<TileData> newList = DeepCloneTileList(pBoardTiles.CurrentBoard);
                BoardData newBoard = new BoardData();
                newBoard.CurrentBoard = newList;
                newBoard.P1Reserve = DeepCloneTileList(pBoardTiles.P1Reserve);
                newBoard.P2Reserve = DeepCloneTileList(pBoardTiles.P2Reserve);
                newBoard.Score = pBoardTiles.Score;

                TileData targetTile = newList.Find(x => x.GetPosition().Equals(range.GetPosition()));

                if (pDepth == 0)
                {
                    newBoard.CurrentTileSelect = targetTile;
                    newBoard.CurrentPawnSelect = pBoardTiles.CurrentBoard[i].PawnData;
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
                        newBoard.Score += m_ScoreIaTake;
                    }
                    else
                    {
                        newBoard.P2Reserve.Find(x => x.PawnData == null).SetPawn(targetTile.PawnData, null);
                        newBoard.Score += m_ScorePlayerTake;
                    }
                }
                else
                {
                    newBoard.Score += m_ScoreMove;
                }

                targetTile.SetPawn(newBoard.CurrentBoard[i].PawnData, null);
                newBoard.CurrentBoard[i].SetPawn(null, null);

                result.Add(newBoard);
            }
        }

        if(pDepth % 2 == 0)
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

                    BoardData newBoard = new BoardData();

                    newBoard.CurrentBoard = newList;
                    newBoard.P1Reserve = newReserveP1;
                    newBoard.P2Reserve = newReserveP2;

                    TileData targetTile = newList.Find(x => x.GetPosition().Equals(range.GetPosition()));
                    targetTile.SetPawn(newList[i].PawnData, null);

                    newReserveP1[i].SetPawn(null, null);

                    if (pDepth == 1)
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
            for(int i = 0; i < pBoardTiles.P2Reserve.Count; i++)
            {
                if (pBoardTiles.P2Reserve[i].PawnData == null)
                    break;

                List<TileData> ranges = GenerateReachableTileList(pBoardTiles.P2Reserve[i], pBoardTiles.CurrentBoard);

                foreach (TileData range in ranges)
                {
                    List<TileData> newList = DeepCloneTileList(pBoardTiles.CurrentBoard);
                    List<TileData> newReserveP2 = DeepCloneTileList(pBoardTiles.P2Reserve);
                    List<TileData> newReserveP1 = DeepCloneTileList(pBoardTiles.P1Reserve);

                    BoardData newBoard = new BoardData();

                    newBoard.CurrentBoard = newList;
                    newBoard.P2Reserve = newReserveP2;
                    newBoard.P1Reserve = newReserveP1;

                    TileData targetTile = newList.Find(x => x.GetPosition().Equals(range.GetPosition()));
                    targetTile.SetPawn(newList[i].PawnData, null);

                    newReserveP2[i].SetPawn(null, null);

                    if(pDepth == 1)
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

    private void CountPossibilities(BoardData main)
    {
        foreach(BoardData sub in main.Possibilities)
        {
            m_Count += 1;
            CountPossibilities(sub);
        }
    }

    private void FindBestPossibility(BoardData main)
    {
        if(main.Possibilities.Count == 0 && main.Score > m_MaxScore)
        {
            m_MaxScore = main.Score;
            m_BestBoardData = main;
        }
        else
        {
            foreach (BoardData sub in main.Possibilities)
            {
                FindBestPossibility(sub);
            }
        }
    }
}

[Serializable]
public class BoardData
{
    public List<TileData> CurrentBoard;
    public List<TileData> P1Reserve;
    public List<TileData> P2Reserve;
    public PawnData CurrentPawnSelect;
    public TileData CurrentTileSelect;
    public int Score = 0;

    public List<BoardData> Possibilities = new();
}