using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Rules/Carnage", fileName = "Carnage")]
public class CarnageRuleSo : VictoryRuleSo
{
    public override Team CheckVictory(List<Pawn> pPlayer1, List<Pawn> pPlayer2)
    {
        if (pPlayer1.Count == 0)
            return Team.Player2;
        else if (pPlayer2.Count == 0)
            return Team.Player1;
        else
            return Team.None;
    }
}
