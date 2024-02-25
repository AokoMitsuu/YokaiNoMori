using NaughtyAttributes;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class SceneManager : MonoBehaviour
    {
        [SerializeField][Scene] private string m_AppScene;
        [SerializeField][Scene] private string m_FirstScene;

        private string m_CurrentScene = null;
        private bool m_IsSwitching = false;

        public void SwitchScene(string pSceneToLoad)
        {
            StartCoroutine(SwitchSceneCoroutine(pSceneToLoad));
        }

        private IEnumerator SwitchSceneCoroutine(string pSceneToLoad)
        {
            if (m_IsSwitching) yield break;

            //Begin
            m_IsSwitching = true;

            //Unload old scene if there is one
            if (!string.IsNullOrEmpty(m_CurrentScene))
            {
                //Fade in
                yield return App.Instance.TransitionManager.TransitionIn();
                yield return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(m_CurrentScene);
            }

            //Switch
            yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(pSceneToLoad, LoadSceneMode.Additive);
            m_CurrentScene = pSceneToLoad;

            //Fade out
            yield return App.Instance.TransitionManager.TransitionOut();

            //End
            m_IsSwitching = false;
        }

        protected void Awake()
        {
#if UNITY_EDITOR
            switch (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex)
            {
                case 0:
                    SwitchScene(m_FirstScene);
                    break;
                default:
                    m_CurrentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                    UnityEngine.SceneManagement.SceneManager.LoadScene(m_AppScene, LoadSceneMode.Additive);
                    break;
            }
#else
            SwitchScene(m_FirstScene);
#endif
        }
    }
}
