using System.Collections.Generic;
using UnityEngine;
using YokaiNoMori.Interface;
using YokaiNoMori.Enumeration;
using NaughtyAttributes;
using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System;

public class Board : MonoBehaviour, IGameManager
{
    [Header("Settings")]
    [SerializeField] private ScenarioSo m_Scenario;
    [SerializeField] private VictoryRuleSo m_VictoryRule;
    [SerializeField] private float m_TileSize;
    [SerializeField] private float m_TileSpacing;

    [Header("Animations Settings")]
    [SerializeField] private Color P1_Color;
    [SerializeField] private Color P2_Color;
    [SerializeField] private Color Draw_Color;

    [SerializeField] private TMP_Text m_TranslationText;
    [SerializeField] private Image m_TranslationImage;
    [SerializeField] private float m_TranslationDelay;
    [SerializeField] private float m_RotateDelay;
    [SerializeField] private float m_VictoryScreenDelay;
    [SerializeField] private Ease m_TranslationEase = Ease.OutExpo;
    [SerializeField] private Ease m_RotateEase = Ease.InOutBack;

    [Header("Prefabs")]
    [SerializeField] private TileDisplay m_TilePrefab;
    [SerializeField] private PawnDisplay m_PawnPrefab;

    [Header("Items")]
    [SerializeField] private Transform m_BoardSpawnPoint;
    [SerializeField] private Transform m_ToRotate;
    [SerializeField] private List<Transform> P1_ReserveSpawnPoint;
    [SerializeField] private List<Transform> P2_ReserveSpawnPoint;
    [SerializeField] private SpriteRenderer m_BoardSprite;
    [SerializeField] private List<Sprite> m_BoardSpriteList;

    [SerializeField, Foldout("Sound")] private SoundSo m_SoundBeginTurn;
    [SerializeField, Foldout("Sound")] private SoundSo m_SoundEndTurn;
    [SerializeField, Foldout("Sound")] private SoundSo m_SoundCapture;
    [SerializeField, Foldout("Sound")] private SoundSo m_SoundDrop;
    [SerializeField, Foldout("Sound")] private SoundSo m_SoundMove;
    [SerializeField, Foldout("Sound")] private SoundSo m_SoundSelect;
    [SerializeField, Foldout("Sound")] private SoundSo m_SoundToReserve;

    private List<TileData> m_TileList = new();

    private List<PawnData> P1_PawnList = new();
    private List<PawnData> P2_PawnList = new();

    private List<TileData> P1_ReserveList = new();
    private List<TileData> P2_ReserveList = new();

    private List<PawnDisplay> m_SpawnedPawnDisplays = new();
    private List<TileDisplay> m_SpawnedTileDisplays = new();

    private List<string> P1_History = new();
    private List<string> P2_History = new();

    private List<TileDisplay> m_ReachableTiles = new();

    private int m_TotalCells;
    private BoardState m_BoardState;
    private TileDisplay m_SelectedTileDisplay;

    private Coroutine m_TurnCoroutine;
    private Tween m_BoardRotation;
    private Tween m_LabelTranslation;

    private ICompetitor m_IA;

    #region Setup

    protected void Start()
    {
        Setup();
    }
    private void Setup()
    {
        Clear();
        SetupBoard();
        SetupAmbiance();
        SetupPawns();

        if (m_TurnCoroutine != null)
        {
            StopCoroutine(m_TurnCoroutine);
        }
        m_TurnCoroutine = StartCoroutine(ShowLabel(P1_Color, "Au tour du joueur 1"));

        m_IA = GetComponent<IA>();
        m_IA.Init(this, 180);

        m_BoardState = BoardState.P1_PawnSelection;
    }
    private void Clear()
    {
        foreach (PawnDisplay pawn in m_SpawnedPawnDisplays)
        {
            Destroy(pawn.gameObject);
        }
        foreach (TileDisplay tile in m_SpawnedTileDisplays)
        {
            Destroy(tile.gameObject);
        }

        m_TileList.Clear();

        P1_PawnList.Clear();
        P2_PawnList.Clear();

        P1_ReserveList.Clear();
        P2_ReserveList.Clear();

        m_SpawnedPawnDisplays.Clear();
        m_SpawnedTileDisplays.Clear();

        P1_History.Clear();
        P2_History.Clear();

        m_ReachableTiles.Clear();

        m_BoardRotation.Kill(false);
        m_BoardRotation = null;
        m_ToRotate.transform.rotation = Quaternion.identity;
    }
    private void SetupBoard()
    {
        m_TotalCells = (int)m_Scenario.BoardSize.x * (int)m_Scenario.BoardSize.y;

        for (int y = 0; y < m_Scenario.BoardSize.y; y++)
        {
            for (int x = 0; x < m_Scenario.BoardSize.x; x++)
            {
                //TileData
                TileData tileData = new();
                tileData.SetTeamBackRow(y == 0 ? ECampType.PLAYER_ONE : y == m_Scenario.BoardSize.y - 1 ? ECampType.PLAYER_TWO : ECampType.NONE);
                tileData.SetPosition(new Vector2Int(x, y));
                tileData.Board = this;
                m_TileList.Add(tileData);

                //TileDisplay
                TileDisplay tileDisplay = Instantiate(m_TilePrefab, BoardPositionToWorldPosition(x, y), Quaternion.identity);
                tileDisplay.transform.localScale = new Vector3(m_TileSize, m_TileSize, 1);
                tileDisplay.name = $"{x}:{y}";
                tileDisplay.transform.SetParent(m_BoardSpawnPoint);
                tileDisplay.SetTileData(tileData);
                m_SpawnedTileDisplays.Add(tileDisplay);
            }
        }

        for (int i = 0; i < P1_ReserveSpawnPoint.Count; i++)
        {
            TileData pTileData = new();
            TileDisplay tileDisplay = Instantiate(m_TilePrefab, P1_ReserveSpawnPoint[i]);
            pTileData.SetIsReserve();
            pTileData.Board = this;
            tileDisplay.SetTileData(pTileData);
            P1_ReserveList.Add(pTileData);
            m_SpawnedTileDisplays.Add(tileDisplay);

            pTileData = new();
            tileDisplay = Instantiate(m_TilePrefab, P2_ReserveSpawnPoint[i]);
            pTileData.SetIsReserve();
            pTileData.Board = this;
            tileDisplay.SetTileData(pTileData);
            P2_ReserveList.Add(pTileData);
            m_SpawnedTileDisplays.Add(tileDisplay);
        }
    }
    private void SetupAmbiance()
    {
        int randomNumber = UnityEngine.Random.Range(0, m_BoardSpriteList.Count);
        m_BoardSprite.sprite = m_BoardSpriteList[randomNumber];
    }
    private void SetupPawns()
    {
        for (int i = 0; i < m_Scenario.Pawns.Count; i++)
        {
            if (m_Scenario.Pawns[i] == null) continue;

            //PLAYER 1
            PawnDisplay pawnDisplay = Instantiate(m_PawnPrefab);
            Vector2Int pos = new Vector2Int(i % (int)m_Scenario.BoardSize.x, i / (int)m_Scenario.BoardSize.x);
            PawnData pawnData = new();
            pawnDisplay.SetPawnData(pawnData);
            pawnDisplay.name = m_Scenario.Pawns[i].name + "1";
            pawnData.Init(m_Scenario.Pawns[i], ECampType.PLAYER_ONE);
            MovePawnTo(pawnData, pawnDisplay.transform, m_TileList.Where(x => x.GetPosition() == pos).ToList().First(), true);
            P1_PawnList.Add(pawnData);
            m_SpawnedPawnDisplays.Add(pawnDisplay);

            //Flip
            int reversedIndex = m_TotalCells - 1 - i;

            //PLAYER 2
            pawnDisplay = Instantiate(m_PawnPrefab);
            pos = new Vector2Int(reversedIndex % (int)m_Scenario.BoardSize.x, reversedIndex / (int)m_Scenario.BoardSize.x);
            pawnData = new();
            pawnDisplay.SetPawnData(pawnData);
            pawnDisplay.name = m_Scenario.Pawns[i].name + "2";
            pawnData.Init(m_Scenario.Pawns[i], ECampType.PLAYER_TWO);
            MovePawnTo(pawnData, pawnDisplay.transform, m_TileList.Where(x => x.GetPosition() == pos).ToList().First(), true);
            P2_PawnList.Add(pawnData);
            m_SpawnedPawnDisplays.Add(pawnDisplay);
        }
    }

    #endregion

    #region Gameplay

    public void OnTileClick(TileDisplay pTileDisplay)
    {
        ProcessAction(pTileDisplay);
    }
    private void SelectPawn(TileDisplay pTileDisplay)
    {
        m_SelectedTileDisplay = pTileDisplay;

        m_SelectedTileDisplay.SetState(TileState.Selected);

        m_ReachableTiles.AddRange(GenerateReachableTileList(pTileDisplay.TileData));

        m_SoundSelect.Play();

        foreach (TileDisplay reachableTile in m_ReachableTiles)
        {
            reachableTile.SetState(TileState.Highlighted);
        }

        m_BoardState = m_BoardState == BoardState.P1_PawnSelection ? BoardState.P1_PawnMove : BoardState.P2_PawnMove;
    }
    private void ProcessAction(TileDisplay pTileDisplay)
    {
        PawnData pawnData = pTileDisplay.TileData.PawnData;

        if (m_BoardState == BoardState.Idle)
        {
            return;
        }
        else if (m_BoardState == BoardState.P1_PawnSelection && pawnData != null && pawnData.Team == ECampType.PLAYER_ONE)
        {
            SelectPawn(pTileDisplay);
        }
        else if (m_BoardState == BoardState.P2_PawnSelection && pawnData != null && pawnData.Team == ECampType.PLAYER_TWO)
        {
            SelectPawn(pTileDisplay);
        }
        else if ((m_BoardState == BoardState.P1_PawnMove || m_BoardState == BoardState.P2_PawnMove) && m_ReachableTiles.Contains(pTileDisplay))
        {
            PawnDisplay pawnDisplay = m_SpawnedPawnDisplays.Find(x => x.PawnData == m_SelectedTileDisplay.TileData.PawnData);
            MovePawnTo(m_SelectedTileDisplay.TileData.PawnData, pawnDisplay.transform, pTileDisplay.TileData);
        }
        else if (m_BoardState == BoardState.P1_PawnMove || m_BoardState == BoardState.P2_PawnMove)
        {
            ClearMoveState();
            m_BoardState = m_BoardState == BoardState.P1_PawnMove ? BoardState.P1_PawnSelection : BoardState.P2_PawnSelection;
        }
    }
    private void MovePawnTo(PawnData pPawn, Transform pPawnTransform, TileData pTargetTile, bool pIsSetup = false)
    {
        //Capture
        if (pTargetTile.PawnData != null) { CapturePawn(pTargetTile.PawnData, pTargetTile); }

        //Movement
        if (m_SelectedTileDisplay != null) { m_SelectedTileDisplay.TileData.SetPawn(null, null); }
        pTargetTile.SetPawn(pPawn, pPawnTransform);

        //History
        if (m_BoardState == BoardState.P1_PawnMove)
        {
            AddActionHistory(P1_History, $"{pPawn.PawnSo.name}-{pTargetTile.GetPosition()}");
        }
        else
        {
            AddActionHistory(P2_History, $"{pPawn.PawnSo.name}-{pTargetTile.GetPosition()}");
        }

        //Dont do the rest when setuping the game
        if (pIsSetup) return;

        bool isWin = CheckWin();

        if (pTargetTile.TeamBackRow != ECampType.NONE && pTargetTile.TeamBackRow != pPawn.Team && !m_SelectedTileDisplay.TileData.IsReserve && !isWin)
            pPawn.Promote();

        if (m_SelectedTileDisplay.TileData.IsReserve)
            m_SoundDrop.Play();
        else
            m_SoundMove.Play();

        m_SoundEndTurn.Play();

        ClearMoveState();

        if (!isWin)
        {
            if (m_BoardState == BoardState.P1_PawnMove && App.Instance.SelectedGamemode == App.Gamemode.PvP)
            {
                m_BoardState = BoardState.P2_PawnSelection;
            }
            else if(App.Instance.SelectedGamemode == App.Gamemode.PvP || m_BoardState == BoardState.IA_Turn)
            {
                m_BoardState = BoardState.P1_PawnSelection;
            }
            else
            {
                m_BoardState = BoardState.IA_Turn;
            }

            StartCoroutine(SwitchTurn(m_BoardState));
        }
    }
    private void CapturePawn(PawnData pPawn,  TileData pTileData)
    {
        pTileData.SetPawn(null, null);
        Transform pawnTransform = m_SpawnedPawnDisplays.Find(x => x.PawnData == pPawn).transform;

        switch (pPawn.Team)
        {
            case ECampType.PLAYER_ONE:
                P1_PawnList.Remove(pPawn);
                P2_ReserveList.Find(x => x.PawnData == null).SetPawn(pPawn, pawnTransform);
                pPawn.Team = ECampType.PLAYER_TWO;
                break;

            case ECampType.PLAYER_TWO:
                P2_PawnList.Remove(pPawn);
                P1_ReserveList.Find(x => x.PawnData == null).SetPawn(pPawn, pawnTransform);
                pPawn.Team = ECampType.PLAYER_ONE;
                break;
        }

        pPawn.Demote();
        m_SoundCapture.Play();
        m_SoundToReserve.Play();
    }
    private void AddActionHistory(List<string> pPlayerHistory, string action)
    {
        pPlayerHistory.Add(action);
        if (pPlayerHistory.Count > 6)
        {
            pPlayerHistory.RemoveAt(0);
        }
    }
    private bool CheckWin()
    {
        if (CheckDraw()) // DRAW
        {
            Debug.Log("DRAW");
            StartCoroutine(EndScreen(ECampType.NONE));
            return true;
        }
        else
        {
            ECampType winner = m_VictoryRule.CheckVictory(m_TileList, P1_PawnList, P2_PawnList);
            if (winner == ECampType.NONE) return false;
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
    private void ClearMoveState()
    {
        m_SelectedTileDisplay?.SetState(TileState.None);
        m_SelectedTileDisplay = null;

        foreach (var reachableTile in m_ReachableTiles)
        {
            reachableTile.SetState(TileState.None);
        }

        m_ReachableTiles.Clear();
    }

    #endregion

    #region Tweens

    private IEnumerator SwitchTurn(BoardState pNewState)
    {
        m_BoardState = BoardState.Idle;

        if (m_TurnCoroutine != null)
            StopCoroutine(m_TurnCoroutine);

        if(App.Instance.SelectedGamemode == App.Gamemode.PvP)
        {
            if (pNewState == BoardState.P1_PawnSelection)
            {
                m_TurnCoroutine = StartCoroutine(ShowLabel(P1_Color, "Au tour du joueur 1"));
                m_BoardRotation = m_ToRotate.DORotate(new Vector3(0, 0, 180f), m_RotateDelay, RotateMode.LocalAxisAdd)
                .SetEase(m_RotateEase)
                .OnComplete(() =>
                {
                    m_BoardState = pNewState;
                    m_SoundBeginTurn.Play();
                });
            }
            else if (pNewState == BoardState.P2_PawnSelection)
            {
                m_TurnCoroutine = StartCoroutine(ShowLabel(P2_Color, "Au tour du joueur 2"));
                m_BoardRotation = m_ToRotate.DORotate(new Vector3(0, 0, 180f), m_RotateDelay, RotateMode.LocalAxisAdd)
                .SetEase(m_RotateEase)
                .OnComplete(() =>
                {
                    m_BoardState = pNewState;
                    m_SoundBeginTurn.Play();
                });
            }
        }
        else
        {
            if (pNewState == BoardState.IA_Turn)
            {
                yield return ShowLabel(P2_Color, "Au tour du joueur 2");
                m_BoardState = pNewState;
                m_IA.GetDatas();
                m_IA.StartTurn();
                m_SoundBeginTurn.Play();
            }
            else
            {
                yield return ShowLabel(P1_Color, "Au tour du joueur 1");
                m_BoardState = pNewState;
            }
        }
    }
    private IEnumerator ShowLabel(Color pColor, string pText, float pDelay = 0)
    {
        m_TranslationText.text = pText;
        m_TranslationImage.color = pColor;

        //Reset
        m_LabelTranslation.Kill(true);
        m_TranslationImage.rectTransform.position = new Vector3(-Screen.width, m_TranslationImage.rectTransform.position.y, m_TranslationImage.rectTransform.position.z);

        //Move
        m_LabelTranslation = m_TranslationImage.rectTransform.DOAnchorPosX(0, m_TranslationDelay).SetEase(m_TranslationEase);
        yield return m_LabelTranslation.WaitForCompletion();
        yield return new WaitForSeconds(pDelay);
        m_LabelTranslation = m_TranslationImage.rectTransform.DOAnchorPosX(Screen.width, m_TranslationDelay).SetEase(m_TranslationEase);
        yield return m_LabelTranslation.WaitForCompletion();
    }
    private IEnumerator EndScreen(ECampType pWiner)
    {
        m_BoardState = BoardState.Idle;

        if (m_TurnCoroutine != null) StopCoroutine(m_TurnCoroutine);

        if (pWiner == ECampType.PLAYER_ONE)
        {
            yield return ShowLabel(P1_Color, "Joueur 1 a gagné", m_VictoryScreenDelay);
        }
        else if (pWiner == ECampType.PLAYER_TWO)
        {
            yield return ShowLabel(P2_Color, "Joueur 2 a gagné", m_VictoryScreenDelay);
        }
        else
        {
            yield return ShowLabel(Draw_Color, "Egalité", m_VictoryScreenDelay);
        }

        Setup();
    }

    #endregion

    #region Utils

    public (List<TileData>, List<TileData>, List<TileData>) GetBoardState()
    {
        return (m_TileList, P1_ReserveList, P2_ReserveList);
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
    private List<TileDisplay> GenerateReachableTileList(TileData pTile)
    {
        List<TileDisplay> reachableTileList = new();

        if (pTile.IsReserve)
        {
            foreach (TileDisplay tileDisplay in m_SpawnedTileDisplays)
            {
                if (tileDisplay.TileData.PawnData == null && !tileDisplay.TileData.IsReserve)
                {
                    reachableTileList.Add(tileDisplay);
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

                TileDisplay tileDisplay = m_SpawnedTileDisplays.Find(x => x.TileData.GetPosition().Equals(newPos));

                if (tileDisplay != null && (tileDisplay.TileData.GetPawnOnIt() == null || tileDisplay.TileData.PawnData.Team != pTile.PawnData.Team))
                {
                    reachableTileList.Add(tileDisplay);
                }

            }
        }

        return reachableTileList;
    }

    #endregion

    //INTERFACE
    public List<IPawn> GetAllPawn()
    {
        List<IPawn> result = new List<IPawn>(P1_PawnList);
        result.AddRange(P2_PawnList);
        return result;
    }
    public List<IBoardCase> GetAllBoardCase()
    {
        return new List<IBoardCase>(m_TileList);
    }
    public void DoAction(IPawn pawnTarget, Vector2Int position, EActionType actionType)
    {
        PawnData selectedPawn;
        PawnDisplay selectedPawnDisplay;
        TileData targetTile;

        if (actionType == EActionType.MOVE)
        {
            selectedPawn = m_TileList.Find(tile => tile.GetPosition() == pawnTarget.GetCurrentPosition()).PawnData;
        }
        else
        {
            selectedPawn = P2_ReserveList.Find(tile => tile.PawnData.GetPawnType() == pawnTarget.GetPawnType()).PawnData;
        }

        selectedPawnDisplay = m_SpawnedPawnDisplays.Find(display => display.PawnData == selectedPawn);
        m_SelectedTileDisplay = m_SpawnedTileDisplays.Find(display => display.TileData.PawnData == selectedPawn);
        targetTile = m_TileList.Find(tile => tile.GetPosition() == position);
        MovePawnTo(selectedPawn, selectedPawnDisplay.transform, targetTile);
    }
    public List<IPawn> GetReservePawnsByPlayer(ECampType campType)
    {
        List<IPawn> result = new();

        List<TileData> campList = campType == ECampType.PLAYER_ONE ? P1_ReserveList : P2_ReserveList;

        foreach (TileData tile in campList)
        {
            result.Add(tile.GetPawnOnIt());
        }

        return result;
    }
    public List<IPawn> GetPawnsOnBoard(ECampType campType)
    {
        List<IPawn> result = new();

        List<PawnData> campList = campType == ECampType.PLAYER_ONE ? P1_PawnList : P2_PawnList;

        foreach (PawnData pawn in campList)
        {
            result.Add(pawn);
        }

        return result;
    }
}

public enum BoardState
{
    Idle,
    P1_PawnSelection,
    P1_PawnMove,
    P2_PawnSelection,
    P2_PawnMove,
    IA_Turn
}