using UnityEngine;

public class InitMusic : MonoBehaviour
{
    [SerializeField] private MusicSo[] m_MusicsToPlay;

    protected void Start()
    {
        App.Instance.AudioManager.StopAllMusicsExcept(m_MusicsToPlay);

        foreach (MusicSo musicSo in m_MusicsToPlay)
        {
            if (!App.Instance.AudioManager.IsPlaying(musicSo))
            {
                App.Instance.AudioManager.Play(musicSo);
            }
        }
    }
}
