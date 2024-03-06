using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

[CreateAssetMenu(menuName = "Rules/Basic", fileName = "Basic")]
public class BasicRuleSo : VictoryRuleSo
{
    [SerializeField] private PawnSo m_King;

    public override Team CheckVictory(
        Dictionary<Vector2, Tile> pBoardState,
        List<Pawn> pP1_OnBoardPawns,
        List<Pawn> pP2_OnBoardPawns,
        List<Pawn> pP1_InReservePawns,
        List<Pawn> pP2_InReservePawns
    )
    {
        Team winner = Team.None;

        if (!CheckKingAlive(pP1_OnBoardPawns) || CheckKingInBackrowAndCannotTakeBack(pBoardState, pP2_OnBoardPawns.Find((pawn) => pawn.PawnSo == m_King), pP1_OnBoardPawns, Team.Player1))
            winner = Team.Player2;
        else if(!CheckKingAlive(pP2_OnBoardPawns) || CheckKingInBackrowAndCannotTakeBack(pBoardState, pP1_OnBoardPawns.Find((pawn) => pawn.PawnSo == m_King), pP1_OnBoardPawns, Team.Player2))
            winner = Team.Player1;

        return winner;
    }

    private bool CheckKingAlive(List<Pawn> playerPawn)
    {
        return playerPawn.Any((pawn) => pawn.PawnSo == m_King);
    }

    private bool CheckKingInBackrowAndCannotTakeBack(Dictionary<Vector2, Tile> pBoardState, Pawn pKing, List<Pawn> pAdversePawn, Team pTeamBackRow)
    {
        Tile targetPos = pBoardState.FirstOrDefault(x => x.Value.Pawn == pKing).Value;

        return !GenerateReachableTileList(pBoardState, pAdversePawn).Contains(targetPos) && pTeamBackRow == targetPos.TeamBackRow;
    }

    private List<Tile> GenerateReachableTileList(Dictionary<Vector2, Tile> pBoardState, List<Pawn> pAdversePawn)
    {
        List<Tile> reachableTileList = new();

        foreach(Pawn pawn in pAdversePawn)
        {
            Vector2 targetPos = pBoardState.FirstOrDefault(x => x.Value.Pawn == pawn).Key;

            foreach (Vector2 range in pawn.PawnSo.Ranges)
            {
                Vector2 newPos;

                if (pawn.Team == Team.Player1)
                {
                    newPos = new Vector2(targetPos.x + range.x, targetPos.y + range.y);
                }
                else
                {
                    newPos = new Vector2(targetPos.x + range.x, targetPos.y - range.y);
                }

                if (pBoardState.TryGetValue(newPos, out Tile tile)
                    && (tile.Pawn == null
                    || tile.Pawn.Team != pawn.Team))
                {
                    reachableTileList.Add(tile);
                }
            }
        }

        return reachableTileList;
    }
}
