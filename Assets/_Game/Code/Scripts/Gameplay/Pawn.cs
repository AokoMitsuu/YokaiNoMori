using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Pawn : MonoBehaviour
{
    [SerializeField] private SpriteRenderer m_Sprite;
    [SerializeField] private Animator m_PromoteAnimator;

    private PawnSo m_PawnSo;
    private Team m_Team;

    public PawnSo PawnSo => m_PawnSo;
    public Team Team
    {
        get => m_Team;
        set
        {
            m_Team = value;
            transform.localEulerAngles = value == Team.Player1 ? new Vector3(0f, 0f, 0f) : new Vector3(0f, 0f, 180f);
        }
    }

    public void Init(PawnSo data, Team team)
    {
        m_PawnSo = data;
        Team = team;
        m_Sprite.sprite = data.Sprite;
    }

    public void Promote()
    {
        if (m_PawnSo.PromotedPawn == null) return;
        StartCoroutine(PromoteEffect());
    }

    public void Demote()
    {
        if (m_PawnSo.DemotedPawn == null) return;

        Init(m_PawnSo.DemotedPawn, m_Team);
    }

    private IEnumerator PromoteEffect()
    {
        m_PromoteAnimator.gameObject.SetActive(true);
        yield return new WaitForSeconds(m_PromoteAnimator.runtimeAnimatorController.animationClips[0].length);
        Init(m_PawnSo.PromotedPawn, m_Team);
        yield return new WaitForSeconds(2f);
        m_PromoteAnimator.gameObject.SetActive(false);
    }
}

public enum Team
{
    None,
    Player1,
    Player2,
}