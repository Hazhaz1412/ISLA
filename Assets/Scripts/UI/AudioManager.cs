using UnityEngine;
using UnityEngine.Audio;

namespace MadKnight.UI
{
    /// <summary>
    /// Audio Manager - Quản lý âm thanh toàn game
    /// Singleton pattern
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        
        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer audioMixer;
        
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource ambienceSource;
        
        // Mixer parameter names
        private const string MASTER_VOLUME = "MasterVolume";
        private const string MUSIC_VOLUME = "MusicVolume";
        private const string SFX_VOLUME = "SFXVolume";
        private const string AMBIENCE_VOLUME = "AmbienceVolume";
        
        private void Awake()
        {
            // Singleton
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            // Create audio sources if not assigned
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }
            
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }
            
            if (ambienceSource == null)
            {
                ambienceSource = gameObject.AddComponent<AudioSource>();
                ambienceSource.loop = true;
                ambienceSource.playOnAwake = false;
            }
        }
        
        #region Volume Control
        
        public void SetMasterVolume(float volume)
        {
            if (audioMixer != null)
            {
                // Convert 0-1 to decibels (-80 to 0)
                float db = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
                audioMixer.SetFloat(MASTER_VOLUME, db);
            }
        }
        
        public void SetMusicVolume(float volume)
        {
            if (audioMixer != null)
            {
                float db = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
                audioMixer.SetFloat(MUSIC_VOLUME, db);
            }
            else if (musicSource != null)
            {
                musicSource.volume = volume;
            }
        }
        
        public void SetSFXVolume(float volume)
        {
            if (audioMixer != null)
            {
                float db = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
                audioMixer.SetFloat(SFX_VOLUME, db);
            }
            else if (sfxSource != null)
            {
                sfxSource.volume = volume;
            }
        }
        
        public void SetAmbienceVolume(float volume)
        {
            if (audioMixer != null)
            {
                float db = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
                audioMixer.SetFloat(AMBIENCE_VOLUME, db);
            }
            else if (ambienceSource != null)
            {
                ambienceSource.volume = volume;
            }
        }
        
        #endregion
        
        #region Play Audio
        
        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (musicSource == null || clip == null) return;
            
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }
        
        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
            }
        }
        
        public void PlaySFX(AudioClip clip)
        {
            if (sfxSource == null || clip == null) return;
            sfxSource.PlayOneShot(clip);
        }
        
        public void PlayAmbience(AudioClip clip, bool loop = true)
        {
            if (ambienceSource == null || clip == null) return;
            
            ambienceSource.clip = clip;
            ambienceSource.loop = loop;
            ambienceSource.Play();
        }
        
        public void StopAmbience()
        {
            if (ambienceSource != null)
            {
                ambienceSource.Stop();
            }
        }
        
        #endregion
    }
}
