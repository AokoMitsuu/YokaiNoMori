using UnityEngine;
using YokaiNoMori.Enumeration;
using YokaiNoMori.Interface;

public class IA : MonoBehaviour, ICompetitor
{
    private string m_Name = "groupe 5-GRASSE-CORNET";
    private IGameManager m_Board;
    private ECampType m_ECampType;

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
    public void Init(IGameManager pIGameManager)
    {
        m_Board = pIGameManager;
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
