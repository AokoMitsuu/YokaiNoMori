using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class VictoryRuleSo : ScriptableObject
{
    public abstract ETeam CheckVictory(List<PawnController> pPlayer1, List<PawnController> pPlayer2);
}
