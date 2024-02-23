using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    [Header("Board Custom")]
    [SerializeField] private BoardTile m_TilePrefab;
    [SerializeField] private float m_TileSize;
    [SerializeField] private float m_TileSpacing;
    [SerializeField] private GameObject m_Board;

    [Space(10), Header("Pieces")]
    [SerializeField] private ScenarioSo m_Scenario;
    [SerializeField] private PieceController m_PieceControllerPrefab;

    private List<PieceController> m_Player1 = new();
    private List<PieceController> m_Player2 = new();

    private Dictionary<Vector2, BoardTile> m_Tiles = new();
    private List<BoardTile> m_ReachableTiles = new();

    private BoardTile m_AcutalTile = null;
    private PieceController m_AcutalPiece = null;

    private EBoardState m_BoardState;
    private int m_TotalCell;

    private void Awake()
    {
        Instance = this;

        m_Board.transform.localScale = new Vector3(
            (m_Scenario.BoardSize.x * m_TileSize) + ((m_Scenario.BoardSize.x + 1) * m_TileSpacing), 
            (m_Scenario.BoardSize.y * m_TileSize) + ((m_Scenario.BoardSize.y + 1) * m_TileSpacing), 
            1);

        for (int x = 0; x < m_Scenario.BoardSize.x; x++)
        {
            for (int y = 0; y < m_Scenario.BoardSize.y; y++)
            {
                BoardTile tile = Instantiate(m_TilePrefab, BoardPositionToWorldPosition(x,y), Quaternion.identity);
                tile.transform.localScale = new Vector3(m_TileSize, m_TileSize, 1);    
                tile.transform.SetParent(m_Board.transform);
                tile.Init(new Vector2(x, y), y == 0 ? ETeam.Player2 : y == m_Scenario.BoardSize.y -1 ? ETeam.Player1 : ETeam.None);

                m_Tiles.Add(new Vector2(x, y), tile);
            }
        }

        m_TotalCell = (int)m_Scenario.BoardSize.x * (int)m_Scenario.BoardSize.y;
        StartGame();
    }

    public void ClickOnTile(BoardTile tile)
    {
        if (m_BoardState == EBoardState.Idle) return;

        if(m_BoardState == EBoardState.Player1SelectPiece && tile.PieceController != null && tile.PieceController.Team == ETeam.Player1)
        {
            PlayerSelectPiece(tile);
        }
        else if (m_BoardState == EBoardState.Player2SelectPiece && tile.PieceController != null && tile.PieceController.Team == ETeam.Player2)
        {
            PlayerSelectPiece(tile);
        }
        else if ((m_BoardState == EBoardState.Player1SelectMove || m_BoardState == EBoardState.Player2SelectMove) && m_ReachableTiles.Contains(tile))
        {
            PlayerSelectMove(tile);
        }
        else if (m_BoardState == EBoardState.Player1SelectMove || m_BoardState == EBoardState.Player2SelectMove)
        {
            m_AcutalTile.SetSelected(false);
            m_AcutalTile = null;
            m_AcutalPiece = null;
            foreach (var reachableTile in m_ReachableTiles)
            {
                reachableTile.SetHighlight(false);
            }
            m_ReachableTiles.Clear();

            if (m_BoardState == EBoardState.Player1SelectMove)
                m_BoardState = EBoardState.Player1SelectPiece;
            else
                m_BoardState = EBoardState.Player2SelectPiece;
        }
    }

    public void RemovePiece(PieceController piece)
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
            var piece = Instantiate(m_PieceControllerPrefab);
            piece.Init(ETeam.Player1, m_Scenario.Pieces[i]);
            m_Tiles[new Vector2(i % (int)m_Scenario.BoardSize.x, i / (int)m_Scenario.BoardSize.x)].SetPiece(piece);
            m_Player1.Add(piece);

            // PLAYER 2
            int reversedIndex = m_TotalCell - 1 - i;
            piece = Instantiate(m_PieceControllerPrefab);
            piece.Init(ETeam.Player2, m_Scenario.Pieces[i]);
            m_Tiles[new Vector2(reversedIndex % (int)m_Scenario.BoardSize.x, reversedIndex / (int)m_Scenario.BoardSize.x)].SetPiece(piece);
            m_Player2.Add(piece);
        }

        m_BoardState = EBoardState.Player1SelectPiece;
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
        m_AcutalPiece = tile.PieceController;
        GenerateReachableTileList(tile.BoardPosition, m_AcutalPiece);
        foreach (var reachableTile in m_ReachableTiles)
        {
            reachableTile.SetHighlight(true);
        }

        if(m_BoardState == EBoardState.Player1SelectPiece)
            m_BoardState = EBoardState.Player1SelectMove;
        else
            m_BoardState = EBoardState.Player2SelectMove;
    }

    private void PlayerSelectMove(BoardTile tile)
    {
        m_AcutalTile.RemovePiece();
        m_AcutalTile.SetSelected(false);
        tile.SetPiece(m_AcutalPiece);
        m_AcutalPiece = null;
        foreach (var reachableTile in m_ReachableTiles)
        {
            reachableTile.SetHighlight(false);
        }
        m_ReachableTiles.Clear();

        if (m_BoardState == EBoardState.Player1SelectMove)
            m_BoardState = EBoardState.Player2SelectPiece;
        else
            m_BoardState = EBoardState.Player1SelectPiece;
    }

    private void GenerateReachableTileList(Vector2 pos, PieceController piece)
    {
        foreach(var range in piece.PieceSo.Ranges)
        {
            Vector2 newPos = Vector2.zero;

            if (m_AcutalPiece.Team == ETeam.Player1)
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

    private void CheckWin()
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
    Player1SelectPiece,
    Player1SelectMove,
    Player2SelectPiece,
    Player2SelectMove
}