using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YokaiNoMori.Enumeration;

[CreateAssetMenu(menuName = "Rules/Basic", fileName = "Basic")]
public class BasicRuleSo : VictoryRuleSo
{
    [SerializeField] private PawnSo m_King;

    public override ECampType CheckVictory(
        List<TileData> pTileList,
        List<PawnData> pP1_OnBoardPawns,
        List<PawnData> pP2_OnBoardPawns
    )
    {
        ECampType winner = ECampType.NONE;

        if (!CheckKingAlive(pP1_OnBoardPawns) || CheckKingInBackrowAndCannotTakeBack(pTileList, pP2_OnBoardPawns.Find((pawn) => pawn.PawnSo == m_King), pP1_OnBoardPawns, ECampType.PLAYER_ONE))
            winner = ECampType.PLAYER_TWO;
        else if(!CheckKingAlive(pP2_OnBoardPawns) || CheckKingInBackrowAndCannotTakeBack(pTileList, pP1_OnBoardPawns.Find((pawn) => pawn.PawnSo == m_King), pP1_OnBoardPawns, ECampType.PLAYER_TWO))
            winner = ECampType.PLAYER_ONE;

        return winner;
    }

    private bool CheckKingAlive(List<PawnData> playerPawn)
    {
        return playerPawn.Any((pawn) => pawn.PawnSo == m_King);
    }

    private bool CheckKingInBackrowAndCannotTakeBack(List<TileData> pTileList, PawnData pKing, List<PawnData> pAdversePawn, ECampType pTeamBackRow)
    {
        return pTeamBackRow == pKing.CurrentTile.TeamBackRow && !GenerateReachableTileList(pTileList, pAdversePawn).Contains(pKing.CurrentTile);
    }

    private List<TileData> GenerateReachableTileList(List<TileData> pTileList, List<PawnData> pAdversePawn)
    {
        List<TileData> reachableTileList = new();

        foreach(PawnData pawn in pAdversePawn)
        {
            Vector2Int targetPos = pawn.CurrentTile.GetPosition();

            foreach (Vector2Int range in pawn.PawnSo.Ranges)
            {
                Vector2Int newPos;

                if (pawn.Team == ECampType.PLAYER_ONE)
                {
                    newPos = new Vector2Int(targetPos.x + range.x, targetPos.y + range.y);
                }
                else
                {
                    newPos = new Vector2Int(targetPos.x + range.x, targetPos.y - range.y);
                }

                TileData tileData = pTileList.Find(x => x.GetPosition().Equals(newPos));

                if (tileData != null && (tileData.GetPawnOnIt() == null || tileData.PawnData.Team != pawn.Team))
                {
                    reachableTileList.Add(tileData);
                }
            }
        }

        return reachableTileList;
    }
}
