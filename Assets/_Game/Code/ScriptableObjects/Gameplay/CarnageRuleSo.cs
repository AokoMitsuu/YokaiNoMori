using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Rules/Carnage", fileName = "Carnage")]
public class CarnageRuleSo : VictoryRuleSo
{
    public override Team CheckVictory(
        Dictionary<Vector2, Tile> pBoardState, 
        List<Pawn> pP1_OnBoardPawns, 
        List<Pawn> pP2_OnBoardPawns, 
        List<Pawn> pP1_InReservePawns, 
        List<Pawn> pP2_InReservePawns
    )
    {
        if (pP1_OnBoardPawns.Count == 0)
            return Team.Player2;
        else if (pP2_OnBoardPawns.Count == 0)
            return Team.Player1;
        else
            return Team.None;
    }
}
