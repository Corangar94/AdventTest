using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages audio for the application
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    
    [SerializeField] private AudioClip shapeChangeSound;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip rotationSound;
    [SerializeField] private AudioClip scalingSound;
    
    // Dictionary to cache audio clips
    private Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Create audio sources if they don't exist
            if (musicSource == null)
            {
                GameObject musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                musicSource = musicObj.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.volume = 0.5f;
            }
            
            if (sfxSource == null)
            {
                GameObject sfxObj = new GameObject("SFXSource");
                sfxObj.transform.SetParent(transform);
                sfxSource = sfxObj.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.volume = 1.0f;
            }
            
            // Load default sounds
            if (shapeChangeSound == null)
            {
                shapeChangeSound = CreateDefaultAudioClip("shapeChange", 440, 0.3f); // A note
            }
            
            if (buttonClickSound == null)
            {
                buttonClickSound = CreateDefaultAudioClip("buttonClick", 330, 0.1f); // E note
            }
            
            if (rotationSound == null)
            {
                rotationSound = CreateDefaultAudioClip("rotation", 550, 0.2f, true); // C# note with sweep
            }
            
            if (scalingSound == null)
            {
                scalingSound = CreateDefaultAudioClip("scaling", 660, 0.2f, true); // E note with sweep
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private AudioClip CreateDefaultAudioClip(string name, float frequency, float duration, bool useFrequencySweep = false)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.FloorToInt(duration * sampleRate);
        
        AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
        
        float[] samples = new float[sampleCount];
        
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            
            // Frequency sweep for more interesting sounds
            float currentFrequency = useFrequencySweep ? 
                frequency * (1f + 0.2f * Mathf.Sin(t * 10f)) : // Add modulation for rotation/scaling sounds
                frequency;
                
            // Simple sine wave with envelope
            float envelope = t < duration * 0.1f ? t / (duration * 0.1f) : (1 - (t - duration * 0.1f) / (duration * 0.9f));
            samples[i] = Mathf.Sin(2 * Mathf.PI * currentFrequency * t) * envelope;
        }
        
        clip.SetData(samples, 0);
        audioClips[name] = clip;
        
        return clip;
    }
    
    public void PlayShapeChangeSound()
    {
        if (shapeChangeSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(shapeChangeSound);
        }
    }
    
    public void PlayButtonClickSound()
    {
        if (buttonClickSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(buttonClickSound);
        }
    }
    
    public void PlayRotationSound()
    {
        if (rotationSound != null && sfxSource != null)
        {
            // Use lower volume for rotation sound as it might be played frequently
            sfxSource.PlayOneShot(rotationSound, 0.6f);
        }
    }
    
    public void PlayScalingSound()
    {
        if (scalingSound != null && sfxSource != null)
        {
            // Use lower volume for scaling sound as it might be played frequently
            sfxSource.PlayOneShot(scalingSound, 0.7f);
        }
    }
    
    public void PlaySound(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
    
    public void PlaySound(string clipName)
    {
        if (audioClips.TryGetValue(clipName, out AudioClip clip))
        {
            PlaySound(clip);
        }
    }
    
    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = Mathf.Clamp01(volume);
        }
    }
    
    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
        {
            sfxSource.volume = Mathf.Clamp01(volume);
        }
    }
} 