using UnityEngine;
using YokaiNoMori.Enumeration;
using YokaiNoMori.Interface;

public class IA : MonoBehaviour, ICompetitor
{
    private string m_Name = "groupe 5-GRASSE-CORNET";
    private IGameManager m_Board;
    private ECampType m_ECampType;
    private float m_MaxThinkingTimer;

    public string GetName()
    {
        return m_Name;
    }
    public ECampType GetCamp()
    {
        return m_ECampType;
    }
    public void SetCamp(ECampType pCamp)
    {
        m_ECampType = pCamp;
    }
    public void Init(IGameManager pIGameManager, float timerForAI)
    {
        m_Board = pIGameManager;
        m_MaxThinkingTimer = timerForAI;
    }
    public void StartTurn()
    {
        throw new System.NotImplementedException();
    }
    public void StopTurn()
    {
        throw new System.NotImplementedException();
    }
}
