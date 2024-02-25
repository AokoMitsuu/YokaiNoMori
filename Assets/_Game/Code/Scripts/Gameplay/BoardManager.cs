using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    [Header("Board")]
    [SerializeField] private Image m_BoardImage;
    [SerializeField] private BoardTile m_TilePrefab;
    [SerializeField] private float m_TileSize;
    [SerializeField] private float m_TileSpacing;

    [Header("Pawns")]
    [SerializeField] private ScenarioSo m_Scenario;
    [SerializeField] private PawnController m_PawnPrefab;

    [Header("Settings")]
    [SerializeField] private List<Sprite> m_BoardList = new();

    private List<PawnController> m_Player1 = new();
    private List<PawnController> m_Player2 = new();

    private Dictionary<Vector2, BoardTile> m_Tiles = new();
    private List<BoardTile> m_ReachableTiles = new();

    private BoardTile m_AcutalTile = null;
    private PawnController m_AcutalPawn = null;

    private EBoardState m_BoardState;
    private int m_TotalCells;

    public static BoardManager Instance;

    private void Awake()
    {
        Instance = this;

        //m_Board.transform.localScale = new Vector3(
        //    (m_Scenario.BoardSize.x * m_TileSize) + ((m_Scenario.BoardSize.x + 1) * m_TileSpacing), 
        //    (m_Scenario.BoardSize.y * m_TileSize) + ((m_Scenario.BoardSize.y + 1) * m_TileSpacing), 
        //    1);

        SetupBackground();

        for (int x = 0; x < m_Scenario.BoardSize.x; x++)
        {
            for (int y = 0; y < m_Scenario.BoardSize.y; y++)
            {
                BoardTile tile = Instantiate(m_TilePrefab, BoardPositionToWorldPosition(x,y), Quaternion.identity);
                tile.transform.localScale = new Vector3(m_TileSize, m_TileSize, 1);    
                tile.transform.SetParent(m_BoardImage.transform);
                tile.Init(new Vector2(x, y), y == 0 ? ETeam.Player2 : y == m_Scenario.BoardSize.y -1 ? ETeam.Player1 : ETeam.None);

                m_Tiles.Add(new Vector2(x, y), tile);
            }
        }

        m_TotalCells = (int)m_Scenario.BoardSize.x * (int)m_Scenario.BoardSize.y;
        StartGame();
    }

    public void ClickOnTile(BoardTile tile)
    {
        if (m_BoardState == EBoardState.Idle) return;

        if(m_BoardState == EBoardState.Player1PawnSelection && tile.PieceController != null && tile.PieceController.Team == ETeam.Player1)
        {
            PlayerSelectPiece(tile);
        }
        else if (m_BoardState == EBoardState.Player2PawnSelection && tile.PieceController != null && tile.PieceController.Team == ETeam.Player2)
        {
            PlayerSelectPiece(tile);
        }
        else if ((m_BoardState == EBoardState.Player1PawnMove || m_BoardState == EBoardState.Player2PawnMove) && m_ReachableTiles.Contains(tile))
        {
            PlayerSelectMove(tile);
        }
        else if (m_BoardState == EBoardState.Player1PawnMove || m_BoardState == EBoardState.Player2PawnMove)
        {
            m_AcutalTile.SetSelected(false);
            m_AcutalTile = null;
            m_AcutalPawn = null;
            foreach (var reachableTile in m_ReachableTiles)
            {
                reachableTile.SetHighlight(false);
            }
            m_ReachableTiles.Clear();

            if (m_BoardState == EBoardState.Player1PawnMove)
                m_BoardState = EBoardState.Player1PawnSelection;
            else
                m_BoardState = EBoardState.Player2PawnSelection;
        }
    }
    public void RemovePiece(PawnController piece)
    {
        if (m_Player1.Contains(piece))
            m_Player1.Remove(piece);
        else
            m_Player2.Remove(piece);

        Destroy(piece.gameObject);
        CheckWin();
    }

    private void StartGame()
    {
        for (int i = 0; i < m_Scenario.Pieces.Count; i++)
        {
            if (m_Scenario.Pieces[i] == null) continue;

            // PLAYER 1
            var piece = Instantiate(m_PawnPrefab);
            piece.Init(ETeam.Player1, m_Scenario.Pieces[i]);
            m_Tiles[new Vector2(i % (int)m_Scenario.BoardSize.x, i / (int)m_Scenario.BoardSize.x)].SetPiece(piece);
            m_Player1.Add(piece);

            // PLAYER 2
            int reversedIndex = m_TotalCells - 1 - i;
            piece = Instantiate(m_PawnPrefab);
            piece.Init(ETeam.Player2, m_Scenario.Pieces[i]);
            m_Tiles[new Vector2(reversedIndex % (int)m_Scenario.BoardSize.x, reversedIndex / (int)m_Scenario.BoardSize.x)].SetPiece(piece);
            m_Player2.Add(piece);
        }

        m_BoardState = EBoardState.Player1PawnSelection;
    }
    private void Clear()
    {
        foreach (var player in m_Player1)
            Destroy(player.gameObject);

        foreach (var player in m_Player2)
            Destroy(player.gameObject);

        m_Player1.Clear();
        m_Player2.Clear();
    }
    private void PlayerSelectPiece(BoardTile tile)
    {
        m_AcutalTile = tile;
        m_AcutalTile.SetSelected(true);
        m_AcutalPawn = tile.PieceController;
        GenerateReachableTileList(tile.BoardPosition, m_AcutalPawn);
        foreach (var reachableTile in m_ReachableTiles)
        {
            reachableTile.SetHighlight(true);
        }

        if(m_BoardState == EBoardState.Player1PawnSelection)
            m_BoardState = EBoardState.Player1PawnMove;
        else
            m_BoardState = EBoardState.Player2PawnMove;
    }
    private void PlayerSelectMove(BoardTile tile)
    {
        m_AcutalTile.RemovePiece();
        m_AcutalTile.SetSelected(false);
        tile.SetPiece(m_AcutalPawn);
        m_AcutalPawn = null;
        foreach (var reachableTile in m_ReachableTiles)
        {
            reachableTile.SetHighlight(false);
        }
        m_ReachableTiles.Clear();

        if (m_BoardState == EBoardState.Player1PawnMove)
            m_BoardState = EBoardState.Player2PawnSelection;
        else
            m_BoardState = EBoardState.Player1PawnSelection;
    }
    private void GenerateReachableTileList(Vector2 pos, PawnController piece)
    {
        foreach(var range in piece.PawnSo.Ranges)
        {
            Vector2 newPos = Vector2.zero;

            if (m_AcutalPawn.Team == ETeam.Player1)
                newPos = new Vector2(pos.x + range.x, pos.y + range.y);
            else
                newPos = new Vector2(pos.x + range.x, pos.y - range.y);

            if (m_Tiles.TryGetValue(newPos, out var tile) && (tile.PieceController == null || tile.PieceController.Team != piece.Team))
                m_ReachableTiles.Add(tile);
        }
    }
    private Vector3 BoardPositionToWorldPosition(int x, int y)
    {
        float xOffset = x * (m_TileSize + m_TileSpacing);
        float yOffset = y * (m_TileSize + m_TileSpacing);

        float totalWidth = (m_Scenario.BoardSize.x * m_TileSize) + ((m_Scenario.BoardSize.x - 1) * m_TileSpacing);
        float totalHeight = (m_Scenario.BoardSize.y * m_TileSize) + ((m_Scenario.BoardSize.y - 1) * m_TileSpacing);

        float centeredXOffset = xOffset - totalWidth / 2 + m_TileSize / 2;
        float centeredYOffset = yOffset - totalHeight / 2 + m_TileSize / 2;

        return new Vector3(centeredXOffset, centeredYOffset, -1);
    }
    [Button]
    private void SetupBackground()
    {
        m_BoardImage.sprite = m_BoardList[Random.Range(0, m_BoardList.Count)];
    }
    private void CheckWin() //TODO rework : Victory if the enemy "king" is taken
    {
        if(m_Player1.Count != 0 && m_Player2.Count != 0)
            return;
        else if (m_Player1.Count == 0)
            Debug.Log("Player2 Won");
        else if(m_Player2.Count == 0)
            Debug.Log("Player1 Won");

        Clear();
        StartGame();
    }
}

public enum EBoardState
{
    Idle,
    Player1PawnSelection,
    Player1PawnMove,
    Player2PawnSelection,
    Player2PawnMove
}