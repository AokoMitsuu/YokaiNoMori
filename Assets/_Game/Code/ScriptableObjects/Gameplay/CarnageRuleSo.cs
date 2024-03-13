using System.Collections.Generic;
using UnityEngine;
using YokaiNoMori.Enumeration;

[CreateAssetMenu(menuName = "Rules/Carnage", fileName = "Carnage")]
public class CarnageRuleSo : VictoryRuleSo
{
    public override ECampType CheckVictory(
        List<TileData> pTileList, 
        List<PawnData> pP1_OnBoardPawns, 
        List<PawnData> pP2_OnBoardPawns
    )
    {
        if (pP1_OnBoardPawns.Count == 0)
            return ECampType.PLAYER_TWO;
        else if (pP2_OnBoardPawns.Count == 0)
            return ECampType.PLAYER_ONE;
        else
            return ECampType.NONE;
    }
}
