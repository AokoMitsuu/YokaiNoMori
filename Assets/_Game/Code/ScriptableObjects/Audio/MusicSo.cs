using UnityEngine;

[CreateAssetMenu(menuName = "Audio/MusicSo", fileName = "New MusicSo")]
public class MusicSo : ScriptableObject
{
    [SerializeField] private AudioClip m_AudioClip;
    [SerializeField][Range(0f, 1f)] private float m_Volume = 0.5f;
    [SerializeField] private bool m_Loop;
    private AudioSource m_AudioSource;

    public AudioClip AudioClip { get { return m_AudioClip; } }
    public float Volume { get { return m_Volume; } }
    public bool Loop { get { return m_Loop; } }
    public AudioSource AudioSource { get { return m_AudioSource; } set { m_AudioSource = value; } }

    public void Play()
    {
        App.Instance.AudioManager.Play(this);
    }
}
