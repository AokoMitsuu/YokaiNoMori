using UnityEngine;

[CreateAssetMenu(menuName = "Audio/SoundSo", fileName = "New SoundSo")]
public class SoundSo : ScriptableObject
{
    [SerializeField] private AudioClip m_AudioClip;
    [SerializeField][Range(0f, 1f)] private float m_Volume = 0.5f;
    [SerializeField][Range(0.1f, 3.0f)] private float m_Pitch = 1.0f;
    private AudioSource m_AudioSource;

    public AudioClip AudioClip { get { return m_AudioClip; } }
    public float Volume { get { return m_Volume; } }
    public float Pitch { get { return m_Pitch; } }
    public AudioSource AudioSource { get { return m_AudioSource; } set { m_AudioSource = value; } }

    public void Play()
    {
        App.Instance.AudioManager.Play(this);
    }
}
