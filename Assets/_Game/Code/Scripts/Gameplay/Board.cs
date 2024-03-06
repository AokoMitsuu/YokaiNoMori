using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using NaughtyAttributes;

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

    [Header("Settings")]
    [SerializeField] private ScenarioSo m_Scenario;
    [SerializeField] private VictoryRuleSo m_VictoryRule;
    [SerializeField] private float m_TileSize;
    [SerializeField] private float m_TileSpacing;

    [Header("Dictionary")]
    [SerializeField] private SerializedDictionary<Vector2, TileData> m_BoardInfo = new();

    private List<Pawn> P1_OnBoardPawns = new();
    private List<Pawn> P2_OnBoardPawns = new();

    private List<Pawn> P1_InReservePawns = new();
    private List<Pawn> P2_InReservePawns = new();

    private int m_TotalCells;
    private BoardState m_BoardState;
    private Pawn m_CurrentSelectedPawn;
    private Tile m_CurrentSelectedTile;
    private List<Tile> m_ReachableTiles = new();

    #region Setup

    private void Clear()
    {
        foreach (KeyValuePair<Vector2, TileData> tileData in m_BoardInfo)
        {
            Destroy(tileData.Value.Pawn.gameObject);
            Destroy(tileData.Value.Tile.gameObject);
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

        SetupBackground();
        SetupTiles();
        SetupPawns();
    }
    private void SetupBackground()
    {
        int randomNumber = Random.Range(0, m_BackgroundList.Count);
        m_BoardImage.sprite = m_BackgroundList[randomNumber];
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
                tile.Init(y == 0 ? Team.Player1 : y == m_Scenario.BoardSize.y - 1 ? Team.Player2 : Team.None);

                var tileData = new TileData();
                tileData.Tile = tile;
                m_BoardInfo.Add(new Vector2(x, y), tileData);
            }
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
            MovePawnTo(pawn, pos);
            P1_OnBoardPawns.Add(pawn);

            // PLAYER 2
            int reversedIndex = m_TotalCells - 1 - i;
            pawn = Instantiate(m_PawnPrefab);
            pos = new Vector2(reversedIndex % (int)m_Scenario.BoardSize.x, reversedIndex / (int)m_Scenario.BoardSize.x);
            pawn.Init(m_Scenario.Pieces[i], Team.Player2);
            MovePawnTo(pawn, pos);
            P2_OnBoardPawns.Add(pawn);
        }

        m_BoardState = BoardState.P1_PawnSelection;
        //GenerateMoveableCard();
    }

    private void Start()
    {
        SetupGame();
    }

    #endregion

    #region Gameplay

    public void OnTileClick(Vector2 pPosition)
    {
        Vector2 targetPos = WorldPositionToBoardPosition(pPosition);

        if (!m_BoardInfo.TryGetValue(targetPos, out TileData pTileData)) return;

        ProcessAction(pTileData, targetPos);
    }

    private void ProcessAction(TileData pTileData, Vector2 pTargetPos)
    {
        Tile tile = pTileData.Tile;
        Pawn pawn = pTileData.Pawn;


        if (m_BoardState == BoardState.Idle)
        {
            return;
        }
        else if (m_BoardState == BoardState.P1_PawnSelection && pawn != null && pawn.Team == Team.Player1)
        {
            SelectPawn(pTileData, pTargetPos);
        }
        else if (m_BoardState == BoardState.P2_PawnSelection && pawn != null && pawn.Team == Team.Player2)
        {
            SelectPawn(pTileData, pTargetPos);
        }
        else if ((m_BoardState == BoardState.P1_PawnMove || m_BoardState == BoardState.P2_PawnMove) && m_ReachableTiles.Contains(tile))
        {
            MovePawnTo(m_CurrentSelectedPawn, pTargetPos);
        }
        else if(m_BoardState == BoardState.P1_PawnMove || m_BoardState == BoardState.P2_PawnMove)
        {
            ClearMoveState();
            m_BoardState = m_BoardState == BoardState.P1_PawnMove ? BoardState.P1_PawnSelection : BoardState.P2_PawnSelection;
        }
    }

    private void SelectPawn(TileData pTileData, Vector2 pTargetPos)
    {
        m_CurrentSelectedPawn = pTileData.Pawn;
        m_CurrentSelectedTile = pTileData.Tile;

        m_CurrentSelectedTile.SetState(TileState.Selected);

        m_ReachableTiles.AddRange(GenerateReachableTileList(pTileData, pTargetPos));
        foreach (Tile reachableTile in m_ReachableTiles)
        {
            reachableTile.SetState(TileState.Highlighted);
        }

        m_BoardState = m_BoardState == BoardState.P1_PawnSelection ? BoardState.P1_PawnMove : BoardState.P2_PawnMove;
    }

    private List<Tile> GenerateReachableTileList(TileData pTileData, Vector2 pTargetPos)
    {
        List<Tile> reachableTileList = new();

        foreach (Vector2 range in pTileData.Pawn.PawnSo.Ranges)
        {
            Vector2 newPos;

            if (pTileData.Pawn.Team == Team.Player1)
            {
                newPos = new Vector2(pTargetPos.x + range.x, pTargetPos.y + range.y);
            }
            else
            {
                newPos = new Vector2(pTargetPos.x + range.x, pTargetPos.y - range.y);
            }

            if (m_BoardInfo.TryGetValue(newPos, out TileData tileData) 
                && (tileData.Pawn == null 
                || tileData.Pawn.Team != pTileData.Pawn.Team))
            {
                reachableTileList.Add(tileData.Tile);
            }
        }

        return reachableTileList;
    }

    private void MovePawnTo(Pawn pPawn, Vector2 pSelectedTarget)
    {
        if (!m_BoardInfo.TryGetValue(pSelectedTarget, out TileData tileData)) return;

        if (tileData.Pawn != null)
        {
            CapturePawn(tileData.Pawn);
        }

        tileData.Pawn = pPawn;
        pPawn.transform.SetParent(tileData.Tile.transform);
        pPawn.transform.position = tileData.Tile.transform.position;
        pPawn.transform.SetAsFirstSibling();

        ClearMoveState();

        m_BoardState = m_BoardState == BoardState.P1_PawnMove ? BoardState.P2_PawnSelection : BoardState.P1_PawnSelection;
    }

    private void CapturePawn(Pawn pPawn)
    {
        switch (pPawn.Team)
        {
            case Team.Player1:
                P2_InReservePawns.Add(pPawn);
                pPawn.transform.SetParent(m_P2ReserveParent);
                pPawn.Team = Team.Player2;
                break;
            case Team.Player2:
                P1_InReservePawns.Add(pPawn);
                pPawn.transform.SetParent(m_P1ReserveParent);
                pPawn.Team = Team.Player1;
                break;
        }
    }


    private void ClearMoveState()
    {
        m_CurrentSelectedTile?.SetState(TileState.None);
        m_CurrentSelectedTile = null;
        m_CurrentSelectedPawn = null;

        foreach (var reachableTile in m_ReachableTiles)
        {
            reachableTile.SetState(TileState.None);
        }

        m_ReachableTiles.Clear();
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

    [Button] private void Test()
    {
        MovePawnTo(P2_OnBoardPawns[0], Vector2.zero);
    }
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
    public Dictionary<Vector2, TileData> BoardState;

    public List<Pawn> P1_OnBoardPawns;
    public List<Pawn> P2_OnBoardPawns;

    public List<Pawn> P1_InReservePawns;
    public List<Pawn> P2_InReservePawns;
}

[System.Serializable]
public class TileData
{
    public Tile Tile;
    public Pawn Pawn;
}