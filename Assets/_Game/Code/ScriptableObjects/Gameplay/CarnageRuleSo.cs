using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Rules/Carnage", fileName = "Carnage")]
public class CarnageRuleSo : VictoryRuleSo
{
    public override ETeam CheckVictory(List<PawnController> pPlayer1, List<PawnController> pPlayer2)
    {
        if (pPlayer1.Count == 0)
            return ETeam.Player2;
        else if (pPlayer2.Count == 0)
            return ETeam.Player1;
        else
            return ETeam.None;
    }
}
