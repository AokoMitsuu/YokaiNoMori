using System.Collections.Generic;
using UnityEngine;

public abstract class VictoryRuleSo : ScriptableObject
{
    public abstract Team CheckVictory(Dictionary<Vector2, Tile> pBoardState, List<Pawn> pP1_OnBoardPawns, List<Pawn> pP2_OnBoardPawns, List<Pawn> pP1_InReservePawns, List<Pawn> pP2_InReservePawns);
}
