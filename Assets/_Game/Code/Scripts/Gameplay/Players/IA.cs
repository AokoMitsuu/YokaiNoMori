using System.Collections.Generic;
using UnityEngine;
using YokaiNoMori.Enumeration;
using YokaiNoMori.Interface;

public class IA : MonoBehaviour, ICompetitor
{
    [Header("References")]
    [SerializeField] private Board m_Board;

    [Header("Settings")]
    [SerializeField] private int m_DepthMax = 4;
    [SerializeField] private float m_MaxThinkingTimer = 30.0f;

    [Header("Weights")]
    [SerializeField] private int m_ScoreIaTake = 50;
    [SerializeField] private int m_ScorePlayerTake = 50;
    [SerializeField] private int m_ScoreMove = 5;
    [SerializeField] private int m_ScoreDrop = 45;
    [SerializeField] private int m_ScoreWin = 5000;
    [SerializeField] private int m_ScoreLose = 50000;

    [Header("TOREMOVE")]
    [SerializeField] private VictoryRuleSo m_VictoryRule;

    private IGameManager m_BoardInterface;
    private float m_StartTimer;

    private List<TileData> m_TileList = new();
    private List<TileData> P1_ReserveList = new();
    private List<TileData> P2_ReserveList = new();

    private string m_Name = "groupe 5-GRASSE-CORNET";

    #region IA

    private BoardData FindBestPossibility(BoardData boardData)
    {
        int maxScore = int.MinValue;
        BoardData bestBoardData = null;
        List<BoardData> allPossibilities = ListAllPossibilities(boardData, 0);

        foreach (BoardData sub in allPossibilities)
        {
            int score = Minimax(sub, 1, false);
            if (maxScore < score)
            {
                maxScore = score;
                bestBoardData = sub;
            }
        }

        return bestBoardData;
    }
    private int Minimax(BoardData pBoard, int pDepth, bool pIsMaximizingPlayer)
    {
        List<BoardData> nodes = ListAllPossibilities(pBoard, pDepth);

        if (nodes.Count == 0 || pDepth >= m_DepthMax  || Time.realtimeSinceStartup - m_StartTimer > m_MaxThinkingTimer)
        {
            return pBoard.Score;
        }

        if (pIsMaximizingPlayer)
        {
            int maxEval = int.MinValue;
            foreach (var child in nodes)
            {
                int eval = Minimax(child, pDepth + 1, false);
                maxEval = Mathf.Max(maxEval, eval);
            }
            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;
            foreach (var child in nodes)
            {
                int eval = Minimax(child, pDepth + 1, true);
                minEval = Mathf.Min(minEval, eval);
            }
            return minEval;
        }
    }
    private List<BoardData> ListAllPossibilities(BoardData pBoardData, int pDepth)
    {
        List<BoardData> results = new();

        for (int i = 0; i < pBoardData.CurrentBoard.Count; i++)
        {
            if (pBoardData.CurrentBoard[i].PawnData == null) continue;

            bool skipCondition1 = pDepth % 2 == 1 && pBoardData.CurrentBoard[i].PawnData.Team == ECampType.PLAYER_TWO;
            bool skipCondition2 = pDepth % 2 == 0 && pBoardData.CurrentBoard[i].PawnData.Team == ECampType.PLAYER_ONE;
            if (skipCondition1 || skipCondition2) continue;

            List<TileData> reachableTiles = GenerateReachableTiles(pBoardData.CurrentBoard[i], pBoardData.CurrentBoard);

            foreach (TileData tile in reachableTiles)
            {
                BoardData newBoard = new();
                List<TileData> newBoardTiles = Clone(pBoardData.CurrentBoard);
                newBoard.CurrentBoard = newBoardTiles;
                newBoard.P1Reserve = Clone(pBoardData.P1Reserve);
                newBoard.P2Reserve = Clone(pBoardData.P2Reserve);

                TileData targetTile = newBoardTiles.Find(x => x.GetPosition().Equals(tile.GetPosition()));

                if (pDepth == 0)
                {
                    newBoard.TargetTile = targetTile;
                    newBoard.SelectedPawn = pBoardData.CurrentBoard[i].PawnData;
                }
                else
                {
                    newBoard.TargetTile = pBoardData.TargetTile;
                    newBoard.SelectedPawn = pBoardData.SelectedPawn;
                }

                if (targetTile.PawnData != null) // Capture
                {
                    if (pDepth % 2 == 0)
                    {
                        newBoard.P1Reserve.Find(x => x.PawnData == null).SetPawn(targetTile.PawnData, null);
                    }
                    else
                    {
                        newBoard.P2Reserve.Find(x => x.PawnData == null).SetPawn(targetTile.PawnData, null);
                    }
                    newBoard.Score = pDepth % 2 == 0 ? m_ScoreIaTake : -m_ScorePlayerTake;
                }
                else // Move
                {
                    newBoard.Score = pDepth % 2 == 0 ? m_ScoreMove : -m_ScoreMove;
                }

                targetTile.SetPawn(newBoard.CurrentBoard[i].PawnData, null);
                newBoard.CurrentBoard[i].SetPawn(null, null);

                //Check Win

                List<PawnData> P1_Pawn = new();
                List<PawnData> P2_Pawn = new();
                foreach (TileData tiles in newBoard.CurrentBoard)
                {
                    if (tiles.PawnData == null) continue;

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
                    newBoard.HasSomeoneWon = true;
                }
                else if (winner == ECampType.PLAYER_ONE)
                {
                    newBoard.Score -= m_ScoreLose;
                    newBoard.HasSomeoneWon = true;
                }

                results.Add(newBoard);
            }
        }

        //Parachute
        if(pDepth % 2 == 0)
        {
            for (int i = 0; i < pBoardData.P2Reserve.Count; i++)
            {
                if (pBoardData.P2Reserve[i].PawnData == null) continue;

                List<TileData> ranges = GenerateReachableTiles(pBoardData.P2Reserve[i], pBoardData.CurrentBoard);

                foreach (TileData range in ranges)
                {
                    List<TileData> newTileList = Clone(pBoardData.CurrentBoard);
                    List<TileData> newReserveP1 = Clone(pBoardData.P1Reserve);
                    List<TileData> newReserveP2 = Clone(pBoardData.P2Reserve);

                    BoardData newBoard = new BoardData();

                    newBoard.ActionType = EActionType.PARACHUTE;
                    newBoard.CurrentBoard = newTileList;
                    newBoard.P1Reserve = newReserveP1;
                    newBoard.P2Reserve = newReserveP2;
                    newBoard.Score = m_ScoreDrop;

                    TileData targetTile = newTileList.Find(x => x.GetPosition().Equals(range.GetPosition()));
                    targetTile.SetPawn(newTileList[i].PawnData, null);

                    newReserveP2[i].SetPawn(null, null);

                    if (pDepth == 0)
                    {
                        newBoard.TargetTile = targetTile;
                        newBoard.SelectedPawn = pBoardData.P2Reserve[i].PawnData;
                    }

                    results.Add(newBoard);
                }
            }
        }
        else
        {
            for (int i = 0; i < pBoardData.P1Reserve.Count; i++)
            {
                if (pBoardData.P1Reserve[i].PawnData == null) continue;

                List<TileData> ranges = GenerateReachableTiles(pBoardData.P1Reserve[i], pBoardData.CurrentBoard);

                foreach (TileData range in ranges)
                {
                    List<TileData> newList = Clone(pBoardData.CurrentBoard);
                    List<TileData> newReserveP1 = Clone(pBoardData.P1Reserve);
                    List<TileData> newReserveP2 = Clone(pBoardData.P2Reserve);

                    BoardData newBoard = new BoardData();

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
                        newBoard.TargetTile = targetTile;
                        newBoard.SelectedPawn = pBoardData.P1Reserve[i].PawnData;
                    }

                    results.Add(newBoard);
                }
            }
        }

        return results;
    }
    private List<TileData> GenerateReachableTiles(TileData pTile, List<TileData> pBoardTiles)
    {
        List<TileData> reachableTiles = new();

        if (pTile.IsReserve)
        {
            foreach (TileData tileData in pBoardTiles)
            {
                if (tileData.PawnData == null && !tileData.IsReserve)
                {
                    reachableTiles.Add(tileData);
                }
            }
        }
        else
        {
            Vector2Int currentPos = pTile.GetPosition();

            foreach (Vector2Int range in pTile.PawnData.PawnSo.Ranges)
            {
                Vector2Int newPos;

                if (pTile.PawnData.Team == ECampType.PLAYER_ONE)
                {
                    newPos = new Vector2Int(currentPos.x + range.x, currentPos.y + range.y);
                }
                else
                {
                    newPos = new Vector2Int(currentPos.x + range.x, currentPos.y - range.y);
                }

                TileData tileData = pBoardTiles.Find(x => x.GetPosition().Equals(newPos));

                if (tileData != null && (tileData.GetPawnOnIt() == null || tileData.PawnData.Team != pTile.PawnData.Team))
                {
                    reachableTiles.Add(tileData);
                }
            }
        }

        return reachableTiles;
    }
    private List<TileData> Clone(List<TileData> pTileListToClone)
    {
        List<TileData> clonedList = new();

        foreach (TileData tileToClone in pTileListToClone)
        {
            TileData clonedTile = new();                          //New Tile
            if (tileToClone.IsReserve) clonedTile.SetIsReserve(); //IsReserve
            clonedTile.Board = tileToClone.Board;                 //Board
            clonedTile.SetPosition(tileToClone.GetPosition());    //Position
            clonedTile.SetTeamBackRow(tileToClone.TeamBackRow);   //Backrow

            if (tileToClone.PawnData != null)                     
            {
                PawnData clonePawn = new();
                clonePawn.Init(tileToClone.PawnData.PawnSo, tileToClone.PawnData.Team, tileToClone.PawnData.GetCurrentOwner());
                clonedTile.SetPawn(clonePawn, null);
            }                  //Pawn

            clonedList.Add(clonedTile);
        }

        return clonedList;
    }

    #endregion

    // INTERFACE
    public ECampType GetCamp()
    {
        throw new System.NotImplementedException();
    }
    public void GetDatas()
    {
        m_TileList = m_Board.GetBoardState().Item1;
        P1_ReserveList = m_Board.GetBoardState().Item2;
        P2_ReserveList = m_Board.GetBoardState().Item3;
    }
    public string GetName()
    {
        return m_Name;
    }
    public void Init(IGameManager igameManager, float timerForAI)
    {
        m_BoardInterface = igameManager;
        m_MaxThinkingTimer = timerForAI;
    }
    public void SetCamp(ECampType camp)
    {
        throw new System.NotImplementedException();
    }
    public void StartTurn()
    {
        m_StartTimer = Time.realtimeSinceStartup;

        BoardData boardData = new();

        boardData.CurrentBoard = Clone(m_TileList);
        boardData.P1Reserve = Clone(P1_ReserveList);
        boardData.P2Reserve = Clone(P2_ReserveList);

        BoardData bestBoardData = FindBestPossibility(boardData);
        m_BoardInterface.DoAction(bestBoardData.SelectedPawn, bestBoardData.TargetTile.GetPosition(), bestBoardData.ActionType);
    }
    public void StopTurn()
    {
        throw new System.NotImplementedException();
    }
}

public class BoardData
{
    public List<TileData> CurrentBoard;
    public List<TileData> P1Reserve;
    public List<TileData> P2Reserve;
    public int Score = 0;
    public PawnData SelectedPawn;
    public TileData TargetTile;
    public bool HasSomeoneWon = false;
    public EActionType ActionType = EActionType.MOVE;

    public List<OldBoardData> Possibilities = new();
}
