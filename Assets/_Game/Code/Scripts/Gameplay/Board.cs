using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using NaughtyAttributes;
using System.Linq;

public class Board : MonoBehaviour
{
    [Header("Items")]
    [SerializeField] private Image m_BoardImage;
    [SerializeField] private Transform m_BoardParent;
    [SerializeField] private Transform m_P1ReserveParent;
    [SerializeField] private Transform m_P2ReserveParent;
    [SerializeField] private Tile m_TilePrefab;
    [SerializeField] private Pawn m_PawnPrefab;
    [SerializeField] private List<Sprite> m_BackgroundList = new();
    [SerializeField] private List<MusicSo> m_BackgroundMusics = new();
    [SerializeField] private List<Tile> P1_InReserveTiles;
    [SerializeField] private List<Tile> P2_InReserveTiles;

    [Header("Settings")]
    [SerializeField] private ScenarioSo m_Scenario;
    [SerializeField] private VictoryRuleSo m_VictoryRule;
    [SerializeField] private float m_TileSize;
    [SerializeField] private float m_TileSpacing;

    [Header("Dictionary")]
    [SerializeField] private SerializedDictionary<Vector2, Tile> m_BoardInfo = new();

    private List<Pawn> P1_OnBoardPawns = new();
    private List<Pawn> P2_OnBoardPawns = new();

    private List<Pawn> P1_InReservePawns = new();
    private List<Pawn> P2_InReservePawns = new();


    private int m_TotalCells;
    private BoardState m_BoardState;
    private Tile m_CurrentSelectedTile;
    private List<Tile> m_ReachableTiles = new();

    #region Setup

    private void Clear()
    {
        foreach (KeyValuePair<Vector2, Tile> tile in m_BoardInfo)
        {
            if(tile.Value.Pawn != null)
                Destroy(tile.Value.Pawn.gameObject);

            if(tile.Value != null)
                Destroy(tile.Value.gameObject);
        }

        foreach(Pawn pawn in P1_InReservePawns)
        {
            Destroy(pawn.gameObject);
        }

        foreach (Pawn pawn in P2_InReservePawns)
        {
            Destroy(pawn.gameObject);
        }

        m_BoardInfo.Clear();
        P1_OnBoardPawns.Clear();
        P2_OnBoardPawns.Clear();
        P1_InReservePawns.Clear();
        P2_InReservePawns.Clear();
    }
    private void SetupGame()
    {
        Clear();

        m_TotalCells = (int)m_Scenario.BoardSize.x * (int)m_Scenario.BoardSize.y;

        SetupAmbience();
        SetupTiles();
        SetupPawns();
    }
    private void SetupAmbience()
    {
        int randomNumber = Random.Range(0, m_BackgroundList.Count);
        m_BoardImage.sprite = m_BackgroundList[randomNumber];
        App.Instance.AudioManager.StopAllMusics();
        m_BackgroundMusics[randomNumber]?.Play();
    }
    private void SetupTiles()
    {
        for (int x = 0; x < m_Scenario.BoardSize.x; x++)
        {
            for (int y = 0; y < m_Scenario.BoardSize.y; y++)
            {
                Tile tile = Instantiate(m_TilePrefab, BoardPositionToWorldPosition(x, y), Quaternion.identity);
                tile.transform.SetParent(m_BoardParent);
                tile.Init(y == 0 ? Team.Player1 : y == m_Scenario.BoardSize.y - 1 ? Team.Player2 : Team.None, this);

                m_BoardInfo.Add(new Vector2(x, y), tile);
            }
        }

        for (int i = 0; i < P1_InReserveTiles.Count; i++)
        {
            P1_InReserveTiles[i].Init(Team.Player1, this);
            P2_InReserveTiles[i].Init(Team.Player2, this);
        }
    }
    private void SetupPawns()
    {
        for (int i = 0; i < m_Scenario.Pieces.Count; i++)
        {
            if (m_Scenario.Pieces[i] == null) continue;

            // PLAYER 1
            var pawn = Instantiate(m_PawnPrefab);
            var pos = new Vector2(i % (int)m_Scenario.BoardSize.x, i / (int)m_Scenario.BoardSize.x);
            pawn.Init(m_Scenario.Pieces[i], Team.Player1);
            MovePawnTo(pawn, m_BoardInfo[pos]);
            P1_OnBoardPawns.Add(pawn);

            // PLAYER 2
            int reversedIndex = m_TotalCells - 1 - i;
            pawn = Instantiate(m_PawnPrefab);
            pos = new Vector2(reversedIndex % (int)m_Scenario.BoardSize.x, reversedIndex / (int)m_Scenario.BoardSize.x);
            pawn.Init(m_Scenario.Pieces[i], Team.Player2);
            MovePawnTo(pawn, m_BoardInfo[pos]);
            P2_OnBoardPawns.Add(pawn);
        }

        m_BoardState = BoardState.P1_PawnSelection;
    }

    private void Start()
    {
        SetupGame();
    }

    #endregion

    #region Gameplay

    public void OnTileClick(Tile pTile, bool pIsReserve)
    {
        ProcessAction(pTile, pIsReserve);
    }

    private void ProcessAction(Tile pTile, bool pIsReserve)
    {
        Pawn pawn = pTile.Pawn;

        if (m_BoardState == BoardState.Idle)
        {
            return;
        }
        else if (m_BoardState == BoardState.P1_PawnSelection && pawn != null && pawn.Team == Team.Player1)
        {
            SelectPawn(pTile, pIsReserve);
        }
        else if (m_BoardState == BoardState.P2_PawnSelection && pawn != null && pawn.Team == Team.Player2)
        {
            SelectPawn(pTile, pIsReserve);
        }
        else if ((m_BoardState == BoardState.P1_PawnMove || m_BoardState == BoardState.P2_PawnMove) && m_ReachableTiles.Contains(pTile))
        {
            MovePawnTo(m_CurrentSelectedTile.Pawn, pTile);
            CheckWin();
        }
        else if(m_BoardState == BoardState.P1_PawnMove || m_BoardState == BoardState.P2_PawnMove)
        {
            ClearMoveState();
            m_BoardState = m_BoardState == BoardState.P1_PawnMove ? BoardState.P1_PawnSelection : BoardState.P2_PawnSelection;
        }
    }

    private void SelectPawn(Tile pTile, bool pIsReserve)
    {
        m_CurrentSelectedTile = pTile;

        m_CurrentSelectedTile.SetState(TileState.Selected);

        m_ReachableTiles.AddRange(GenerateReachableTileList(pTile, pIsReserve));

        foreach (Tile reachableTile in m_ReachableTiles)
        {
            reachableTile.SetState(TileState.Highlighted);
        }

        m_BoardState = m_BoardState == BoardState.P1_PawnSelection ? BoardState.P1_PawnMove : BoardState.P2_PawnMove;
    }

    private List<Tile> GenerateReachableTileList(Tile pTile, bool pIsReserve)
    {
        List<Tile> reachableTileList = new();

        if (pIsReserve)
        {
            foreach (KeyValuePair<Vector2, Tile> tile in m_BoardInfo)
            {
                if (tile.Value.Pawn == null)
                    reachableTileList.Add(tile.Value);
            }
        }
        else
        {
            Vector2 targetPos = m_BoardInfo.FirstOrDefault(x => x.Value == pTile).Key;

            foreach (Vector2 range in pTile.Pawn.PawnSo.Ranges)
            {
                Vector2 newPos;

                if (pTile.Pawn.Team == Team.Player1)
                {
                    newPos = new Vector2(targetPos.x + range.x, targetPos.y + range.y);
                }
                else
                {
                    newPos = new Vector2(targetPos.x + range.x, targetPos.y - range.y);
                }

                if (m_BoardInfo.TryGetValue(newPos, out Tile tile)
                    && (tile.Pawn == null
                    || tile.Pawn.Team != pTile.Pawn.Team))
                {
                    reachableTileList.Add(tile);
                }
            }
        }
        

        return reachableTileList;
    }

    private void MovePawnTo(Pawn pPawn, Tile pTargetTile)
    {

        if (pTargetTile.Pawn != null)
        {
            CapturePawn(pTargetTile.Pawn);
        }

        if(m_CurrentSelectedTile != null)
        {
            m_CurrentSelectedTile.Pawn = null;
        }

        pTargetTile.Pawn = pPawn;

        if (pTargetTile.TeamBackRow != Team.None && pTargetTile.TeamBackRow != pPawn.Team && !m_CurrentSelectedTile.IsReserve)
            pPawn.Promote();

        ClearMoveState();

        m_BoardState = m_BoardState == BoardState.P1_PawnMove ? BoardState.P2_PawnSelection : BoardState.P1_PawnSelection;
    }

    private void CapturePawn(Pawn pPawn)
    {
        switch (pPawn.Team)
        {
            case Team.Player1:
                P1_OnBoardPawns.Remove(pPawn);
                P2_InReservePawns.Add(pPawn);
                P2_InReserveTiles.First((x) => x.Pawn == null).Pawn = pPawn;
                pPawn.Team = Team.Player2;
                break;
            case Team.Player2:
                P2_OnBoardPawns.Remove(pPawn);
                P1_InReservePawns.Add(pPawn);
                P1_InReserveTiles.First((x) => x.Pawn == null).Pawn = pPawn;
                pPawn.Team = Team.Player1;
                break;
        }
    }

    private void ClearMoveState()
    {
        m_CurrentSelectedTile?.SetState(TileState.None);
        m_CurrentSelectedTile = null;

        foreach (var reachableTile in m_ReachableTiles)
        {
            reachableTile.SetState(TileState.None);
        }

        m_ReachableTiles.Clear();
    }

    private void CheckWin()
    {
        Team winner = m_VictoryRule.CheckVictory(m_BoardInfo, P1_OnBoardPawns, P2_OnBoardPawns, P1_InReservePawns, P2_InReservePawns);
        if (winner == Team.None) return;

        Debug.Log(winner);
        SetupGame();
    }

    #endregion

    #region Utils

    private Vector3 BoardPositionToWorldPosition(int x, int y)
    {
        float xOffset = x * (m_TileSize + m_TileSpacing);
        float yOffset = y * (m_TileSize + m_TileSpacing);

        float totalWidth = (m_Scenario.BoardSize.x * m_TileSize) + ((m_Scenario.BoardSize.x - 1) * m_TileSpacing);
        float totalHeight = (m_Scenario.BoardSize.y * m_TileSize) + ((m_Scenario.BoardSize.y - 1) * m_TileSpacing);

        float centeredXOffset = xOffset - totalWidth / 2 + m_TileSize / 2;
        float centeredYOffset = yOffset - totalHeight / 2 + m_TileSize / 2;

        return new Vector3(centeredXOffset + m_BoardImage.rectTransform.rect.width / 2.0f, centeredYOffset + m_BoardImage.rectTransform.rect.height / 2.0f, -1);
    }
    private Vector2 WorldPositionToBoardPosition(Vector2 pWorldPos)
    {
        // Calcul des offsets centrés à l'inverse
        float centeredXOffset = pWorldPos.x - m_BoardImage.rectTransform.rect.width / 2.0f;
        float centeredYOffset = pWorldPos.y - m_BoardImage.rectTransform.rect.height / 2.0f;

        // Retrait des demi-tailles de tuile pour obtenir le début du grid
        float startX = centeredXOffset + (m_Scenario.BoardSize.x * m_TileSize + (m_Scenario.BoardSize.x - 1) * m_TileSpacing) / 2 - m_TileSize / 2;
        float startY = centeredYOffset + (m_Scenario.BoardSize.y * m_TileSize + (m_Scenario.BoardSize.y - 1) * m_TileSpacing) / 2 - m_TileSize / 2;

        // Conversion des positions en coordonnées de grille
        int x = Mathf.RoundToInt(startX / (m_TileSize + m_TileSpacing));
        int y = Mathf.RoundToInt(startY / (m_TileSize + m_TileSpacing));

        return new Vector2(x, y);
    }

    #endregion
}

public enum BoardState
{
    Idle,
    P1_PawnSelection,
    P1_PawnMove,
    P2_PawnSelection,
    P2_PawnMove
}

public struct BoardData
{
    public Dictionary<Vector2, Tile> BoardState;

    public List<Pawn> P1_OnBoardPawns;
    public List<Pawn> P2_OnBoardPawns;

    public List<Pawn> P1_InReservePawns;
    public List<Pawn> P2_InReservePawns;
}