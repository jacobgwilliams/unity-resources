using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace UnityTemplates.General
{
    [CreateAssetMenu(fileName = "AudioData", menuName = "Unity Templates/General/Audio Data")]
    public class AudioData : ScriptableObject
    {
        [Header("Audio Clips")]
        public AudioClip[] musicTracks;
        public AudioClip[] soundEffects;
        public AudioClip[] ambientSounds;
        
        [Header("Settings")]
        public float masterVolume = 1f;
        public float musicVolume = 0.7f;
        public float sfxVolume = 1f;
        public float ambientVolume = 0.5f;
    }
    
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource ambientSource;
        [SerializeField] private AudioSource uiSource;
        
        [Header("Settings")]
        [SerializeField] private AudioData audioData;
        [SerializeField] private int maxSFXSources = 10;
        [SerializeField] private float fadeTime = 1f;
        
        // Private fields
        private List<AudioSource> sfxSources = new List<AudioSource>();
        private Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();
        private Coroutine musicFadeCoroutine;
        private Coroutine ambientFadeCoroutine;
        
        // Singleton
        private static AudioManager instance;
        public static AudioManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<AudioManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("AudioManager");
                        instance = go.AddComponent<AudioManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }
        
        // Properties
        public float MasterVolume => audioData.masterVolume;
        public float MusicVolume => audioData.musicVolume;
        public float SFXVolume => audioData.sfxVolume;
        public float AmbientVolume => audioData.ambientVolume;
        
        // Events
        public System.Action<float> OnMasterVolumeChanged;
        public System.Action<float> OnMusicVolumeChanged;
        public System.Action<float> OnSFXVolumeChanged;
        public System.Action<float> OnAmbientVolumeChanged;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioSources();
                LoadAudioClips();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            ApplyVolumeSettings();
        }
        
        private void InitializeAudioSources()
        {
            // Create music source if not assigned
            if (musicSource == null)
            {
                GameObject musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                musicSource = musicObj.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }
            
            // Create SFX source if not assigned
            if (sfxSource == null)
            {
                GameObject sfxObj = new GameObject("SFXSource");
                sfxObj.transform.SetParent(transform);
                sfxSource = sfxObj.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }
            
            // Create ambient source if not assigned
            if (ambientSource == null)
            {
                GameObject ambientObj = new GameObject("AmbientSource");
                ambientObj.transform.SetParent(transform);
                ambientSource = ambientObj.AddComponent<AudioSource>();
                ambientSource.loop = true;
                ambientSource.playOnAwake = false;
            }
            
            // Create UI source if not assigned
            if (uiSource == null)
            {
                GameObject uiObj = new GameObject("UISource");
                uiObj.transform.SetParent(transform);
                uiSource = uiObj.AddComponent<AudioSource>();
                uiSource.playOnAwake = false;
            }
            
            // Create additional SFX sources
            for (int i = 0; i < maxSFXSources; i++)
            {
                GameObject sfxObj = new GameObject($"SFXSource_{i}");
                sfxObj.transform.SetParent(transform);
                AudioSource source = sfxObj.AddComponent<AudioSource>();
                source.playOnAwake = false;
                sfxSources.Add(source);
            }
        }
        
        private void LoadAudioClips()
        {
            if (audioData == null) return;
            
            audioClips.Clear();
            
            // Load music tracks
            if (audioData.musicTracks != null)
            {
                foreach (AudioClip clip in audioData.musicTracks)
                {
                    if (clip != null)
                    {
                        audioClips[clip.name] = clip;
                    }
                }
            }
            
            // Load sound effects
            if (audioData.soundEffects != null)
            {
                foreach (AudioClip clip in audioData.soundEffects)
                {
                    if (clip != null)
                    {
                        audioClips[clip.name] = clip;
                    }
                }
            }
            
            // Load ambient sounds
            if (audioData.ambientSounds != null)
            {
                foreach (AudioClip clip in audioData.ambientSounds)
                {
                    if (clip != null)
                    {
                        audioClips[clip.name] = clip;
                    }
                }
            }
        }
        
        private void ApplyVolumeSettings()
        {
            if (audioData == null) return;
            
            musicSource.volume = audioData.musicVolume * audioData.masterVolume;
            sfxSource.volume = audioData.sfxVolume * audioData.masterVolume;
            ambientSource.volume = audioData.ambientVolume * audioData.masterVolume;
            uiSource.volume = audioData.sfxVolume * audioData.masterVolume;
            
            foreach (AudioSource source in sfxSources)
            {
                source.volume = audioData.sfxVolume * audioData.masterVolume;
            }
        }
        
        // Music methods
        public void PlayMusic(string clipName, bool fadeIn = true)
        {
            if (!audioClips.ContainsKey(clipName))
            {
                Debug.LogWarning($"Music clip '{clipName}' not found!");
                return;
            }
            
            if (musicFadeCoroutine != null)
            {
                StopCoroutine(musicFadeCoroutine);
            }
            
            musicSource.clip = audioClips[clipName];
            musicSource.Play();
            
            if (fadeIn)
            {
                musicFadeCoroutine = StartCoroutine(FadeIn(musicSource, fadeTime));
            }
        }
        
        public void StopMusic(bool fadeOut = true)
        {
            if (musicFadeCoroutine != null)
            {
                StopCoroutine(musicFadeCoroutine);
            }
            
            if (fadeOut)
            {
                musicFadeCoroutine = StartCoroutine(FadeOut(musicSource, fadeTime));
            }
            else
            {
                musicSource.Stop();
            }
        }
        
        public void PauseMusic()
        {
            musicSource.Pause();
        }
        
        public void ResumeMusic()
        {
            musicSource.UnPause();
        }
        
        // SFX methods
        public void PlaySFX(string clipName, float volume = 1f, float pitch = 1f)
        {
            if (!audioClips.ContainsKey(clipName))
            {
                Debug.LogWarning($"SFX clip '{clipName}' not found!");
                return;
            }
            
            AudioSource source = GetAvailableSFXSource();
            if (source != null)
            {
                source.clip = audioClips[clipName];
                source.volume = volume * audioData.sfxVolume * audioData.masterVolume;
                source.pitch = pitch;
                source.Play();
            }
        }
        
        public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (clip == null) return;
            
            AudioSource source = GetAvailableSFXSource();
            if (source != null)
            {
                source.clip = clip;
                source.volume = volume * audioData.sfxVolume * audioData.masterVolume;
                source.pitch = pitch;
                source.Play();
            }
        }
        
        public void PlayUISFX(string clipName, float volume = 1f)
        {
            if (!audioClips.ContainsKey(clipName))
            {
                Debug.LogWarning($"UI SFX clip '{clipName}' not found!");
                return;
            }
            
            uiSource.clip = audioClips[clipName];
            uiSource.volume = volume * audioData.sfxVolume * audioData.masterVolume;
            uiSource.Play();
        }
        
        // Ambient methods
        public void PlayAmbient(string clipName, bool fadeIn = true)
        {
            if (!audioClips.ContainsKey(clipName))
            {
                Debug.LogWarning($"Ambient clip '{clipName}' not found!");
                return;
            }
            
            if (ambientFadeCoroutine != null)
            {
                StopCoroutine(ambientFadeCoroutine);
            }
            
            ambientSource.clip = audioClips[clipName];
            ambientSource.Play();
            
            if (fadeIn)
            {
                ambientFadeCoroutine = StartCoroutine(FadeIn(ambientSource, fadeTime));
            }
        }
        
        public void StopAmbient(bool fadeOut = true)
        {
            if (ambientFadeCoroutine != null)
            {
                StopCoroutine(ambientFadeCoroutine);
            }
            
            if (fadeOut)
            {
                ambientFadeCoroutine = StartCoroutine(FadeOut(ambientSource, fadeTime));
            }
            else
            {
                ambientSource.Stop();
            }
        }
        
        // Volume control methods
        public void SetMasterVolume(float volume)
        {
            audioData.masterVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
            OnMasterVolumeChanged?.Invoke(audioData.masterVolume);
        }
        
        public void SetMusicVolume(float volume)
        {
            audioData.musicVolume = Mathf.Clamp01(volume);
            musicSource.volume = audioData.musicVolume * audioData.masterVolume;
            OnMusicVolumeChanged?.Invoke(audioData.musicVolume);
        }
        
        public void SetSFXVolume(float volume)
        {
            audioData.sfxVolume = Mathf.Clamp01(volume);
            sfxSource.volume = audioData.sfxVolume * audioData.masterVolume;
            uiSource.volume = audioData.sfxVolume * audioData.masterVolume;
            
            foreach (AudioSource source in sfxSources)
            {
                source.volume = audioData.sfxVolume * audioData.masterVolume;
            }
            
            OnSFXVolumeChanged?.Invoke(audioData.sfxVolume);
        }
        
        public void SetAmbientVolume(float volume)
        {
            audioData.ambientVolume = Mathf.Clamp01(volume);
            ambientSource.volume = audioData.ambientVolume * audioData.masterVolume;
            OnAmbientVolumeChanged?.Invoke(audioData.ambientVolume);
        }
        
        // Utility methods
        private AudioSource GetAvailableSFXSource()
        {
            foreach (AudioSource source in sfxSources)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }
            
            // If no available source, use the first one (will interrupt)
            return sfxSources.Count > 0 ? sfxSources[0] : sfxSource;
        }
        
        private IEnumerator FadeIn(AudioSource source, float duration)
        {
            float startVolume = 0f;
            float targetVolume = source.volume;
            
            source.volume = startVolume;
            
            while (source.volume < targetVolume)
            {
                source.volume += Time.deltaTime * (targetVolume / duration);
                yield return null;
            }
            
            source.volume = targetVolume;
        }
        
        private IEnumerator FadeOut(AudioSource source, float duration)
        {
            float startVolume = source.volume;
            
            while (source.volume > 0f)
            {
                source.volume -= Time.deltaTime * (startVolume / duration);
                yield return null;
            }
            
            source.Stop();
            source.volume = startVolume;
        }
        
        public void StopAllSFX()
        {
            sfxSource.Stop();
            uiSource.Stop();
            
            foreach (AudioSource source in sfxSources)
            {
                source.Stop();
            }
        }
        
        public void StopAllAudio()
        {
            StopMusic();
            StopAmbient();
            StopAllSFX();
        }
        
        public bool IsMusicPlaying()
        {
            return musicSource.isPlaying;
        }
        
        public bool IsAmbientPlaying()
        {
            return ambientSource.isPlaying;
        }
    }
}