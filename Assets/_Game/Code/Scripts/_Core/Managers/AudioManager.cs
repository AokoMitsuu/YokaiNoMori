using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace Managers
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Items")]
        [SerializeField] private AudioMixerGroup m_SFXAudioMixer;
        [SerializeField] private AudioMixerGroup m_MusicAudioMixer;

        [Header("Parameters")]
        [SerializeField] private float m_MusicEndFadeDuration = 1.5f;

        private List<MusicSo> m_CurrentMusics = new();

        public void Play(SoundSo pSoundSo)
        {
            if (pSoundSo.AudioSource == null)
            {
                pSoundSo.AudioSource = gameObject.AddComponent<AudioSource>();
                pSoundSo.AudioSource.clip = pSoundSo.AudioClip;
                pSoundSo.AudioSource.volume = pSoundSo.Volume;
                pSoundSo.AudioSource.loop = false;
                pSoundSo.AudioSource.pitch = pSoundSo.Pitch;
                pSoundSo.AudioSource.outputAudioMixerGroup = m_SFXAudioMixer;
            }

            pSoundSo.AudioSource.Play();
        }

        public void Play(MusicSo pMusicSo)
        {
            if (pMusicSo.AudioSource == null)
            {
                pMusicSo.AudioSource = gameObject.AddComponent<AudioSource>();
                pMusicSo.AudioSource.clip = pMusicSo.AudioClip;
                pMusicSo.AudioSource.volume = pMusicSo.Volume;
                pMusicSo.AudioSource.loop = pMusicSo.Loop;
                pMusicSo.AudioSource.outputAudioMixerGroup = m_MusicAudioMixer;
            }

            pMusicSo.AudioSource.Play();
            m_CurrentMusics.Add(pMusicSo);
            pMusicSo.AudioSource.DOFade(pMusicSo.Volume, m_MusicEndFadeDuration);
        }

        public async void StopMusic(MusicSo pMusicSo)
        {
            if (m_CurrentMusics.Contains(pMusicSo) && pMusicSo.AudioSource.isPlaying)
            {
                await pMusicSo.AudioSource.DOFade(0, m_MusicEndFadeDuration).AsyncWaitForCompletion();
                pMusicSo.AudioSource.Stop();
                m_CurrentMusics.Remove(pMusicSo);
            }
        }

        public void StopAllMusics()
        {
            if (m_CurrentMusics.Count == 0) { return; }

            foreach (MusicSo musicSo in m_CurrentMusics)
            {
                StopMusic(musicSo);
            }
        }

        public void StopAllMusicsExcept(MusicSo[] pMusicSo)
        {
            if (m_CurrentMusics.Count == 0) { return; }

            List<MusicSo> songsToStop = m_CurrentMusics.Where(currentSound => !pMusicSo.Contains(currentSound)).ToList();

            foreach (MusicSo musicSo in songsToStop)
            {
                StopMusic(musicSo);
            }
        }

        public bool IsPlaying(MusicSo pMusicSo)
        {
            return m_CurrentMusics.Contains(pMusicSo);
        }

        protected void OnApplicationQuit()
        {
            m_CurrentMusics.Clear();
        }
    }
}