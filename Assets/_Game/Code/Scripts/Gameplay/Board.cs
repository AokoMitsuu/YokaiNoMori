using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;
using DG.Tweening;
using NaughtyAttributes;
using TMPro;
using UnityEngine.UI;
using System.Data;
using System.Collections;
using Unity.VisualScripting;

public class Board : MonoBehaviour
{
    [Header("Items")]
    [SerializeField] private SpriteRenderer m_BoardSprite;
    [SerializeField] private Transform m_ToRotate;
    [SerializeField] private Transform m_BoardParent;
    [SerializeField] private Tile m_TilePrefab;
    [SerializeField] private Pawn m_PawnPrefab;
    [SerializeField] private List<Sprite> m_BackgroundList = new();
    [SerializeField] private List<MusicSo> m_BackgroundMusics = new();
    [SerializeField] private List<Tile> P1_InReserveTiles;
    [SerializeField] private List<Tile> P2_InReserveTiles;
    [SerializeField] private Image m_TurnLabel;
    [SerializeField] private TMP_Text m_TurnText;

    [Header("Settings")]
    [SerializeField] private ScenarioSo m_Scenario;
    [SerializeField] private VictoryRuleSo m_VictoryRule;
    [SerializeField] private float m_TileSize;
    [SerializeField] private float m_TileSpacing;
    [SerializeField] private float m_RotateDelay;
    [SerializeField] private Color P1_Color;
    [SerializeField] private Color P2_Color;
    [SerializeField] private Color Draw_Color;
    [SerializeField] private float m_VictoryScreenDelay;

    [SerializeField, Foldout("Sound")] private SoundSo m_SoundBeginTurn;
    [SerializeField, Foldout("Sound")] private SoundSo m_SoundEndTurn;
    [SerializeField, Foldout("Sound")] private SoundSo m_SoundCapture;
    [SerializeField, Foldout("Sound")] private SoundSo m_SoundDrop;
    [SerializeField, Foldout("Sound")] private SoundSo m_SoundMove;
    [SerializeField, Foldout("Sound")] private SoundSo m_SoundSelect;
    [SerializeField, Foldout("Sound")] private SoundSo m_SoundToReserve;

    [Header("Dictionary")]
    [SerializeField] private SerializedDictionary<Vector2, Tile> m_BoardInfo = new();

    private List<Pawn> P1_OnBoardPawns = new();
    private List<Pawn> P2_OnBoardPawns = new();

    private List<Pawn> P1_InReservePawns = new();
    private List<Pawn> P2_InReservePawns = new();

    private List<string> P1_History = new();
    private List<string> P2_History = new();

    private int m_TotalCells;
    private BoardState m_BoardState;
    private Tile m_CurrentSelectedTile;
    private List<Tile> m_ReachableTiles = new();
    private Tween m_Rotation;
    private Tween m_Translation;
    private bool m_IsReserve;
    private Coroutine m_TurnCoroutine;

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

        m_Rotation.Kill(false);
        m_Rotation = null;

        m_BoardParent.transform.rotation = Quaternion.identity;

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

        if(m_TurnCoroutine != null)
            StopCoroutine(m_TurnCoroutine);

        m_TurnCoroutine = StartCoroutine(ShowLabel(P1_Color, "Au tour du joueur 1"));
    }
    private void SetupAmbience()
    {
        int randomNumber = Random.Range(0, m_BackgroundList.Count);
        m_BoardSprite.sprite = m_BackgroundList[randomNumber];
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
                tile.transform.localScale = new Vector3(m_TileSize, m_TileSize, 1);
                tile.name = $"{x}:{y}";
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
            MovePawnTo(pawn, m_BoardInfo[pos], true);
            P1_OnBoardPawns.Add(pawn);

            // PLAYER 2
            int reversedIndex = m_TotalCells - 1 - i;
            pawn = Instantiate(m_PawnPrefab);
            pos = new Vector2(reversedIndex % (int)m_Scenario.BoardSize.x, reversedIndex / (int)m_Scenario.BoardSize.x);
            pawn.Init(m_Scenario.Pieces[i], Team.Player2);
            MovePawnTo(pawn, m_BoardInfo[pos], true);
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
        m_IsReserve = pIsReserve;
        ProcessAction(pTile);
    }

    private void ProcessAction(Tile pTile)
    {
        Pawn pawn = pTile.Pawn;

        if (m_BoardState == BoardState.Idle)
        {
            return;
        }
        else if (m_BoardState == BoardState.P1_PawnSelection && pawn != null && pawn.Team == Team.Player1)
        {
            SelectPawn(pTile);
        }
        else if (m_BoardState == BoardState.P2_PawnSelection && pawn != null && pawn.Team == Team.Player2)
        {
            SelectPawn(pTile);
        }
        else if ((m_BoardState == BoardState.P1_PawnMove || m_BoardState == BoardState.P2_PawnMove) && m_ReachableTiles.Contains(pTile))
        {
            MovePawnTo(m_CurrentSelectedTile.Pawn, pTile);
        }
        else if(m_BoardState == BoardState.P1_PawnMove || m_BoardState == BoardState.P2_PawnMove)
        {
            ClearMoveState();
            m_BoardState = m_BoardState == BoardState.P1_PawnMove ? BoardState.P1_PawnSelection : BoardState.P2_PawnSelection;
        }
    }

    private void SelectPawn(Tile pTile)
    {
        m_CurrentSelectedTile = pTile;

        m_CurrentSelectedTile.SetState(TileState.Selected);

        m_ReachableTiles.AddRange(GenerateReachableTileList(pTile));

        m_SoundSelect.Play();

        foreach (Tile reachableTile in m_ReachableTiles)
        {
            reachableTile.SetState(TileState.Highlighted);
        }

        m_BoardState = m_BoardState == BoardState.P1_PawnSelection ? BoardState.P1_PawnMove : BoardState.P2_PawnMove;
    }

    private List<Tile> GenerateReachableTileList(Tile pTile)
    {
        List<Tile> reachableTileList = new();

        if (m_IsReserve)
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

    private void MovePawnTo(Pawn pPawn, Tile pTargetTile, bool pIsSetup = false)
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

        bool isWin = CheckWin(pIsSetup);

        if (pIsSetup) return;

        if (pTargetTile.TeamBackRow != Team.None && pTargetTile.TeamBackRow != pPawn.Team && !m_CurrentSelectedTile.IsReserve && !isWin)
            pPawn.Promote();

        if(m_IsReserve)
            m_SoundDrop.Play();
        else
            m_SoundMove.Play();

        m_SoundEndTurn.Play();

        ClearMoveState();

        if (!isWin)
        {
            if (m_BoardState == BoardState.P1_PawnMove)
            {
                RotateBoard(m_BoardState = BoardState.P2_PawnSelection);
                AddActionHistory(P1_History, $"{pPawn.PawnSo.name}-{pTargetTile.name}");
            }
            else
            {
                RotateBoard(m_BoardState = BoardState.P1_PawnSelection);
                AddActionHistory(P2_History, $"{pPawn.PawnSo.name}-{pTargetTile.name}");
            }
        }
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

        pPawn.Demote();
        m_SoundCapture.Play();
        m_SoundToReserve.Play();
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

    private bool CheckWin(bool pIsSetup)
    {
        if (pIsSetup) return false;

        if(CheckDraw()) // DRAW
        {
            Debug.Log("DRAW");
            StartCoroutine(EndScreen(Team.None));
            return true;
        }
        else
        {
            Team winner = m_VictoryRule.CheckVictory(m_BoardInfo, P1_OnBoardPawns, P2_OnBoardPawns, P1_InReservePawns, P2_InReservePawns);
            if (winner == Team.None) return false;

            StartCoroutine(EndScreen(winner));
            return true;
        }
    }
    private bool CheckDraw()
    {
        if (P1_History.Count < 6 || P2_History.Count < 6) return false;

        bool player1ActionA = P1_History[0] == P1_History[2] && P1_History[2] == P1_History[4];
        bool player1ActionB = P1_History[1] == P1_History[3] && P1_History[3] == P1_History[5];

        bool player2ActionA = P2_History[0] == P2_History[2] && P2_History[2] == P2_History[4];
        bool player2ActionB = P2_History[1] == P2_History[3] && P2_History[3] == P2_History[5];

        return player1ActionA && player1ActionB && player2ActionA && player2ActionB;
    }

    private void RotateBoard(BoardState pNewState)
    {
        m_BoardState = BoardState.Idle;

        if (m_TurnCoroutine != null)
            StopCoroutine(m_TurnCoroutine);

        if (pNewState == BoardState.P1_PawnSelection)
        {
            m_TurnCoroutine = StartCoroutine(ShowLabel(P1_Color, "Au tour du joueur 1"));
        }
        else
        {
            m_TurnCoroutine = StartCoroutine(ShowLabel(P2_Color, "Au tour du joueur 2"));
        }

        m_Rotation = m_ToRotate.transform.DORotate(new Vector3(0, 0, 180f), m_RotateDelay, RotateMode.LocalAxisAdd).SetEase(Ease.InOutBack).OnComplete(() => { m_BoardState = pNewState; m_SoundBeginTurn.Play(); });
    }
    private IEnumerator EndScreen(Team pWiner)
    {
        m_BoardState = BoardState.Idle;
        
        if (m_TurnCoroutine != null)
            StopCoroutine(m_TurnCoroutine);

        if (pWiner == Team.Player1)
        {
            yield return ShowLabel(P1_Color, "Joueur 1 a gagné", m_VictoryScreenDelay);
        }
        else if (pWiner == Team.Player2)
        {
            yield return ShowLabel(P2_Color, "Joueur 2 a gagné", m_VictoryScreenDelay);
        }
        else
        {
            yield return ShowLabel(Draw_Color, "Egalité", m_VictoryScreenDelay);
        }

        SetupGame();
    }

    private IEnumerator ShowLabel(Color pColor, string pText, float pDelay = 0)
    {
        m_TurnText.text = pText;
        m_TurnLabel.color = pColor;

        m_Translation.Kill(true);

        m_TurnLabel.rectTransform.position = new Vector3(-Screen.width, m_TurnLabel.rectTransform.position.y, m_TurnLabel.rectTransform.position.z);

        m_Translation = m_TurnLabel.rectTransform.DOAnchorPosX(0, m_RotateDelay).SetEase(Ease.OutExpo);

        yield return m_Translation.WaitForCompletion();

        yield return new WaitForSeconds(pDelay);

        m_Translation = m_TurnLabel.rectTransform.DOAnchorPosX(Screen.width, m_RotateDelay).SetEase(Ease.OutExpo);

        yield return m_Translation.WaitForCompletion();
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

        return new Vector3(centeredXOffset, centeredYOffset, -1);
    }

    private void AddActionHistory(List<string> pPlayerHistory, string action)
    {
        pPlayerHistory.Add(action);
        if(pPlayerHistory.Count > 6)
        {
            pPlayerHistory.RemoveAt(0);
        }
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