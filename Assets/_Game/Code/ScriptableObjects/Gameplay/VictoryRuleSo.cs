using System.Collections.Generic;
using UnityEngine;
using YokaiNoMori.Enumeration;

public abstract class VictoryRuleSo : ScriptableObject
{
    public abstract ECampType CheckVictory(
        List<TileData> pTileList, 
        List<PawnData> pP1_OnBoardPawns, 
        List<PawnData> pP2_OnBoardPawns
        );
}
