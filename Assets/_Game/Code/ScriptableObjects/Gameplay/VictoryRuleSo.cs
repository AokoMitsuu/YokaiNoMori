using System.Collections.Generic;
using UnityEngine;

public abstract class VictoryRuleSo : ScriptableObject
{
    public abstract Team CheckVictory(List<Pawn> pPlayer1, List<Pawn> pPlayer2);
}
