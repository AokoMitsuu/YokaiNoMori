using System.Collections;
using UnityEngine;

public class PawnDisplay : MonoBehaviour
{
    [SerializeField] private SpriteRenderer m_Sprite;
    [SerializeField] private Animator m_PromoteAnimator;

    private PawnData m_PawnData;

    public PawnData PawnData => m_PawnData;

    public void SetPawnData(PawnData pPawnData)
    {
        m_PawnData = pPawnData;

        m_PawnData.OnPawnSoRefresh += SetSprite;
        m_PawnData.OnTeamChange += TeamChanged;
        m_PawnData.OnPromote += Promote;
    }

    private void TeamChanged(Vector3 pRotation)
    {
        transform.localEulerAngles = pRotation;
    }
    private void SetSprite(Sprite pSprite)
    {
        m_Sprite.sprite = pSprite;
    }

    private void Promote()
    {
        StartCoroutine(PromoteEffect());
    }
    private IEnumerator PromoteEffect()
    {
        m_PromoteAnimator.gameObject.SetActive(true);
        yield return new WaitForSeconds(m_PromoteAnimator.runtimeAnimatorController.animationClips[0].length);
        m_PawnData.Init(m_PawnData.PawnSo.PromotedPawn, m_PawnData.Team, m_PawnData.GetCurrentOwner());
        yield return new WaitForSeconds(2f);
        m_PromoteAnimator.gameObject.SetActive(false);
    }

    protected void OnDestroy()
    {
        m_PawnData.OnPawnSoRefresh -= SetSprite;
        m_PawnData.OnTeamChange -= TeamChanged;
        m_PawnData.OnPromote -= Promote;
    }
}
