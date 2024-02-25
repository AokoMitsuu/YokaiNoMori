using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    public class TransitionManager : MonoBehaviour
    {
        [SerializeField] private Image m_BlackCurtain;
        [SerializeField] private float m_FadeDuration = 0.5f;
        [SerializeField] private Ease m_FadeEase = Ease.InOutSine;

        public IEnumerator TransitionIn()
        {
            m_BlackCurtain.gameObject.SetActive(true);
            yield return m_BlackCurtain.DOFade(1, m_FadeDuration).SetEase(m_FadeEase).WaitForCompletion();
        }
        public IEnumerator TransitionOut()
        {
            yield return m_BlackCurtain.DOFade(0, m_FadeDuration).SetEase(m_FadeEase).WaitForCompletion();
            m_BlackCurtain.gameObject.SetActive(false);
        }

        protected void Awake()
        {
            m_BlackCurtain.gameObject.SetActive(false);
        }
    }
}
